using DiscordRankingBot.Network.Service;
using DiscordRankingBot.Network.Service.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Services.Internal
{
    class Goal
    {
        private Func<IReadOnlyPlayerStats, bool>[] _goals;

        public static Goal Pusher => new Goal(
            (stats) => { return stats.Character == Characters.Pusher; },
            (stats) => { return stats.CleanFinish; },
            (stats) => { return stats.SkillRating >= 1700; },
            (stats) => { return stats.LongestChain >= 230; },
            (stats) => { return stats.MaxMatch == Match.Match21; },
            (stats) => { return stats.ColorStats[BlockColor.Yellow].TotalTrafficCollectedPercent >= 70; }
            );

        public static Goal DVE => new Goal(
            (stats) => { return stats.Character == Characters.DVE; },
            (stats) => { return stats.CleanFinish; },
            (stats) => { return stats.SkillRating >= 2000; },
            (stats) => { return stats.AverageClusterColorCount >= 1.8; },
            (stats) => { return stats.AverageClusterSize >= 10; },
            (stats) => { return stats.MaxMatch == Match.Match21; }
            );

        public static Goal Eraser => new Goal(
            (stats) => { return stats.Character == Characters.Eraser; },
            (stats) => { return stats.CleanFinish; },
            (stats) => { return stats.SkillRating >= 1600; },
            (stats) => { return stats.SeeingRed; },
            (stats) => { return stats.LongestChain >= 230; },
            (stats) => { return stats.MaxMatch == Match.Match21; }
            );

        public static Goal Pointman => new Goal(
            (stats) => { return stats.Character == Characters.Pointman; },
            (stats) => { return stats.CleanFinish; },
            (stats) => { return stats.SkillRating >= 1000; },
            (stats) => { return stats.SeeingRed || stats.ButterNinja; },
            (stats) => { return stats.Chaindrops <= 7; },
            (stats) => { return stats.MaxMatch == Match.Match11 || stats.MaxMatch == Match.Match21; }
            );

        public Goal(params Func<IReadOnlyPlayerStats, bool>[] goals)
        {
            _goals = goals;
        }

        public bool Approve(IReadOnlyPlayerStats stats)
        {
            return _goals.All(x => x(stats));
        }
    }
}
