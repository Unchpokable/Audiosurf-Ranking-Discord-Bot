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
using AngleSharp.Dom;
using DiscordRankingBot.Network.Service.Internal;
using System.Globalization;

namespace DiscordRankingBot.Network
{
    public class LeaderboardHtmlParser
    {
        private HttpClient _httpClient;
        private HtmlParser _parser;

        public LeaderboardHtmlParser()
        {
            _httpClient = new HttpClient();
            _parser = new HtmlParser();
        }
        
        public async Task<IReadOnlyPlayerStats[]> GetStats(int rideId, string diff) 
        {
            var result = new List<PlayerStats>();
            var rawResult = await _httpClient.GetAsync(StatsQuerries.GetLeaderboard + rideId);
            var selectedDifficulty = _parser.ParseDocument(rawResult.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            var scores = selectedDifficulty.QuerySelector($"div#global_scores_container.song_scores .scores.{diff}").QuerySelectorAll("div.score");
            var difficulty = DifficultyHtmlClass.ConvertToEnum(diff);

            foreach (var block in scores)
            {
                var stats = await GetExtendedStats(block.GetAttribute("id"), difficulty);
                stats.Nickname = block.QuerySelector("span.score_name a").TextContent;
                stats.Score = int.Parse(block.QuerySelector("span.score_points").TextContent.Replace(",", string.Empty));
                await GetBasicStats(block.GetAttribute("id"), stats);
                result.Add(stats);
            }
            return result.ToArray();
        }

        private async Task GetBasicStats(string uid, PlayerStats stats)
        {
            var reply = await _httpClient.GetAsync(StatsQuerries.GetBasicStatsUrl + uid);
            var rawText = await reply.Content.ReadAsStringAsync();
            stats.MaxMatch = GetMaxMatch(rawText);
            stats.CleanFinish = rawText.Contains("Clean Finish");
            stats.SeeingRed = rawText.Contains("Butter Ninja");
            stats.SeeingRed = rawText.Contains("Seeing Red");
        }

        private Match GetMaxMatch(string text)
        {
            if (text.Contains("Match21"))
                return Match.Match21;
            if (text.Contains("Match11"))
                return Match.Match11;
            if (text.Contains("Match7"))
                return Match.Match7;
            return Match.NoMatch;
        }

        private async Task<PlayerStats> GetExtendedStats(string uid, Difficulties diff) 
        {
            var result = new PlayerStats();
            var rawReply = await _httpClient.GetAsync(StatsQuerries.GetExtendedStatsUrl + uid);
            var stringContent = await rawReply.Content.ReadAsStringAsync();
            var playerCharacter = RecognizePlayerCharacter(stringContent);
            if (playerCharacter == null)
                throw new Exception("API Error: Unable to recognize character from reply, given by https://audio-surf.com/ext");

            result.Character = (Characters)playerCharacter;

            var content = _parser.ParseDocument(stringContent).QuerySelector("table#ext_table tbody").QuerySelectorAll("tr").Skip(2).ToArray(); //First 2 rows is a specific stats that not needed for rating calculation

            for (var i = 0; i < 9; i++)
            {
                try
                {
                    var tds = content[i].QuerySelectorAll("td");
                    result[tds[0].TextContent] = float.Parse(tds[1].TextContent, CultureInfo.InvariantCulture);
                }
                catch (Exception e) 
                {
                    Console.WriteLine($"Error during parsing result given by audisourf website API:\n{e.Message}");
                }
            }
            ParseColorTable(content[9], result);
            result.Difficulty = diff;
            result.PowerupsStats[Powerups.Paint] = int.Parse(content[14].QuerySelectorAll("td")[1].TextContent);
            result.PowerupsStats[Powerups.Storm] = int.Parse(content[15].QuerySelectorAll("td")[1].TextContent);
            result.PowerupsStats[Powerups.Multiplier] = int.Parse(content[16].QuerySelectorAll("td")[1].TextContent);
            result.PowerupsStats[Powerups.Sort] = int.Parse(content[17].QuerySelectorAll("td")[1].TextContent);
            return result;
        }

        private Nullable<Characters> RecognizePlayerCharacter(string rawHtml)
        {
            if (rawHtml.Contains("Right Pushes")) //audiosurf website has no normal API so i'll forced to do this shit
                return Characters.Pusher;

            if (rawHtml.Contains("% of matches shared"))
                return Characters.DVE;

            if (rawHtml.Contains("Scooped blocks"))
                return Characters.Pointman;

            if (rawHtml.Contains("Erases"))
                return Characters.Eraser;

            if (rawHtml.Contains("Number of grey blocks hit:"))
                return Characters.Mono;

            return null;
        }

        private void ParseColorTable(IElement content, PlayerStats stats)
        {
            var table = content.QuerySelector("table#color_table tbody").QuerySelectorAll("tr");

            //magic numbers. If Audiosurf API returs shit, it will crushes. But Dylan is lazy dick so this is single way to do this
            int collectedRow = 0;
            int derivedPointsRow = 1;
            int totalTrafficPercentRow = 2;
            //

            var columnPtr = 0;
            foreach (var color in Enum.GetValues<BlockColor>())
            {
                stats.ColorStats[color].CollectedCount = int.Parse(table[collectedRow].QuerySelectorAll("td")[columnPtr].TextContent);
                stats.ColorStats[color].DerivedPointPercent = int.Parse(table[derivedPointsRow].QuerySelectorAll("td")[columnPtr].TextContent);
                stats.ColorStats[color].TotalTrafficCollectedPercent = int.Parse(table[totalTrafficPercentRow].QuerySelectorAll("td")[columnPtr].TextContent);
                ++columnPtr;
            }
        }
    }
}
