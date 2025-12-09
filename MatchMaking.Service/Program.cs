using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MatchMaking.Service.Kafka;
using MatchMaking.Service.Redis;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MatchMaking.Service.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("Configs/appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var redisConnection = builder.Configuration.GetConnectionString("Redis");

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection ?? "matchmaking-redis:6379"));
builder.Services.AddSingleton<IMatchRepository, RedisMatchRepository>();
builder.Services.AddSingleton<IRateLimiter, RedisRateLimiter>();

// Kafka
builder.Services.AddSingleton<IMatchmakingRequestProducer, KafkaMatchmakingRequestProducer>();
builder.Services.AddHostedService<MatchmakingCompleteConsumer>();

var app = builder.Build();

app.MapPost("/match/search", async (
    [FromQuery] string userId,
    IRateLimiter rateLimiter,
    IMatchmakingRequestProducer producer) =>
{
    if (!await rateLimiter.IsAllowedAsync(userId))
        return Results.BadRequest("Too many requests");

    await producer.SendAsync(userId);

    return Results.NoContent();
});

app.MapGet("/match/info", async (
    [FromQuery] string userId,
    IMatchRepository repo) =>
{
    var match = await repo.GetLastMatchForUserAsync(userId);
    return match is null ? Results.NotFound() : Results.Ok(match);
});


app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();