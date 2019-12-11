using System.Data;
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
            #region shit ton of comments

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
            #endregion

            Log.Info($"TestCommandHandler.OrderId: {message.OrderId}");
            var sql = $"INSERT INTO [dbo].[TestCommandHandler] ([OrderId]) VALUES ('{message.OrderId}');";
            
            //this is the way to get to the underlying ADO.NET IDBConnection that NHibernate is using
            var session = context.SynchronizedStorageSession.Session();

            await UseNHibernatesSessionToExecuteRawSqlAgainstDb(session, sql, context, message);
            //await UseNHibernatesSessionToExecuteAdoCalls(session, sql, context, message);
            //UseSyncStorageContextToGetIDbConnectionFromNHibernateAndUseThatConnectionToExecuteAdoDbOps(session, sql);
            //await TransactionScopeAndThrowingExceptionsNoOutbox(message, context, sql);
        }

        public Task UseNHibernatesSessionToExecuteRawSqlAgainstDb(NHibernate.ISession session, string sql, IMessageHandlerContext context, TestCommand message)
        {
            //https://stackoverflow.com/questions/36386613/using-createsqlquery-with-insert-query-in-nhibernate
            //so, it looks like if you use SynchronizedStorageSession in handler code, and you enable Outbox, then Outbox's transaction and any business data written using the synche'd session are automatically included in the same transaction:
            //https://discuss.particular.net/t/outbox-questions/1201/2?u=mgmccarthy
            //aka, you get the Outbox and biz data participating in the same transation without having to float the currently active connection and transation into handler code
            session.CreateSQLQuery(sql).ExecuteUpdate();
            return context.Publish(new TestEvent { OrderId = message.OrderId });
        }

        public async Task UseNHibernatesSessiontoExecuteAdoCalls(NHibernate.ISession session, string sql, IMessageHandlerContext context, TestCommand message)
        {
            using (var transaction = session.BeginTransaction())
            {
                var command = session.Connection.CreateCommand();
                command.Connection = session.Connection;

                transaction.Enlist(command);

                command.CommandText = sql;
                command.ExecuteNonQuery();

                await context.Publish(new TestEvent { OrderId = message.OrderId });

                //so, this is failing, b/c I think the Outbox expects to close/commit the transaction then connection. Here, I'm forcing that, and not giving Outbox a AdoTransation to commit
                //not too sure what to do here... the quickest/easiest/guess thing is going to be that I just don't have to call .Committ(), b/c the Outbox will take of it for me?
                //TODO: find out
                transaction.Commit();
            }
        }

        public void UseSyncStorageContextToGetIDbConnectionFromNHibernateAndUseThatConnectionToExecuteAdoDbOps(NHibernate.ISession session, string sql)
        {
            //IDbCommand, this is what will be required by db access like Dapper and SqlBulkCopy
            var command = session.Connection.CreateCommand();

            //IDbTransation
            //problem here is there is no way to "get" the current IDbTransaction from NHibernate's transaction AND, there is no way to "get" the IDbTransaction from the IDbConnection
            //you have to beging a new transation, which is not want we want. Why? Outbox already has a transaction in play that the db biz ops need to enlist in
            var transaction = session.Connection.BeginTransaction();

            //so, you HAVE TO assign a transation instance to the session returned by SynchronizedStorageSession.Session() or the call won't work
            //2019-12-07 11:08:20.607 INFO  NServiceBus.RecoverabilityExecutor Immediate Retry is going to retry message 'cb9e9517-dd12-4a2c-b9da-ab1c0109a504' because of an exception:
            //System.InvalidOperationException: ExecuteNonQuery requires the command to have a transaction when the connection assigned to the command is in a pending local transaction.  The Transaction property of the command has not been initialized.

            //trying this... AND, no surprise, it will not work:
            command.Transaction = (IDbTransaction)session.Transaction;
            //2019-12-07 11:18:04.458 INFO  NServiceBus.RecoverabilityExecutor Immediate Retry is going to retry message 'cb9e9517-dd12-4a2c-b9da-ab1c0109a504' because of an exception:
            //System.InvalidCastException: Unable to cast object of type 'NHibernate.Transaction.AdoTransaction' to type 'System.Data.IDbTransaction'.
            //which means if I want to use NHibernate for NSB persistence, and I want Outbox on, and need to share both the connction and transatoin from Outbox with all business db ops in each handler, that all db biz ops need to happen through NHiberate?
            //this could be a major problem for CRB, b/c they use a mash up of NHibernate via Repositories, ADO.NET, and some Dapper.
            //check these two blog posts to see if there is a way to pull this off:
            //https://blog.maartenballiauw.be/post/2007/06/20/enlisting-an-ado-net-command-in-an-nhibernate-transaction.html
            //https://lostechies.com/joshualockwood/2007/04/10/how-to-enlist-ado-commands-into-an-nhibernate-transaction/

            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public async Task TransactionScopeAndThrowingExceptionsNoOutbox(TestCommand message, IMessageHandlerContext context, string sql)
        {
            //12/2: after talking to Indu and Sean at Particular software today, putting bus ops in TransactionScope does NOTHING
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                Log.Info($"TestCommandHandler. OrderId: {message.OrderId}");

                const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=SimpleRabbitMQ; Integrated Security=True;";
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                //throw new Exception("boom!");

                //var options = new PublishOptions();
                //options.RequireImmediateDispatch();
                //await context.Publish(new TestEvent { OrderId = message.OrderId }, options);

                await context.Publish(new TestEvent { OrderId = message.OrderId });

                //throw new Exception("boom!");

                scope.Complete();
            }
        }
    }
}