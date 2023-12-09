﻿using ChurchDiscordBot.Helpers;

namespace ChurchDiscordBot.Configuration
{
    public class RadioStationConfig
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string MediaChannels { get; set; }
        public List<ulong> MediaChannelsIds
        {
            get
            {
                return ConfigParser.ParseIdsFromString(MediaChannels);
            }
        }
    }
}
