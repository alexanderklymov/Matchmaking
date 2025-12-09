using MatchMaking.Worker.Kafka;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("Configs/appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddHostedService<MatchmakingWorker>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();
await host.RunAsync();
