using ChurchDiscordBot;
using ChurchDiscordBot.Configuration;
using ChurchDiscordBot.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the connection string
//var connectionString = builder.Configuration.GetValue<string>("Azure:AppConfiguration:ConnectionString") ?? string.Empty;

//// Load configuration from Azure App Configuration
//builder.Configuration.AddAzureAppConfiguration(options =>
//{
//    options.Connect(connectionString)
//        // Load configuration values with no label
//        .Select(KeyFilter.Any, "ChurchBot");
//});
//var hostConfig = builder.Configuration.Get<HostConfig>();
//builder.Services.AddSingleton<HostConfig>(hostConfig);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddHttpClient("LTN", c =>
{
    c.BaseAddress = new Uri("https://api.live365.com/");
});
// builder.Services.AddHostedService<DiscordWorker>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
