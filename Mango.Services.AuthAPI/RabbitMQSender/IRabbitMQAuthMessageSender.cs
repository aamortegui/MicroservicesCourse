namespace Mango.Services.AuthAPI.RabbitMQSender
{
    public interface IRabbitMQAuthMessageSender
    {
        Task SendMessage(Object message, string queueName);
    }
}
