using System.Data.SqlClient;
using System.Threading.Tasks;
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

            const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=SimpleRabbitMQ; Integrated Security=True;";
            var sql = $"INSERT INTO [dbo].[TestEventHandler] ([OrderId]) VALUES ('{message.OrderId}');";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }
    }
}