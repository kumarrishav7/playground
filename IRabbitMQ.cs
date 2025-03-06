namespace NotificationService
{
    public interface IMessagePublisher
    {
        void PublishMessage(string message);
    }

    public interface IQueueChannel : IDisposable
    {
        void DeclareQueue(string queueName);
        void Publish(string exchange, string routingKey, byte[] message);
    }

}
