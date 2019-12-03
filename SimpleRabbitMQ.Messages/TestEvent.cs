using System;
using NServiceBus;

namespace SimpleRabbitMQ.Messages
{
    public class TestEvent : IEvent
    {
        public Guid OrderId { get; set; }
    }
}