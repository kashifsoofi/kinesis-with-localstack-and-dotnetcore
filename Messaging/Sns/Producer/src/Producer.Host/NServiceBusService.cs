namespace Producer.Host
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.SQS;
    using Microsoft.Extensions.Hosting;
    using NServiceBus;
    using Producer.Contracts;

    public class NServiceBusService : IHostedService
    {
        private Timer timer;
        private IEndpointInstance endpointInstance;

        public IMessageSession MessageSession { get; internal set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpointConfiguration = ConfigureEndpoint();

            endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            MessageSession = endpointInstance;

            timer = new Timer(DoWork, null, 10 * 1000, Timeout.Infinite);
        }

        private async void DoWork(object state)
        {
            var id = Guid.NewGuid();
            var producerEvent = new ProducerEvent
            {
                Id = id,
                Message = id.ToString("N"),
            };
            await endpointInstance.Publish(producerEvent).ConfigureAwait(false);

            Console.WriteLine("Do you want to continue timer? (Y/N)");
            var consoleKeyInfo = Console.ReadKey();
            if (consoleKeyInfo.Key == ConsoleKey.Y)
            {
                timer?.Change(10 * 1000, Timeout.Infinite);
            }
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
            var endpointConfiguration = new EndpointConfiguration("producer-host");
            endpointConfiguration.DoNotCreateQueues();

            var serverName = "localhost";
            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(
                new AnonymousAWSCredentials(),
                new AmazonSQSConfig
                {
                    ServiceURL = $"http://{serverName}:4576"
                }));

            var s3Configuration = transport.S3("bucketname", "producer-host");
            s3Configuration.ClientFactory(() => new AmazonS3Client(
                new AnonymousAWSCredentials(),
                new AmazonS3Config
                {
                    ServiceURL = $"http://{serverName}:4572",
                    ForcePathStyle = true,
                }));

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            var pipeline = endpointConfiguration.Pipeline;
            pipeline.Register(
                behavior: new SnsPublisherBehavior(),
                description: "Publish message to sns");

            return endpointConfiguration;
        }
    }
}
