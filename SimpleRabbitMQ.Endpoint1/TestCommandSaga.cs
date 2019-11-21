namespace SimpleRabbitMQ.Endpoint1
{
    //public class TestCommandSaga : Saga<TestCommandSaga.TestCommandSagaData>,
    //    IAmStartedByMessages<TestCommand>,
    //    IHandleTimeouts<TestCommandSaga.TimeoutState>
    //{
    //    static ILog log = LogManager.GetLogger<TestCommandSaga>();

    //    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestCommandSagaData> mapper)
    //    {
    //        mapper.ConfigureMapping<TestCommand>(message => message.OrderId).ToSaga(sagaData => sagaData.OrderId);
    //    }

    //    public Task Handle(TestCommand message, IMessageHandlerContext context)
    //    {
    //        //var nhibernateSession = context.SynchronizedStorageSession.Session();
    //        //nhibernateSession.Save()

    //        log.Info("Handling TestCommand");
    //        Data.ProductName = message.ProductName;
    //        Data.Descriptions = message.Descriptions;
    //        return RequestTimeout<TimeoutState>(context, TimeSpan.FromSeconds(10));
    //    }

    //    public Task Timeout(TimeoutState state, IMessageHandlerContext context)
    //    {
    //        log.Info("Timeout Fired.");
    //        return context.Publish(new TestEvent());
    //    }

    //    public class TestCommandSagaData : ContainSagaData
    //    {
    //        public virtual Guid OrderId { get; set; }
    //        public virtual string ProductName { get; set; }
    //        public virtual IList<string> Descriptions { get; set; }
    //    }

    //    public class TimeoutState { }
    //}
}