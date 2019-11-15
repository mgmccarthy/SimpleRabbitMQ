using System;
using System.Collections.Generic;
using NServiceBus;

namespace SimpleRabbitMQ.Messages
{
    public class TestCommand : ICommand
    {
        public Guid OrderId { get; set; }
        public string ProductName { get; set; }
        public List<string> Descriptions { get; set; }
    }
}