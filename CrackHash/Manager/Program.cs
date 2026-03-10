using Manager.BackgroundServices;
using Manager.Clients;
using Manager.Options;
using Manager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddXmlSerializerFormatters();

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerUrls"));

builder.Services.AddHttpClient();

builder.Services.AddSingleton<RequestQueueService>();
builder.Services.AddSingleton<RequestStateService>();

builder.Services.AddScoped<HashCrackService>();
builder.Services.AddScoped<WorkerClient>();

builder.Services.AddHostedService<RequestProcessingService>();
builder.Services.AddHostedService<RequestTimeoutService>();

var app = builder.Build();

app.MapControllers();
app.Run();

