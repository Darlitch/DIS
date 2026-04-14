using Worker.BackgroundServices;
using Worker.Options;
using Worker.Services;

namespace Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddXmlSerializerFormatters();
        
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMqOptions"));

        builder.Services.AddHttpClient();

        builder.Services.AddHostedService<TaskConsumerService>();

        builder.Services.AddScoped<BruteForceService>();
        builder.Services.AddScoped<ResultPublisher>();

        var app = builder.Build();

        app.MapControllers();
        app.Run();
    }
}

