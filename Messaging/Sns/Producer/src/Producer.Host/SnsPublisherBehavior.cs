namespace Producer.Host
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using Producer.Contracts;

    public class SnsPublisherBehavior : Behavior<IOutgoingLogicalMessageContext>
    {
        public SnsPublisherBehavior()
        {

        }

        public override async Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            var eventType = context.Message.MessageType.FullName;
            var serviceEvent = new ServiceEvent
            {
                EventType = eventType,
                Event = context.Message.Instance
            };

            var topicName = eventType.Replace(".", "-").ToLower();
            var message = JsonSerializer.Serialize(serviceEvent);

            var snsService = new SnsService();
            var topicArn = await snsService.GetOrCreateTopicArn(topicName);
            await snsService.PublishAsync(topicArn, message);

            await next().ConfigureAwait(false);
        }
    }
}
