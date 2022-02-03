using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Network.Service.Internal
{
    public interface IReadOnlyPlayerStats
    {
        string Nickname { get; }
        int Score { get; }
        int SkillRating { get; }
        int LongestChain { get; }
        int Chaindrops { get; }
        int BestCluster { get; }
        float AverageClusterSize { get; }
        float AverageClusterColorCount { get; }
        bool CleanFinish { get; }
        bool SeeingRed { get; }
        bool ButterNinja { get; }
        Match MaxMatch { get; }
        Characters Character { get; }
        Difficulties Difficulty { get; }
        Dictionary<BlockColor, ColorStats> ColorStats { get; }
        Dictionary<Powerups, int> PowerupsStats { get; }
    }
}
