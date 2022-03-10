using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordRankingBot.Network;
using DiscordRankingBot.Network.Service;
using EventScheduler.Service;
using EventScheduler.Events;
using System.Data.SQLite;

namespace DiscordRankingBot.Services.Internal
{
    class ExamJob
    {
        private ISocketMessageChannel _channel;
        private SocketGuildUser _user;
        private LeaderboardHtmlParser _parser;
        private LeaderboardDetail _details;
        private Scheduler _eventScheduler;
        private IScheduledEvent _caller;
        private Goal _goal;
        private SQLiteConnection _database;

        public ExamJob(ISocketMessageChannel channel, SocketGuildUser user, LeaderboardDetail details, Scheduler scheduler, Goal goal, SQLiteConnection db)
        {
            _channel = channel;
            _user = user;
            _parser = new LeaderboardHtmlParser();
            _details = details;
            _eventScheduler = scheduler;
            _goal = goal;
            _database = db;
        }

        public void PinCaller(IScheduledEvent caller)
        {
            _caller = caller;
        }

        public void Execute()
        {
            if (_caller == null)
                Console.WriteLine("ExanHob Warning: caller isn't pinned so it may cause undefined behaviour");

            Task.Run(() =>
            {
                var stats = _parser.GetStats(_details.RideID, _details.Difficulty).GetAwaiter().GetResult().First(x => x.Nickname == _user.Nickname);
                if (stats == null)
                    return;

                if (_goal.Approve(stats))
                {
                    var query = @$"INSERT OR REPLACE INTO Users (nickname, perf_rating, is_apprentice) VALUES ('{_user.Nickname}', {stats.SkillRating}, 0)"; //is_apprentice = 0 -> user passed qualification exam
                    _database.Open();
                    var affected_rows = new SQLiteCommand(query, _database).ExecuteNonQuery();
                    Console.WriteLine($"[DATABASE] :: SQLite query executed, {affected_rows} affected");
                    _database.Close();

                    var embed = new EmbedBuilder()
                        .WithThumbnailUrl(_user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithTitle($"Puzzle Qualification Approve: {_user.Username}")
                        .WithColor(Color.Green)
                        .AddField(new EmbedFieldBuilder() { Name = "Character", Value = $"{stats.Character}" })
                        .AddField(new EmbedFieldBuilder() { Name = "Skill Rating: ", Value = $"{stats.SkillRating}" })
                        .WithCurrentTimestamp()
                        .Build();
                    var apprenticeRole = _user.Guild.Roles.First(x => x.Name == "Apprentice");
                    var memberRole = _user.Guild.Roles.First(x => x.Name == "Member");
                    _user.RemoveRoleAsync(apprenticeRole);
                    _user.AddRoleAsync(memberRole);
                    _channel.SendMessageAsync(null, false, embed).GetAwaiter().GetResult();
                    _eventScheduler.CancelTask(_caller);
                    StaticResoursesContainer.ActiveExams.Remove(_user.Username);
                }
            });
        }
    }
}
