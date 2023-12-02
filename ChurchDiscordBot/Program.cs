using ChurchDiscordBot;
using ChurchDiscordBot.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = Host.CreateApplicationBuilder(args);

// Retrieve the connection string
string connectionString = builder.Configuration.GetConnectionString("AzureAppConfigurationEndpoint") ?? string.Empty;

// Load configuration from Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
        // Load configuration values with no label
        .Select(KeyFilter.Any, "ChurchBot");
});

var hostConfig = builder.Configuration.Get<HostConfig>();
builder.Services.AddSingleton<HostConfig>(hostConfig);

builder.Services.Configure<HostConfig>(builder.Configuration.GetSection("HostConfig"));

builder.Services.AddHttpClient("LTN", c =>
{
    c.BaseAddress = new Uri("https://api.live365.com/");
});
builder.Services.AddHostedService<DiscordWorker>();

var app = builder.Build();

await app.RunAsync();
