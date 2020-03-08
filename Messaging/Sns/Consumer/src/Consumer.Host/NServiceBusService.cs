namespace Consumer.Host
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.SQS;
    using Microsoft.Extensions.Hosting;
    using NServiceBus;
    using NServiceBus.Logging;
    using Producer.Contracts;

    public class NServiceBusService : IHostedService
    {
        private IEndpointInstance endpointInstance;

        public IMessageSession MessageSession { get; internal set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpointConfiguration = ConfigureEndpoint();

            endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            MessageSession = endpointInstance;

            await RegisterServiceEventListener();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (endpointInstance != null)
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }

        private EndpointConfiguration ConfigureEndpoint()
        {
            LogManager.Use<DefaultFactory>()
                .Level(LogLevel.Info);

            var endpointConfiguration = new EndpointConfiguration("consumer-host");
            endpointConfiguration.DoNotCreateQueues();

            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                new AmazonSQSConfig
                {
                    ServiceURL = "http://localhost:4576"
                }));

            var s3Configuration = transport.S3("bucketname", "consumer-host");
            s3Configuration.ClientFactory(() => new AmazonS3Client(
                new AnonymousAWSCredentials(),
                new AmazonS3Config
                {
                    ServiceURL = "http://localhost:4572"
                }));

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            var pipeline = endpointConfiguration.Pipeline;
            pipeline.Register(
                behavior: new ServiceEventListenerBehavior(),
                description: "Publish message to sns");

            return endpointConfiguration;
        }

        private async Task RegisterServiceEventListener()
        {
            var serviceEventTypes = new List<Type>
            {
                typeof(ProducerEvent),
            };

            var sqsClient = new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                new AmazonSQSConfig
                {
                    ServiceURL = "http://localhost:4576"
                });

            var sqsQueueName = "consumer-host";
            var getQueueUrlResponse = await sqsClient.GetQueueUrlAsync(sqsQueueName);


            var snsService = new SnsService();
            foreach (var serviceEventType in serviceEventTypes)
            {
                var topicName = serviceEventType.FullName.Replace(".", "-").ToLower();
                var topicArn = await snsService.GetOrCreateTopicArn(topicName);

                await snsService.SubscribeAsync(topicArn, sqsClient, getQueueUrlResponse.QueueUrl);
            }
        }
    }
}
