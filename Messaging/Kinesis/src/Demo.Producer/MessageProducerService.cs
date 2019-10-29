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
using Amazon.Runtime;
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
            var serverName = "localstack";
            _kinesisClient = new AmazonKinesisClient(
                "DUMMY_KEY",
                "DUMMY_KEY",
                new AmazonKinesisConfig
                {
                    ServiceURL = $"http://{serverName}:4568",
                });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MessageProducer service is starting.");

            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Console.WriteLine("MessageProducer service is started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MessageProducer service is stopping.");

            _timer?.Stop();

            Console.WriteLine("MessageProducer service is stopped.");
            return Task.CompletedTask;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("MessageProducer service on timed event starting.");

            var dataMessage = new DataMessage
            {
                Id = Guid.NewGuid(),
                CreatedOn = DateTime.UtcNow,
            };

            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<DataMessage>(dataMessage));
            var putRecordRequest = new PutRecordRequest
            {
                StreamName = _streamName,
                Data = new MemoryStream(messageBytes),
                PartitionKey = "demo-partition",
            };

            try
            {
                var putRecordResponse = _kinesisClient.PutRecordAsync(putRecordRequest).GetAwaiter().GetResult();
                Console.WriteLine($"Successfully putrecord number={_messageCounter}{Environment.NewLine}PartitionKey={putRecordRequest.PartitionKey}, ShardId={putRecordResponse.ShardId}{Environment.NewLine}DataMessage Id={dataMessage.Id}, CreatedOn={dataMessage.CreatedOn.ToString("yyyy-MM-dd HH:mm")}");

                _messageCounter++;
                if (_messageCounter >= TotalMessagesToCreate)
                {
                    _timer.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception: {ex.Message}");
            }

            Console.WriteLine("MessageProducer service on timed event finishing.");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
