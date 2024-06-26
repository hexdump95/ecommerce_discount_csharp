using System.Text.Json;

using Discount.Middlewares.Models;

using StackExchange.Redis;

namespace Discount.Token
{
    public interface ITokenService
    {
        Task<LoggedInUser?> FindByToken(string token);
        Task SaveToken(string token, LoggedInUser user);
    }

    public class TokenService : ITokenService
    {
        private readonly IDatabase _database;
        private readonly JsonSerializerOptions _jsonOptions;
        const string EntityName = "token";
        const string EntityPluralName = "tokens";
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IConnectionMultiplexer redisConnection, ILogger<TokenService> logger
        )
        {
            _database = redisConnection.GetDatabase();
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };
            _logger = logger;
        }

        public async Task<LoggedInUser?> FindByToken(string token)
        {
            var tokenExists = await _database.SetContainsAsync(EntityPluralName, token);

            if (!tokenExists)
                return null;

            var redisValue = await _database.StringGetAsync($"{EntityName}:{token}");

            return redisValue.IsNullOrEmpty
                ? null
                : JsonSerializer.Deserialize<LoggedInUser>(redisValue!, _jsonOptions);
        }

        public async Task SaveToken(string token, LoggedInUser user)
        {
            var jsonString = JsonSerializer.Serialize(user, _jsonOptions);
            await _database.SetAddAsync(EntityPluralName, token);
            await _database.StringSetAsync($"{EntityName}:{token}", jsonString);
        }
    }
}
