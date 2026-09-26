using RawConsumer.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RawConsumer.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace RawConsumer.Services;

public class RawConsumerWorker : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly IMongoCollection<RawReading> _collection;
    private readonly ILogger<RawConsumerWorker> _logger;


    public RawConsumerWorker(
        IConsumer<string, string> consumer,
        KafkaOptions options,
        IMongoCollection<RawReading> collection,
        ILogger<RawConsumerWorker> logger)
    {
        _consumer = consumer;
        _options = options;
        _collection = collection;
        _logger = logger;
    }



    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await Task.Yield();

        _consumer.Subscribe(_options.Topic);

        _logger.LogInformation(
            "Subscribe to topic '{TopicName}'",
            _options.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                if (result?.Message?.Value is null)
                    continue;

                IncomingReading? reading;
                try
                {
                    reading = JsonSerializer
                   .Deserialize<IncomingReading>(result.Message.Value);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Invalid JSON at offset {Offset}",
                        result.Offset.Value);

                    _consumer.Commit(result);
                    continue;
                }

                if (reading is null)
                {
                    _logger.LogWarning(
                        "Returned null from kafka message");

                    continue;
                }

                RawReading? validReading;
                string? error;

                bool isValid = ReadingValidationService.Validate(
                    reading,
                    out validReading,
                    out error);

                if (!isValid)
                {
                    _logger.LogWarning(
                        "Invalid reading for event {EventId}. Error: {Error}",
                        result.Message.Key,
                        error);

                    _consumer.Commit(result);
                    continue;
                }

                await _collection.InsertOneAsync(validReading!);

                _logger.LogInformation(
                    "Save event: {EventId} 'raw readings' collection",
                    validReading!.EventId);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Raw consumer is stopping");
        }
        finally
        {
            _consumer.Close();
        }
    }
}
