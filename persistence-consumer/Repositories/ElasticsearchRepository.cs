
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using PersistenceConsumer.Configuration;
using PersistenceConsumer.Models;

namespace PersistenceConsumer.Repositories;

public  class ElasticsearchRepository :
    IElasticsearchRepository
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchRepository(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task UpsertAsync(
        AnomalyDocument document,
        CancellationToken cancellationToken)
    {
        var upsertResponse =
            await _client.IndexAsync(
                document,
                request => request
                .Index(_options.IndexName)
                .Id(document.Id),
                cancellationToken);


        if (!upsertResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"failed to save documet: {document.Id}");
        }
    }
}
