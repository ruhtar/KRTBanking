namespace KRTBank.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topicArn, T message, CancellationToken cancellationToken = default);
}