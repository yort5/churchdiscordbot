using ChurchDiscordBot.Helpers;

namespace ChurchDiscordBot.Configuration
{
    public class DiscordConfig
    {
        public string Token { get; set; }
        public string MediaChannelsString { get; set; }
        public List<ulong> MediaChannelsIds
        {
            get
            {
                return ConfigParser.ParseIdsFromString(MediaChannelsString);
            }
        }
    }
}
