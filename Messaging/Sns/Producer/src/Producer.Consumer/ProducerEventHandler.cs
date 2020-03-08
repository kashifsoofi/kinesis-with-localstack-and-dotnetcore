namespace Producer.Consumer
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using Producer.Contracts;

    public class ProducerEventHandler : IHandleMessages<ProducerEvent>
    {
        public Task Handle(ProducerEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine($"{message.Id}");
            return Task.CompletedTask;
        }
    }
}
