using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PersistenceConsumer.Configuration;
using PersistenceConsumer.Data;
using PersistenceConsumer.Repositories;
using PersistenceConsumer.Services;
using Elastic.Clients.Elasticsearch;


namespace PersistenceConsumer;

public class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder =
            Host.CreateApplicationBuilder();

        // Add kafka options to the DI
        builder.Services.AddOptions<KafkaOptions>()
            .Bind(builder.Configuration.GetSection("Kafka"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.BootstrapServers),
                "Kafka:BootstrapServes is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Topic),
                "Kafka:Topic name is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.GroupId),
                "Kafka:GroupId is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.ClientId),
                "Kafka:ClientId is required")
            .ValidateOnStart();


        // Register the cosnumer into the DI
        builder.Services.AddSingleton<
            IConsumer<string, string>>(serviceProvider =>
            {
                var options =
                serviceProvider.GetRequiredService<
                    IOptions<KafkaOptions>>()
                    .Value;

                var config = new ConsumerConfig()
                {
                    BootstrapServers = options.BootstrapServers,
                    GroupId = options.GroupId,
                    ClientId = options.ClientId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                return new ConsumerBuilder<
                    string, string>(config)
                    .Build();
            });


        // Add mysql options to the DI
        builder.Services.AddOptions<MySqlOptions>()
            .Bind(builder.Configuration.GetSection("MySql"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.ConnectionStrings),
                "MySql:ConnectionStrings is required")
            .ValidateOnStart();


        // Register the ApplicationDbContext
        builder.Services.AddDbContextFactory<
            ApplicationDbContext>(
            (serviceProvider, dbOptions) =>
            {
                MySqlOptions mySqlOptions =
                serviceProvider.GetRequiredService<
                    IOptions<MySqlOptions>>()
                    .Value;

                string connectionString =
                    mySqlOptions.ConnectionStrings;

                dbOptions.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString));
            });

        // Register the mysql repository
        builder.Services.AddSingleton<
            IMySqlRepository, MySqlRepository>();


        // Add the Elasticsearch options to the DI
        builder.Services.AddOptions<ElasticsearchOptions>()
            .Bind(builder.Configuration.GetSection("Elasticsearch"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Url),
                "Elasticsearch:Url is required")
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.IndexName),
                "Elsaticsearch:IndexName is required")
            .ValidateOnStart();

        // Register the elastic client
        builder.Services.AddSingleton<ElasticsearchClient>(
            services =>
            {
                ElasticsearchOptions options =
                services.GetRequiredService<
                    IOptions<ElasticsearchOptions>>()
                    .Value;


                ElasticsearchClientSettings settings =
                    new ElasticsearchClientSettings(
                        new Uri(options.Url))
                        .DefaultIndex(options.IndexName);

                return new ElasticsearchClient(settings);
            });


        // Register the Elastic initializer to the DI
        builder.Services.AddSingleton<
            ElasticsearchIndexInitializer>();

        // Register the Elastic repository to the DI
        builder.Services.AddSingleton<
            IElasticsearchRepository,
            ElasticsearchRepository>();


        // Add Rabbitmq options to the DI
        builder.Services.AddOptions<RabbitMqOptions>()
           .Bind(builder.Configuration.GetSection("RabbitMq"))
           .Validate(options =>
               !string.IsNullOrWhiteSpace(options.HostName),
               "RabbitMq:HostName is required")
           .Validate(options =>
               options.Port > 0,
               "RabbitMq:Port must be positive")
           .Validate(options =>
               !string.IsNullOrWhiteSpace(options.UserName),
               "RabbitMq:UserName is required")
           .Validate(options =>
               !string.IsNullOrWhiteSpace(options.Password),
               "RabbitMq:Password is required")
           .Validate(options =>
               !string.IsNullOrWhiteSpace(options.QueueName),
               "RabbitMq:QueueName is required")
           .ValidateOnStart();


        // Register the RabbitMq publisher to the DI
        builder.Services.AddSingleton<
            IRabbitMqPublisher,
            RabbitMqPublisher>();


        builder.Services.AddSingleton<AnomalyProcessor>();

        builder.Services.AddHostedService<
            PersistenceConsumerWorker>();

        var host = builder.Build();

        await host.RunAsync();
    }
}
