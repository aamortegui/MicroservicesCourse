namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public interface IRabbitMQCartMessageSender
    {
        Task SendMessage(Object message, string queueName);
    }
}
