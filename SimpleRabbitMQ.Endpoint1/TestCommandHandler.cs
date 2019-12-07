using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint1
{
    public class TestCommandHandler : IHandleMessages<TestCommand>
    {
        private static readonly ILog Log = LogManager.GetLogger<TestCommandHandler>();

        public async Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            //https://docs.microsoft.com/en-us/dotnet/framework/data/transactions/implementing-an-implicit-transaction-using-transaction-scope
            //TODO: default is serializable, which is not what I want, I want ReadCommitted
            //TransactionScopeOption.Suppress should prevent the transaction to enlist as a distributed/ambient transaction
            //override IsolationLevel to be ReadCommited (default iso level of TransactionScope is Serializable)

            #region new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted })):
            //so, interesting run time exception thrown when using TransactionScope with these constructor args:
            //2019 - 11 - 24 12:45:51.163 INFO SimpleRabbitMQ.Endpoint1.TestCommandHandler Hello from TestCommandHandler
            //2019 - 11 - 24 12:45:59.347 WARN NServiceBus.ForceImmediateDispatchForOperationsInSuppressedScopeBehavior
            //    Suppressed ambient transaction detected when requesting the outgoing operation.
            //    Support for this behavior is deprecated and will be removed in Version 7.The new api for requesting immediate dispatch is
            //var options = new Send| Publish | ReplyOptions()
            //options.RequireImmediateDispatch()
            //session.Send | Publish | Reply(new MyMessage(), options)
            #endregion

            #region using (var scope = new TransactionScope()){}
            //default TransactionScopeOption is TransactionScopeOption.Required, and default IsolationLevel is ReadCommitted
            //ok, using this option threw the following excpetion:
            //2019 - 11 - 24 12:53:42.515 INFO NServiceBus.RecoverabilityExecutor Immediate Retry is going to retry message '590f033d-edac-4fcc-9e60-ab0f0124ba41' because of an exception:
            //System.InvalidOperationException: A TransactionScope must be disposed on the same thread that it was created
            //MIKE: this might have to do with AsyncFlow?
            #endregion

            #region Daniel Marbach's series of posts on TransactionScope and Async/Await
            //https://www.planetgeek.ch/2014/12/07/participating-in-transactionscopes-and-asyncawait-introduction/
            //https://www.planetgeek.ch/2014/12/16/participating-in-transactionscopes-and-asyncawait-going-deep-into-the-abyss/
            //https://www.planetgeek.ch/2015/03/16/participating-in-transactionscopes-and-asyncawait-alone-in-the-dark/
            //https://www.planetgeek.ch/2015/03/20/participating-in-transactionscopes-and-asyncawait-lets-get-the-money-back/
            #endregion
            //when new'ing up a TransactionScope using TransactionScopeAsyncFlowOption.Enabled, the Isoluation level is ReadCommitted, which is what we want
            //however, the TransactionScopeOption is Required, which might be a problem b/c of this command next to that option type in the decompiled code:
            //A transaction is required by the scope. It uses an ambient transaction if one already exists. Otherwise, it creates a new transaction before entering the scope. This is the default value.
            //so, either it uses and existing ambient transaction (which we don't want, b/c an ambient transaction I THINK is basically going to be bumped to a distributed transaction). So it either enlists in current or creates new, both of which option we want suppress, not require

            #region new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted })
            //this is what Crb's TransactionScopeBuilder uses to create a transaction scope. Problem here is this doesn't support async
            #endregion

            //TODO: find out why when using the MSMQ transport when using TransactionScope like this (aka, not AsyncFlowOption.enabled):
            //using (var scope = new TransactionScope())
            //the EP does NOT throw this exception: System.InvalidOperationException: A TransactionScope must be disposed on the same thread that it was created.
            //(when using the RabbitMQ transport, this exception is throw)

            //12/2: after talking to Indu and Sean at Particular software today, putting bus ops in TransactionScope does NOTHING
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                Log.Info($"TestCommandHandler. OrderId: {message.OrderId}");

                const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=SimpleRabbitMQ; Integrated Security=True;";
                var  sql = $"INSERT INTO [dbo].[TestCommandHandler] ([OrderId]) VALUES ('{message.OrderId}');";
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                //with exception thrown here, the outgoing publish is NOT sent
                //HOWEVER, TransactionScope does do something here, but it has nothing to do with messaging. With or without TransactionScope, the message is not published
                //with TransactionScope, the db rolls back properly (aka, there is no record in TestCommandHandler table)
                //WITHOUT TransactionScope, the db does NOT roll back properly. Even though an exception if thrown after .ExecuteNonQuery and the handler retries,
                //the row is still written to the db.
                //SO, TransactionScope does do something here, but it's in the context of the db rollback, nothing to do with messaging
                //my best guess as to why is the db operation is not rolling back is lack of a try/catch block when not using TransactionScope?
                //- or, my understanding of ADO.NET is so outdated that transactions need to manually handle/be assigned to the open connection in order for the operation to participate in the handler's ambient transaction
                //throw new Exception("boom!");

                //var options = new PublishOptions();
                //options.RequireImmediateDispatch();
                //await context.Publish(new TestEvent { OrderId = message.OrderId }, options);

                await context.Publish(new TestEvent { OrderId = message.OrderId });

                //with an exception thrown here, the outgoing publish does NOT happen (this is the publish call with RequireImmediateDispatch off and TransactionScope on)
                //interesting   , when RequireImmediateDispatch is on, and TransactionScope is on, then the outgoing publish DOES happen! WTF!
                //- AND, the event handler is writing to the database!
                //- maybe b/c the event handler's db ops are not in a TransactionScope?
                //TODO: test this with TransactionScope off and RequireImmediateDispatch on
                //throw new Exception("boom!");

                scope.Complete();
            }
        }
    }
}