using Elastic.Clients.Elasticsearch;
using PersistenceConsumer.Configuration;
using PersistenceConsumer.Models;
using Microsoft.Extensions.Options;


namespace PersistenceConsumer.Services;

public sealed class ElasticsearchIndexInitializer
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchIndexInitializer(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options)
    {
        _client = client;
        _options = options.Value;
    }


    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var existsResponse =
            await _client.Indices.ExistsAsync(
                _options.IndexName,
                cancellationToken);


        if (existsResponse.Exists)
            return;


        var createResponse =
            await _client.Indices.CreateAsync<
                AnomalyDocument>(index => index
                .Index(_options.IndexName)
                .Mappings(mapping => mapping
                    .Properties(properties => properties
                        .LongNumber(document => document.Id)
                        .Keyword(document => document.EventId)
                        .Keyword(document => document.SourceId)
                        .Keyword(document => document.Sector)
                        .DoubleNumber(document => document.Value)
                        .DoubleNumber(document => document.Mean)
                        .DoubleNumber(document =>
                            document.StandardDeviation)
                        .DoubleNumber(document => document.ZScore)
                        .Keyword(document => document.Severity)
                        .Keyword(document => document.Status)
                        .Date(document => document.DetectedAt)
                        )
                    ),
                cancellationToken);


        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                "Failed to create elasticsearch index.");
        }
    }
}
