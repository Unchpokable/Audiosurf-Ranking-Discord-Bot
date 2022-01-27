using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRankingBot.Network.Service.Internal;

namespace DiscordRankingBot.Network.Service
{
    public class PlayerStats : IReadOnlyPlayerStats
    {
        public string Nickname { get; set; }
        public int Score { get; set; }
        public int SkillRating { get; set; }
        public int LongestChain { get; set; }
        public int Chaindrops { get; set; }
        public int BestCluster { get; set; }
        public float AverageClusterSize { get; set; }
        public float AverageClusterColorCount { get; set; }
        public int Overfills { get; set; }

        public Characters Character { get; set; }
        public Difficulties Difficulty { get; set; }
        public Dictionary<BlockColor, ColorStats> ColorStats { get; set; }
        public Dictionary<Powerups, int> PowerupsStats { get; set; }

        public float this[string key]
        {
            set
            {
                /*
                 * I'm very ashamed of this code, but I really don't know how to do it any other way
                 */
                if (key == "Skill rating:")
                    SkillRating = (int)value;
                else if (key == "Overfills:")
                    Overfills = (int)value;
                else if (key == "Longest chain (seconds):")
                    LongestChain = (int)value;
                else if (key == "Dropped chains:")
                    Chaindrops = (int)value;
                else if (key == "Best cluster (points):")
                    BestCluster = (int)value;
                else if (key == "Average cluster size:")
                    AverageClusterSize = value;
                else if (key == "Average cluster color count:")
                    AverageClusterColorCount = value;
            }
        }

        public PlayerStats()
        {
            ColorStats = new Dictionary<BlockColor, ColorStats>()
            {
                {BlockColor.Purple, new ColorStats() },
                {BlockColor.Blue, new ColorStats() },
                {BlockColor.Green, new ColorStats() },
                {BlockColor.Yellow, new ColorStats() },
                {BlockColor.Red, new ColorStats() },
                {BlockColor.White, new ColorStats() }
            };

            PowerupsStats = new Dictionary<Powerups, int>()
            {
                {Powerups.Storm, 0 },
                {Powerups.Paint, 0 },
                {Powerups.Multiplier, 0 },
                {Powerups.Sort, 0 }
            };
        }

        public override string ToString()
        {
            return $"Name: {Nickname}\n" +
                    $"Score: {Score}\n" +
                    $"Skillrating: {SkillRating}\n" +
                    $"Character: {Character}\n"+
                    "======================\n";
        }
    }
}
