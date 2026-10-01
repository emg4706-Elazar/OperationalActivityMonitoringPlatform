using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;
using PersistenceConsumer.Models;
using PersistenceConsumer.Configuration;

namespace PersistenceConsumer.Services;

public sealed class RabbitMqPublisher :
    IRabbitMqPublisher,
    IAsyncDisposable
{
    private readonly RabbitMqOptions _options;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }


    public async Task PublishAsync(
        CriticalAlertMessage message,
        CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);

        byte[] body =
            JsonSerializer.SerializeToUtf8Bytes(message);

        BasicProperties properties = new()
        {
            ContentType = "application/json",
            Persistent = true
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }


    private async Task EnsureConnectedAsync(
        CancellationToken cancellationToken)
    {
        if (_connection is null || !_connection.IsOpen)
        {
            ConnectionFactory factory = new()
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                ClientProvidedName = "persistence-consumer",
                AutomaticRecoveryEnabled = true
            };

            _connection =
                await factory.CreateConnectionAsync(
                    cancellationToken);
        }

        if (_channel is null || !_channel.IsOpen)
        {
            CreateChannelOptions channelOptions = new(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);


            _channel =
                await _connection.CreateChannelAsync(
                    channelOptions,
                    cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }  
    }

    
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
