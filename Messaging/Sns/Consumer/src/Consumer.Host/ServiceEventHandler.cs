namespace Consumer.Host
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Producer.Contracts;

    public class ServiceEventHandler : IHandleMessages<ServiceEvent>
    {
        static ILog log = LogManager.GetLogger<ServiceEventHandler>();

        public Task Handle(ServiceEvent message, IMessageHandlerContext context)
        {
            log.Info($"ServiceEvent received with eventType: {message.EventType}");
            return Task.CompletedTask;
        }
    }
}