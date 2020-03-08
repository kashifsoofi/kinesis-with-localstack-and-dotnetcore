namespace Producer.Contracts
{
    using System;

    public class ServiceEvent
    {
        public string EventType { get; set; }

        public object Event { get; set; }
    }
}
