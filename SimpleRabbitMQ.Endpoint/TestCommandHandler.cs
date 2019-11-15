using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint
{
    public class TestCommandHandler : IHandleMessages<TestCommand>
    {
        static ILog log = LogManager.GetLogger<TestCommandHandler>();

        public Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            log.Info("Hello from TestCommandHandler");
            //return context.Publish(new TestEvent());
            return Task.CompletedTask;
        }
    }
}