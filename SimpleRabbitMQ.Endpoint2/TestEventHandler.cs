using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint2
{
    public class TestEventHandler :IHandleMessages<TestEvent>
    {
        private static readonly ILog Log = LogManager.GetLogger<TestEventHandler>();

        public Task Handle(TestEvent message, IMessageHandlerContext context)
        {
            Log.Info($"TestEventHandler. OrderId: {message.OrderId}");

            var sql = $"INSERT INTO [dbo].[TestEventHandler] ([OrderId]) VALUES ('{message.OrderId}');";

            var session = context.SynchronizedStorageSession.Session();
            session.CreateSQLQuery(sql).ExecuteUpdate();

            //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //{
            //    const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=SimpleRabbitMQ; Integrated Security=True;";
            //    using (var connection = new SqlConnection(connectionString))
            //    {
            //        connection.Open();
            //        var command = connection.CreateCommand();
            //        command.CommandText = sql;
            //        command.ExecuteNonQuery();
            //    }
            //    scope.Complete();
            //}

            return Task.CompletedTask;
        }
    }
}