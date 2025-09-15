using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.RewardAPI.Messaging
{
    public class RabbitMQOrderRewardConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;
        private IConnection _connection;
        private IChannel _channel;
        private string ExchangeName = "";
        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";   

        public RabbitMQOrderRewardConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;
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
            _channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue,false,false,false,null).GetAwaiter().GetResult();
            _channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, ExchangeName, "RewardsUpdate").GetAwaiter().GetResult();
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
            _channel.BasicConsumeAsync(OrderCreated_RewardsUpdateQueue, false, consumer).Wait();
            return Task.CompletedTask;
        }

        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            _rewardService.UpdateRewards(rewardsMessage).GetAwaiter().GetResult();
        }
    }
}
