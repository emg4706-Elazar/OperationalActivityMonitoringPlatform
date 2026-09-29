using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersistenceConsumer.Configuration;
using PersistenceConsumer.Models;
using System.Text.Json;

namespace PersistenceConsumer.Services;

public class PersistenceConsumerWorker : BackgroundService, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly AnomalyProcessor _processor;
    private readonly ILogger<PersistenceConsumerWorker> _logger;

    public PersistenceConsumerWorker(
        IConsumer<string, string> consumer,
        IOptions<KafkaOptions> options,
        AnomalyProcessor processor,
        ILogger<PersistenceConsumerWorker> logger)
    {
        _consumer = consumer;
        _options = options.Value;
        _processor = processor;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_options.Topic);

        _logger.LogInformation(
            "Consumer subscribe to topic '{TopicName}'",
            _options.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(
                stoppingToken);

                if (result?.Message?.Value is null)
                    continue;

                AnomalyMessage? anomalyMessage;
                try
                {
                    anomalyMessage = JsonSerializer
                    .Deserialize<AnomalyMessage>(
                    result.Message.Value);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Invalid JSON. Failed to deserialize to CSharp object.");

                    _consumer.Commit(result);
                    continue;
                }

                if (anomalyMessage is null)
                {
                    _logger.LogWarning(
                        "Received null from kafka topic '{Topic}', key '{Key}' offset '{Offset}'",
                        result.Topic,
                        result.Message.Key,
                        result.Offset);

                    _consumer.Commit(result);
                    continue;
                }

                await _processor.Process(
                    anomalyMessage, stoppingToken);

                _consumer.Commit(result);
            }
            

        }
        catch (OperationCanceledException)
            when(stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Kafka consumer is stopping");
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(
                ex,
                "Kafka consumer error: {Reason}.",
                ex.Error.Reason);
        }
        finally
        {
            _consumer.Close();
        }
    }


    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}
