using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MijnSauna.Middleware.Processor.DependencyInjection;
using MijnSauna.Middleware.Processor.Services;
using MijnSauna.Middleware.Processor.Workers;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSystemd();

builder.WebHost.UseKestrel();
builder.WebHost.ConfigureKestrel((hostContext, options) =>
{
    options.Listen(IPAddress.Any, 5050,
        configure => configure.Protocols = HttpProtocols.Http2);
});

builder.Services.ConfigureProcessor();
builder.Services.AddGrpc();
builder.Services.AddHostedService<ConfigurationWorker>();
builder.Services.AddHostedService<SessionWorker>();
builder.Services.AddHostedService<SampleWorker>();

var elasticHost = builder.Configuration.GetValue<string>("ElasticHost");
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticHost))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "mijnsauna-processor-{0:yyyy.MM}"
    }).CreateLogger();
builder.Logging.AddSerilog();

var app = builder.Build();

app.MapGrpcService<GrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();