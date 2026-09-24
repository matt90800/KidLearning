using KidLearning.Server.Services;
using KidsLearning.Server;
using NLog;
using NLog.Web;
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    // Remove default Microsoft logging providers
    builder.Logging.ClearProviders();
    // Register NLog
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.Configure<SpeechOptions>(
        builder.Configuration.GetSection("Speech"));
    builder.Services.AddSingleton<WyomingClient>();

    builder.Services.AddScoped<ISpeechService, PiperSpeechService>();

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseDefaultFiles();
    app.MapStaticAssets();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    //app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.MapFallbackToFile("/index.html");

    app.Run();
}
catch (Exception)
{
    logger.Log(NLog.LogLevel.Error, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}