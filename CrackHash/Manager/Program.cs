using System.Text.Json.Serialization;
using Manager.BackgroundServices;
using Manager.Clients;
using Manager.Options;
using Manager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddMemoryCache();

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));

builder.Services.AddHttpClient();

builder.Services.AddSingleton<RequestQueueService>();
builder.Services.AddSingleton<RequestStateService>();
builder.Services.AddSingleton<WorkerClient>();

builder.Services.AddScoped<HashCrackService>();

builder.Services.AddHostedService<RequestProcessingService>();
builder.Services.AddHostedService<RequestTimeoutService>();

var app = builder.Build();

app.MapControllers();
app.Run();

