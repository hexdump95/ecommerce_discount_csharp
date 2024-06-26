using StackExchange.Redis;

namespace Discount.Tools.Redis
{
    public interface IRedisConnection
    {
        IDatabase GetDatabase();
    }

    public class RedisConnection : IRedisConnection
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisConnection(string connectionString)
        {
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }
    }
}
