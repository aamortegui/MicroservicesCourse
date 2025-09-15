
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.QueueDeclareAsync(_configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"), 
                false, false, false, null).GetAwaiter().GetResult();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (ch, ea) =>
            {
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                string email = JsonConvert.DeserializeObject<string>(content);
                HandleMessage(email).GetAwaiter().GetResult();                
                _channel.BasicAckAsync(ea.DeliveryTag, false).GetAwaiter().GetResult();
                return Task.CompletedTask;
            };
            _channel.BasicConsumeAsync(_configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"), false, consumer).Wait();
            return Task.CompletedTask;
        }

        private async Task HandleMessage(string email)
        {
            _emailService.RegisterUserEmailAndLog(email).GetAwaiter().GetResult();
        }
    }
}
