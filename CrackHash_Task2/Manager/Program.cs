using System.Text.Json.Serialization;
using Manager.BackgroundServices;
using Manager.Options;
using Manager.Repositories;
using Manager.Services;
using MongoDB.Driver;

namespace Manager;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddXmlSerializerFormatters()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddMemoryCache();

        builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));
        builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("MongoOptions"));
        builder.Services.Configure<RequestOptions>(builder.Configuration.GetSection("RequestOptions"));
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMqOptions"));

        builder.Services.AddHttpClient();

        
        builder.Services.AddSingleton<RequestRepository>();
        builder.Services.AddSingleton<SubtaskRepository>();
        builder.Services.AddSingleton<TaskPublisher>();
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoOptions>>().Value;
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
            settings.RetryWrites = true;
            return new MongoClient(settings);
        });

        builder.Services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoOptions>>().Value;
            return sp.GetRequiredService<IMongoClient>().GetDatabase(options.DatabaseName);
        });


        builder.Services.AddScoped<HashCrackService>();

        builder.Services.AddHostedService<RequestTimeoutService>();

        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var rRepo = scope.ServiceProvider.GetRequiredService<RequestRepository>();
            rRepo.CreateIndexesAsync(CancellationToken.None).GetAwaiter().GetResult();
            var sRepo = scope.ServiceProvider.GetRequiredService<SubtaskRepository>();
            sRepo.CreateIndexesAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        app.MapControllers();
        app.Run();
    }
}

