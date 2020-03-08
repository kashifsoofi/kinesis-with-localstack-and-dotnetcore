namespace Consumer.Host
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Producer.Contracts;

    public class ProducerEventHandler : IHandleMessages<ProducerEvent>
    {
        static ILog log = LogManager.GetLogger<ProducerEventHandler>();

        public Task Handle(ProducerEvent message, IMessageHandlerContext context)
        {
            log.Info($"ProducerEvent received with description: {message.Message}");
            return Task.CompletedTask;
        }
    }
}