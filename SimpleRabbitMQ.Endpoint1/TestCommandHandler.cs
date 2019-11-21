using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint1
{
    public class TestCommandHandler : IHandleMessages<TestCommand>
    {
        static readonly ILog log = LogManager.GetLogger<TestCommandHandler>();

        public Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            log.Info("Hello from TestCommandHandler");

            //commenting this out until I can figure out how to set up Rabbit so an the published TestEvent can find it's way to it's subscriber in Endpoint2
            //UPDATE: so, this must have something to do with the underlying topology that RabbitMQ creates. I blew all queues and exchanges away, rebuilt, than ran from scratch and it looks like now the publishing handler (this one) now longer throws and exception, and the event is handled in Endpoint2 accordingly
            var options = new PublishOptions();
            options.RequireImmediateDispatch();
            return context.Publish(new TestEvent(), options);
            //return Task.CompletedTask;

            //this SHOULD simulate the "ack" for the transport failing and allowing the event to "escape", aka, ghost messages
            //throwing an exception here never let the TestEvent event get published with outbox off
            //throw new Exception();
        }
    }
}