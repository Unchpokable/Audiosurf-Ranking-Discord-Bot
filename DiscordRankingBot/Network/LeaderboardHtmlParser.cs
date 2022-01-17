using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using DiscordRankingBot.Network.Service;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Attributes;
using AngleSharp.Html.Parser;

namespace DiscordRankingBot.Network
{
    class LeaderboardHtmlParser
    {
        public PlayerStatsDescriptor[] Scores { get; private set; }

        private HttpClient _httpClient;
        private HtmlParser _parser;

        public LeaderboardHtmlParser()
        {
            _httpClient = new HttpClient();
        }
        
        public async Task<PlayerStatsDescriptor[]> GetStats(string url)
        {
            var stats = 
        }
    }
}
