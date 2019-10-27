using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Amazon;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Demo.Domain;
using Microsoft.Extensions.Hosting;

namespace Demo.Producer
{
    public class MessageProducerService : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly AmazonKinesisClient _kinesisClient;

        private const string _streamName = "demo-stream";
        private const int TotalMessagesToCreate = 100;

        private int _messageCounter = 1;

        public MessageProducerService()
        {
            _kinesisClient = new AmazonKinesisClient(RegionEndpoint.EUWest1);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Timed Message Producer Background Service is starting.");

            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Console.WriteLine("Timed Message Producer Background Service is started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Timed Message Producer Background Service is stopping.");

            _timer?.Stop();

            Console.WriteLine("Timed Message Producer Background Service is stopped.");
            return Task.CompletedTask;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var dataMessage = new DataMessage
            {
                Id = Guid.NewGuid(),
                CreatedOn = e.SignalTime,
            };

            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<DataMessage>(dataMessage));
            var putRecordRequest = new PutRecordRequest
            {
                StreamName = _streamName,
                Data = new MemoryStream(messageBytes),
                PartitionKey = "demo-partition",
            };

            var putRecordResponse = _kinesisClient.PutRecordAsync(putRecordRequest).GetAwaiter().GetResult();
            Console.WriteLine($"Successfully putrecord {_messageCounter}:\n\t partition key = {putRecordRequest.PartitionKey}, shard ID = {putRecordResponse.ShardId}");

            _messageCounter++;
            if (_messageCounter <= TotalMessagesToCreate)
            {
                _timer.Stop();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
