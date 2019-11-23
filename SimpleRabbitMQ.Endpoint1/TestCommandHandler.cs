using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint1
{
    public class TestCommandHandler : IHandleMessages<TestCommand>
    {
        private static readonly ILog Log = LogManager.GetLogger<TestCommandHandler>();

        public Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            Log.Info("Hello from TestCommandHandler");
            
            //commenting out the RequireImmediateDispatch had a significant impact on now many req per second went through both WebAPI (via West Wind Web Surge)
            //and the amount of messages per second both ep's could process. About 23 req per sec vs. 208 req per sec. Significant
            //var options = new PublishOptions();
            //options.RequireImmediateDispatch();
            //return context.Publish(new TestEvent(), options);
            
            return context.Publish(new TestEvent());
        }
    }
}