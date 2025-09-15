﻿using RabbitMQ.Client;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;
        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

        public RabbitMQOrderMessageSender()
        {
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest";
        }

        public async Task SendMessage(object message, string exchangeName)
        {
            if (ConnectionExists()) 
            {
                using var channel = await _connection.CreateChannelAsync();
                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: false);
                await channel.QueueDeclareAsync(OrderCreated_EmailUpdateQueue, false, false, false, null);
                await channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue, false, false, false, null);
                
                await channel.QueueBindAsync(OrderCreated_EmailUpdateQueue, exchangeName, "EmailUpdate");
                await channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, exchangeName, "RewardsUpdate");
                
                var json = System.Text.Json.JsonSerializer.Serialize(message);
                var body = System.Text.Encoding.UTF8.GetBytes(json);
                await channel.BasicPublishAsync(exchange: exchangeName, "EmailUpdate", body);
                await channel.BasicPublishAsync(exchange: exchangeName, "RewardsUpdate", body);
            }            
        }

        private async Task CreateConnectionAsync() 
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password
                };
                _connection = await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                throw;
            }            
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateConnectionAsync().GetAwaiter().GetResult();
            return true;
        }
    }
}
