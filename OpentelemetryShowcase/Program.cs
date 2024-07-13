using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

logger.LogInformation("Starting application configuration");

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        logger.LogInformation("Configuring OpenTelemetry tracing");

        var jaegerHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "localhost";
        var jaegerPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT") ?? "6831");

        logger.LogInformation($"Jaeger configuration - Host: {jaegerHost}, Port: {jaegerPort}");

        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("YourServiceName"))
            //.AddConsoleExporter()
            .AddJaegerExporter(opts =>
            {
                opts.AgentHost = jaegerHost;
                opts.AgentPort = jaegerPort;
                logger.LogInformation($"Jaeger exporter configured with Host: {opts.AgentHost}, Port: {opts.AgentPort}");
            });

        logger.LogInformation("OpenTelemetry tracing configuration completed");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

logger.LogInformation("Application configuration completed, starting the app");

app.Run();