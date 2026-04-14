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

        builder.Services.AddScoped<BruteForceService>();
        builder.Services.AddScoped<CallbackService>();

        var app = builder.Build();

        app.MapControllers();
        app.Run();
    }
}

