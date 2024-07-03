using System.Text;
using System.Text.Json;

using Discount.Rabbit.Dtos;
using Discount.Token;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Discount.Rabbit
{
    public class RabbitLogoutService : IHostedService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitLogoutService> _logger;
        private readonly JsonSerializerOptions _json;
        private readonly ITokenService _tokenService;

        public RabbitLogoutService(
            ILogger<RabbitLogoutService> logger,
            ITokenService tokenService,
            IConfiguration configuration
        )
        {
            var factory = new ConnectionFactory { HostName = configuration["Uris:RabbitUrl"] };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(
                exchange: "auth",
                type: ExchangeType.Fanout,
                durable: false,
                autoDelete: false,
                arguments: null
            );
            _channel.QueueDeclare(
                queue: "auth",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _channel.QueueBind(
                queue: "auth",
                exchange: "auth",
                routingKey: "",
                arguments: null
            );

            _logger = logger;
            _json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _tokenService = tokenService;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var logoutEvent = JsonSerializer.Deserialize<LogoutEvent>(message, _json)!;
                var token = logoutEvent.Message.Split(" ")[1];
                _logger.LogDebug("logout...");
                await _tokenService.DeleteToken(token);
            };
            try
            {
                _channel.BasicConsume(
                    queue: "auth",
                    consumer: consumer,
                    autoAck: true,
                    exclusive: false,
                    noLocal: false,
                    arguments: null
                );
            }
            catch (Exception e)
            {
                _logger.LogError("There was an error: {error}", e.Message);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Close();
            _connection.Close();
            return Task.CompletedTask;
        }
    }
}
