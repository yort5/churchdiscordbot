using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDiscordBot.Helpers
{
    public static class ConfigParser
    {
        public static List<ulong> ParseIdsFromString(string input)
        {
            List<ulong> ids = new List<ulong>();
            if (!string.IsNullOrEmpty(input))
            {
                foreach (var ulongString in input.Split(','))
                {
                    ulong.TryParse(ulongString, out ulong ulongId);
                    ids.Add(ulongId);
                }
            }
            return ids;
        }
    }
}
