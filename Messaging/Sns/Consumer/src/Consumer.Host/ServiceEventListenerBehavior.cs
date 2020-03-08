namespace Consumer.Host
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using Producer.Contracts;

    public class ServiceEventListenerBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        public ServiceEventListenerBehavior()
        {
        }

        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            if (context.Message.MessageType == typeof(ServiceEvent))
            {
                var serviceEvent = context.Message.Instance as ServiceEvent;

            }

            await next().ConfigureAwait(false);
        }
    }
}
