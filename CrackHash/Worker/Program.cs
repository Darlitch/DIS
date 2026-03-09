using Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddXmlSerializerFormatters();

builder.Services.AddHttpClient();

builder.Services.AddScoped<BruteForceService>();
builder.Services.AddScoped<CallbackService>();

var app = builder.Build();

app.MapControllers();
app.Run();

