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
            if (context.Message.Instance is ServiceEvent serviceEvent)
            {
                context.UpdateMessageInstance(serviceEvent.Event);
            }

            await next().ConfigureAwait(false);
        }
    }
}
