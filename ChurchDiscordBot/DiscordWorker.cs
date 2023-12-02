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
        private readonly IHostEnvironment _environment;
        private readonly HttpClient _ltnHttpClient;

        private DiscordSocketClient _client;

        public DiscordWorker(
            HostConfig configOptions,
            IHostEnvironment environment,
            IHttpClientFactory httpClientFactory,
            ILogger<DiscordWorker> logger)
        {
            _config = configOptions;
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _ltnHttpClient = httpClientFactory.CreateClient("LTN");
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DiscordBot Service is starting.");

            stoppingToken.Register(() => _logger.LogInformation("DiscordBot Service is stopping."));

            try
            {
                var discordConfig = new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences | GatewayIntents.Guilds };
                _client = new DiscordSocketClient(discordConfig);

                //_client.Log += Log;

                //Initialize command handling.
                //_client.Ready += Client_Ready;
                //_client.MessageReceived += DiscordMessageReceived;
                //_client.SlashCommandExecuted += SlashCommandHandler;
                //_client.ModalSubmitted += ModalResponseHandler;

                // Connect the bot to Discord
                await _client.LoginAsync(TokenType.Bot, _config?.Discord?.Token);
                await _client.StartAsync();

                // give the service a chance to start before moving on to computational things
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

                var guilds = _client.Guilds.ToList();

                var lastUpdatedTicks = DateTime.MinValue.ToUniversalTime().Ticks;
                var mediaChannelId = (ulong)_config?.Discord?.MediaChannelsIds.FirstOrDefault();
                var testLtnChannel = _client.GetChannel(mediaChannelId) as IMessageChannel;
                string lastPostedTrack = string.Empty;

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("DiscordBot is doing background work.");
                    try
                    {
                        var ltnSong = await GetLtnSong();
                        if (ltnSong != null && lastPostedTrack != ltnSong.currenttrack.title)
                        {
                            var trackEmbed = new EmbedBuilder
                            {
                                Title = $"{ltnSong.currenttrack.artist}",
                                ThumbnailUrl = ltnSong.currenttrack.art
                            };

                            var trackSpan = TimeSpan.FromSeconds(ltnSong.currenttrack.duration);
                            trackEmbed
                            .AddField("Track", ltnSong.currenttrack.title)
                                //.AddField("Die stats", communityCardInfo.StatLine)
                                .WithFooter(footer => footer.Text = string.Format($"{trackSpan.Minutes}:{trackSpan.Seconds}"));
                            await testLtnChannel.SendMessageAsync(embed: trackEmbed.Build());
                            lastPostedTrack = ltnSong.currenttrack.title;
                        }
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError($"Exception trying to update nickname: {exc.Message}");
                    }

                    // only do these once a day
                    var currentDayTicks = DateTime.Today.ToUniversalTime().Ticks;
                    if (lastUpdatedTicks < currentDayTicks)
                    {
                        lastUpdatedTicks = currentDayTicks;
                    }

                    // ... and rest!
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
            }

            _logger.LogInformation("DiscordService has stopped.");
        }

        public async Task<LtnSongInfo> GetLtnSong()
        {
            //var request = new HttpRequestMessage(HttpMethod.Get);

            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(
                           new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://api.live365.com/station/a65452");
            var pageContent = await page.GetContentAsync();
            var startIndex = pageContent.IndexOf('{');
            var endIndex = pageContent.LastIndexOf('}');

            var jsonContent = pageContent.Substring(startIndex, endIndex - startIndex + 1);

            return JsonConvert.DeserializeObject<LtnSongInfo>(jsonContent);
        }
    }

    public class LtnSongInfo
    {
        public string name { get; set; }
        public string stationlogo { get; set; }
        public string stationlogodominantcolor { get; set; }
        public string[] genres { get; set; }
        public string website { get; set; }
        public string timezone { get; set; }
        public StreamUrls[] streamurls { get; set; }
        public string streamurl { get; set; }
        public string streamhlsurl { get; set; }
        public string description { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }
        public string instagram { get; set; }
        [JsonProperty("current-track")]
        [JsonPropertyName("current-track")]
        public CurrentTrack currenttrack { get; set; }
        public LastPlayed[] lastplayed { get; set; }
        public string mountid { get; set; }
        public string cover { get; set; }
        public bool auto_dj_on { get; set; }
        public bool live_dj_on { get; set; }
        public string active_mount { get; set; }
        public bool is_playing { get; set; }
        public bool station_enabled { get; set; }
        public string slug { get; set; }
        public int listeners { get; set; }
        public string station_type { get; set; }
        public string cachetime { get; set; }
        public string cachehost { get; set; }
    }

    public class CurrentTrack
    {
        public string title { get; set; }
        public string artist { get; set; }
        public string art { get; set; }
        public string start { get; set; }
        public string played { get; set; }
        public string sync_offset { get; set; }
        public float duration { get; set; }
        public string end { get; set; }
        public string source { get; set; }
        public string status { get; set; }
    }

    public class StreamUrls
    {
        public string high_quality { get; set; }
        public string encoding { get; set; }
        public string low_quality { get; set; }
        public string hls { get; set; }
    }

    public class LastPlayed
    {
        public string title { get; set; }
        public string artist { get; set; }
        public string art { get; set; }
        public string start { get; set; }
        public string played { get; set; }
        public string sync_offset { get; set; }
        public string duration { get; set; }
        public string end { get; set; }
    }
}