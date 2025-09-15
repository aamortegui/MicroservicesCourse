
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";
        private string ExchangeName = "";
        string queueName = "";

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            ExchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.ExchangeDeclareAsync(ExchangeName,ExchangeType.Direct).GetAwaiter().GetResult();
            _channel.QueueDeclareAsync(OrderCreated_EmailUpdateQueue, false, false, false, null).GetAwaiter().GetResult();
            _channel.QueueBindAsync(OrderCreated_EmailUpdateQueue, ExchangeName, "EmailUpdate").GetAwaiter().GetResult();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (ch, ea) =>
            {
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                HandleMessage(rewardsMessage).GetAwaiter().GetResult();                
                _channel.BasicAckAsync(ea.DeliveryTag, false).GetAwaiter().GetResult();
                return Task.CompletedTask;
            };
            _channel.BasicConsumeAsync(OrderCreated_EmailUpdateQueue, false, consumer).Wait();
            return Task.CompletedTask;
        }

        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            _emailService.LogOrderPlaced(rewardsMessage).GetAwaiter().GetResult();
        }
    }
}
