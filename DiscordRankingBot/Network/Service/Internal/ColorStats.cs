using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Network.Service.Internal
{
    public sealed class ColorStats
    {
        public int CollectedCount { get; set; }

        public int DerivedPointPercent { get; set; }

        public int TotalTrafficCollectedPercent { get; set; }
    }
}
