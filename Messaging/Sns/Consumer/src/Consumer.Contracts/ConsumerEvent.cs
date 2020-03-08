namespace Consumer.Contracts
{
    using NServiceBus;
    using System;

    public class ConsumerEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
}