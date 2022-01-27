using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Network.Service.Internal
{
    public interface IReadOnlyPlayerStats
    {
        string Nickname { get; set; }
        int Score { get; set; }
        int SkillRating { get; set; }
        int LongestChain { get; set; }
        int Chaindrops { get; set; }
        int BestCluster { get; set; }
        float AverageClusterSize { get; set; }
        float AverageClusterColorCount { get; set; }

        Characters Character { get; set; }
        Difficulties Difficulty { get; }
        Dictionary<BlockColor, ColorStats> ColorStats { get; }
        Dictionary<Powerups, int> PowerupsStats { get; }
    }
}
