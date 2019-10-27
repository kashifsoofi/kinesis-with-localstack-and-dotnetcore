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

namespace Demo.Consumer
{
    public class MessageConsumerService : BackgroundService
    {
        private readonly AmazonKinesisClient _kinesisClient;

        private const string _streamName = "demo-stream";

        public MessageConsumerService()
        {
            _kinesisClient = new AmazonKinesisClient(
                new AnonymousAWSCredentials(),
                new AmazonKinesisConfig
                {
                    ServiceURL = "http://localstack:4568",
                    RegionEndpoint = RegionEndpoint.EUWest1,
                });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ReadFromStream();
        }

        private async Task ReadFromStream()
        {
            var describeRequest = new DescribeStreamRequest
            {
                StreamName = _streamName,
            };

            var describeStreamResponse = await _kinesisClient.DescribeStreamAsync(describeRequest);
            var shards = describeStreamResponse.StreamDescription.Shards;
            foreach (var shard in shards)
            {
                var getShardIteratorRequest = new GetShardIteratorRequest
                {
                    StreamName = _streamName,
                    ShardId = shard.ShardId,
                    ShardIteratorType = ShardIteratorType.TRIM_HORIZON,
                };

                var getShardIteratorResponse = await _kinesisClient.GetShardIteratorAsync(getShardIteratorRequest);
                var shardIterator = getShardIteratorResponse.ShardIterator;
                while (!string.IsNullOrEmpty(shardIterator))
                {
                    var getRecordsRequest = new GetRecordsRequest
                    {
                        Limit = 100,
                        ShardIterator = shardIterator,
                    };

                    var getRecordsResponse = await _kinesisClient.GetRecordsAsync(getRecordsRequest);
                    var nextIterator = getRecordsResponse.NextShardIterator;
                    var records = getRecordsResponse.Records;

                    if (records.Count > 0)
                    {
                        Console.WriteLine($"Received {records.Count} records.");
                        foreach (var record in records)
                        {
                            var dataMessage = await JsonSerializer.DeserializeAsync<DataMessage>(record.Data);
                            Console.WriteLine($"DataMessage Id={dataMessage.Id}, CreatedOn={dataMessage.CreatedOn.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                    shardIterator = nextIterator;
                }
            }
        }
    }
}
