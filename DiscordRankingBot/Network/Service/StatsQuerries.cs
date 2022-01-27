using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Network.Service
{
    class StatsQuerries
    {
        public static string GetBasicStatsUrl => _basicDomain + "song_getScoreDetail.php?rid=";
        public static string GetExtendedStatsUrl => _basicDomain + "song_getExtendedScoreDetail.php?rid=";
        public static string GetLeaderboard => _basicDomain + "song_getSong.php?sid=";

        private static readonly string _basicDomain = @"https://www.audio-surf.com/ext/";
    }
}
