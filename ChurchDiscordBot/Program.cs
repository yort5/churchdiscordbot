using ChurchDiscordBot;
using ChurchDiscordBot.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the connection string
// AZURE_APPCONFIGURATION_CONNECTIONSTRING
var connectionString = builder.Configuration.GetValue<string>("AZURE:APPCONFIGURATION:CONNECTIONSTRING") ?? string.Empty;
if(string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("AzureAppConfigurationEndpoint") ?? "DIDNOTWORK";
}

var hostConfig = builder.Configuration.Get<HostConfig>();
builder.Services.AddSingleton<HostConfig>(hostConfig);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHostedService<DiscordWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
