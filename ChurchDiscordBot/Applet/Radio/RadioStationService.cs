using ChurchDiscordBot.Configuration;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Collections.Generic;

namespace ChurchDiscordBot.Applet.Radio
{
    public class RadioStationService
    {
        private readonly ILogger<RadioStationService> _logger;
        private Dictionary<string, List<CurrentTrack>> _trackHistory = new Dictionary<string, List<CurrentTrack>>();

        public RadioStationService(
            ILogger<RadioStationService> logger)
        {
            _logger = logger;
        }

        public async Task CheckForNewSongAsync(RadioStationConfig stationConfig, DiscordSocketClient client)
        {
            try
            {
                var currentStationInfo = await GetStationInfoAsync(stationConfig);
                var currentTrack = currentStationInfo.currenttrack;
                _logger.LogInformation($"Retrieved information for song {currentTrack.title}.");

                var songList = _trackHistory.SingleOrDefault(t => t.Key == stationConfig.Name).Value;
                if (songList == null)
                {
                    songList = new List<CurrentTrack>();
                    _trackHistory.Add(stationConfig.Name, songList);
                }

                if (songList.Any(s => s.title == currentTrack.title))
                {
                    return; // track is not new
                }

                songList.Add(currentTrack);
                if (songList.Count > 5)
                {
                    songList.Remove(songList.OrderBy(s => s.start).First());
                }

                // send the song to Discord
                var trackEmbed = new EmbedBuilder
                {
                    Title = $"{currentTrack.artist}",
                    ThumbnailUrl = currentTrack.art
                };

                var trackSpan = TimeSpan.FromSeconds(currentTrack.duration);
                var timeText = trackSpan.Hours == 0 ? string.Format($"{trackSpan.Minutes}:{trackSpan.Seconds}") : string.Format($"{trackSpan.Hours}:{trackSpan.Minutes}:{trackSpan.Seconds}");
                trackEmbed
                .AddField("Track", currentTrack.title)
                    //.AddField("Die stats", communityCardInfo.StatLine)
                    .WithFooter(footer => footer.Text = timeText);

                foreach (var channelId in stationConfig.MediaChannelsIds)
                {
                    var mediaChannel = await client.GetChannelAsync(channelId) as IMessageChannel;
                    await mediaChannel.SendMessageAsync(embed: trackEmbed.Build());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception checking for new song ({stationConfig.Identifier}).");
            }
        }

        public async Task<RadioStationInfo> GetStationInfoAsync(RadioStationConfig stationConfig)
        {
            RadioStationInfo stationInfo = new RadioStationInfo();
            try
            {
                if (stationConfig != null && !string.IsNullOrEmpty(stationConfig.Identifier))
                {
                    await using var browser = await Puppeteer.LaunchAsync(
                                   new LaunchOptions
                                   {
                                       Headless = true,
                                       Args = new[]
                                        {
                                    "--no-sandbox"
                                        }
                                   });
                    await using var page = await browser.NewPageAsync();
                    // await page.GoToAsync("https://api.live365.com/station/a65452");
                    await page.GoToAsync(stationConfig.Identifier);
                    var pageContent = await page.GetContentAsync();
                    var startIndex = pageContent.IndexOf('{');
                    var endIndex = pageContent.LastIndexOf('}');

                    var jsonContent = pageContent.Substring(startIndex, endIndex - startIndex + 1);

                    stationInfo = JsonConvert.DeserializeObject<RadioStationInfo>(jsonContent) ?? new RadioStationInfo();
                }
                else
                {
                    _logger.LogError("Null station config found.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving station info ({stationConfig.Identifier}).");
            }
            return stationInfo;
        }
    }
}
