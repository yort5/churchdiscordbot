using ChurchDiscordBot.Applet.Radio;
using ChurchDiscordBot.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Runtime;
using System.Text.Json.Serialization;

namespace ChurchDiscordBot
{
    public class DiscordWorker : BackgroundService
    {
        private readonly HostConfig _config;
        private readonly ILogger<DiscordWorker> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostEnvironment _environment;
        private readonly DiscordSocketClient _client;

        private RadioStationService _radioStationService;

        public DiscordWorker(
            HostConfig configOptions,
            IHostEnvironment environment,
            ILoggerFactory loggerFactory,
            ILogger<DiscordWorker> logger)
        {
            _config = configOptions;
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _loggerFactory = loggerFactory;
            _logger = logger;

            var discordConfig = new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences | GatewayIntents.Guilds };
            _client = new DiscordSocketClient(discordConfig);

            ILogger<RadioStationService> radioStationServiceLogger = _loggerFactory.CreateLogger<RadioStationService>();
            _radioStationService = new RadioStationService(radioStationServiceLogger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DiscordBot Service is starting.");

            stoppingToken.Register(() => _logger.LogInformation("DiscordBot Service is stopping."));

            try
            {


                //_client.Log += Log;

                //Initialize command handling.
                //_client.Ready += Client_Ready;
                //_client.MessageReceived += DiscordMessageReceived;
                //_client.SlashCommandExecuted += SlashCommandHandler;
                //_client.ModalSubmitted += ModalResponseHandler;

                // Connect the bot to Discord
                await _client.LoginAsync(TokenType.Bot, _config?.Discord?.Token);
                await _client.StartAsync();

                // Initialize apps


                // give the service a chance to start before moving on to computational things
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

                var guilds = _client.Guilds.ToList();

                var lastUpdatedTicks = DateTime.MinValue.ToUniversalTime().Ticks;

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("DiscordBot is doing background work.");
                    try
                    {
                        foreach (var radioStation in _config.Applet.RadioStations)
                        {
                            await _radioStationService.CheckForNewSongAsync(radioStation, _client);
                        }
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError($"Exception checking radio stations: {exc.Message}");
                    }

                    // only do these once a day
                    var currentDayTicks = DateTime.Today.ToUniversalTime().Ticks;
                    if (lastUpdatedTicks < currentDayTicks)
                    {
                        lastUpdatedTicks = currentDayTicks;
                    }

                    // ... and rest!
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (Exception exc)
            {
                _logger.LogError($"Exception in main loop: {exc.Message}");
            }

            _logger.LogInformation("DiscordService has stopped.");
        }
    }
}