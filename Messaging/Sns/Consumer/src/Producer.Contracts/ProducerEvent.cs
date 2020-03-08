namespace Producer.Contracts
{
    using System;
    using NServiceBus;

    public class ProducerEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
}