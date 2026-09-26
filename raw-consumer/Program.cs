using Microsoft.Extensions.Hosting;
using RawConsumer.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using RawConsumer.Services;
using MongoDB.Driver;
using RawConsumer.Models;


namespace RawConsumer;

public class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder
            = Host.CreateApplicationBuilder(args);

        // Add kafka options to DI
        builder.Services.AddOptions<KafkaOptions>()
            .Bind(builder.Configuration.GetSection("Kafka"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.BootstrapServers),
                "Kafka:BootstrapServers is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.ClientId),
                "Kafka:ClientId is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.GroupId),
                "Kafka:Groupid is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Topic),
                "Kafka:Topic is required")
            .ValidateOnStart();



        builder.Services.AddSingleton<IConsumer<string, string>>(
                services =>
                {
                    KafkaOptions options =
                        services.GetRequiredService<
                            IOptions<KafkaOptions>>()
                            .Value;

                    ConsumerConfig config = new()
                    {
                        BootstrapServers = options.BootstrapServers,
                        GroupId = options.GroupId,
                        ClientId = options.ClientId,
                        AutoOffsetReset = AutoOffsetReset.Earliest,
                        EnableAutoCommit = false
                    };

                    return new ConsumerBuilder<string, string>(config)
                        .Build();
                });


        builder.Services.AddHostedService<RawConsumerWorker>();


        // Add Mongo Options to into the DI
        builder.Services.AddOptions<MongoOptions>()
            .Bind(builder.Configuration.GetSection("Mongo"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Uri),
                "Mongo:Uri is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Database),
                "Mongo:Database is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Collection),
                "Mongo:Collection is required")
            .ValidateOnStart();


        // Regiseter the Mongo Collection into the DI
        builder.Services.AddSingleton<IMongoCollection<RawReading>>(serviceProvider =>
                {
                    MongoOptions options =
                    serviceProvider.GetRequiredService<
                        IOptions<MongoOptions>>()
                        .Value;

                    var client = new MongoClient(options.Uri);

                    var database = client
                    .GetDatabase(options.Database);

                    return database.GetCollection<RawReading>(
                        options.Collection);
                });

        IHost host = builder.Build();

        await host.RunAsync();
    }
}
