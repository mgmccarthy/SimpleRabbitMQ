using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint
{
    public class TestCommandHandler : IHandleMessages<TestCommand>
    {
        static readonly ILog log = LogManager.GetLogger<TestCommandHandler>();

        public Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            log.Info("Hello from TestCommandHandler");
            //commenting this out until I can figure out how to set up Rabbit so an the published TestEvent can find it's way to it's subscriber in Endpoint2
            //return context.Publish(new TestEvent());
            return Task.CompletedTask;
        }
    }
}