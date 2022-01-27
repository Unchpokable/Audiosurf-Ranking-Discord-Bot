using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DiscordRankingBot.Network.Service
{
    public class DifficultyHtmlClass
    {
        public static readonly string CasualContainer = "casual_scores_container";
        public static readonly string ProContainer = "pro_scores_container";
        public static readonly string EliteContainer = "elite_scores_container";

        public static readonly string Casual = "casual_scores";
        public static readonly string Pro = "pro_scores";
        public static readonly string Elite = "elite_scores";

        private static Dictionary<string, Difficulties> _diffAssoc = new Dictionary<string, Difficulties>()
        {
            {Casual, Difficulties.Casual },
            {Pro, Difficulties.Pro },
            {Elite, Difficulties.Elite }
        };

        public static Difficulties ConvertToEnum(string diff)
        {
            if (_diffAssoc.ContainsKey(diff))
                return _diffAssoc[diff];
            throw new Exception("Unable to translate given string into Difficulty enum");
        }
    }
}
