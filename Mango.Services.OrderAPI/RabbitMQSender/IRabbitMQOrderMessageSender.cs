namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public interface IRabbitMQOrderMessageSender
    {
        Task SendMessage(Object message, string exchangeName);
    }
}
