using DiscordRankingBot.Network.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Services.Internal
{
    class LeaderboardDetail
    {
        public string FullUrl { get; set; }
        public int RideID { get; set; }
        public string Difficulty { get; set; }

        public Characters Character { get; set; }
    }
}
