using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using DiscordRankingBot.Services.Internal;
using EventScheduler.Events;
using DiscordRankingBot.Services;
using DiscordRankingBot.Network.Service;
using System.Data.SQLite;
using DiscordRankingBot.Network;
using System.Data.Common;
using DiscordRankingBot.Network.Service.Internal;
using System.Text;
using DiscordRankingBot;
using Discord;

using StaticResource = DiscordRankingBot.StaticResoursesContainer;

namespace DiscordRankingBot.CommandModules
{
    [Name("Exams manager")]
    [Summary("Command module for scheduling and tracking user qualification exams")]
    public class RankingModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private IConfigurationRoot _config;
        private Dictionary<string, IScheduledEvent> _scheduledExams;
        private Scheduler _eventScheduler;
        private SQLiteConnection _database;
        private LeaderboardHtmlParser _parser;

        public RankingModule(DiscordSocketClient client, IConfigurationRoot config, SQLiteConnection db)
        {
            _client = client;
            _config = config;
            _database = db;
            _scheduledExams = new Dictionary<string, IScheduledEvent>();
            _eventScheduler = new Scheduler();
            _parser = new LeaderboardHtmlParser();
        }

        [Command("exam")]
        public async Task StartExam([Remainder] string character)
        {
            if (character.ToLower() == "mono")
            {
                await ReplyAsync("What are you doing here wtf?");
                return;
            }    

            if (StaticResource.ActiveExams.ContainsKey(Context.User.Username))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Username}, You already passing exam!");
                return;
            }

            var goal = ParseGoal(character);
            var details = GetExamLeaderboard(character);

            if (goal == null || details == null)
            {
                await ReplyAsync("I dont understand what character you requested exam for. Please, check that you're spelled character name right and try again");
                return;
            }


            var examJob = new ExamJob(Context.Channel, Context.Guild.GetUser(Context.User.Id), details, _eventScheduler, goal, _database);
            var task = _eventScheduler.NewTask(examJob.Execute, new TimeSpan(0, 0, 10), 24);
            examJob.PinCaller(task);
            StaticResource.ActiveExams.Add(Context.User.Username, task);

            await Context.Channel.SendMessageAsync($"Your exam has started, {Context.User.Username}! I'll be check leaderboard for characted that you've selected every hour for next 24 hours. Good luck, samurai!");
        }

        [Command("apprentice")]
        public async Task AddUserAprrentice([Remainder] string nickname)
        {
            var query = @$"INSERT INTO Users(nickname, perf_rating, mavg_factor, is_apprentice) VALUES ('{nickname}', 0, 0, 1)";
            _database.Open();
            var affected_rows = new SQLiteCommand(query, _database).ExecuteNonQuery();
            _database.Close();
            var user = Context.Guild.Users.First(x => x.Nickname == nickname);
            Console.WriteLine($"[DATABASE] SQLite command executed, {affected_rows} rows affected");
            await ReplyAsync($@"{user.Mention}, You're an apprentice now! This role allows you to become a full member of out community before you pass entrance test (but remember, you anyway need to pass it!), but only if you'll still intersted in Audiosurf for while you here. Long inactive will cause you to lose your member role");

            
            await user.AddRoleAsync(Context.Guild.Roles.First(x => x.Name == "Member"));
            await user.AddRoleAsync(Context.Guild.Roles.First(x => x.Name == "Apprentice"));
        }
       
        [Command("summarize")]
        public async Task SummarizeChallange([Remainder] int challangeId)
        {
            try
            {
                var rawTop = await _parser.GetStats(challangeId, DifficultyHtmlClass.Elite);
                _database.Open();
                var reader = new SQLiteCommand(@"SELECT nickname, perf_rating FROM Users ORDER BY perf_rating", _database).ExecuteReader();
                
                var rawSkillRatings = new Dictionary<string, int>();
                var approvedTop = new List<IReadOnlyPlayerStats>();

                foreach (DbDataRecord record in reader)
                {
                    rawSkillRatings.Add(record["nickname"].ToString(), int.Parse(record["perf_rating"].ToString()));
                    if (rawTop.Contains(x => x.Nickname == record["nickname"].ToString()))
                    {
                        approvedTop.Add(rawTop.First(x => x.Nickname == record["nickname"].ToString()));
                    }
                }
                _database.Close();
                var rootRating = 3000 / approvedTop.First().SkillRating;

                foreach (var user in approvedTop.Select(x => (PlayerStats)x))
                    user.SkillRating *= rootRating;

                var reply = new StringBuilder();
                var counter = 1;
                reply.Append(DateTime.Now.ToString() + "\n");
                _database.Open();
                foreach (var player in approvedTop)
                {
                    var serverUser = Context.Guild.Users.First(x => x.Nickname == player.Nickname);
                    reply.Append($"{counter} place - {serverUser.Mention}, {player.SkillRating} performance rating, {player.Character}\n");
                    counter++;
                    
                    new SQLiteCommand(@$"UPDATE Users SET perf_rating = ((perf_rating + {player.SkillRating}) / 2) WHERE nickname = '{player.Nickname}'", _database).ExecuteNonQuery();
                }
                _database.Close();
                var channel = Context.Guild.GetChannel(942126586214359040) as ITextChannel; //TODO: fix magic numbers
                await channel.SendMessageAsync(reply.ToString());
            }
            catch (Exception e)
            { await ReplyAsync($"Something went wrong during execution command: {e.Message}"); _database.Close(); }
        }

        [Command("Leaderboard")]
        public async Task GenerateLeaderboard()
        {
            _database.Open();
            var reader = new SQLiteCommand(@"SELECT nickname, perf_rating FROM Users WHERE is_apprentice = 0 ORDER BY perf_rating", _database).ExecuteReader();
            var reply = new StringBuilder();
            foreach (DbDataRecord record in reader)
            {
                reply.Append($"{record["nickname"]} :: {record["perf_rating"]} PR\n");
            }
            _database.Close();
            await ReplyAsync(reply.ToString());
        }

        //======= "Exam" command Service methods ========//

        private Goal ParseGoal(string requestedCharacter)
        {
            if (requestedCharacter.ToLower() == "eraser")
                return Goal.Eraser;
            if (requestedCharacter.ToLower() == "pointman" || requestedCharacter.ToLower() == "pintman")
                return Goal.Pointman;
            if (requestedCharacter.ToLower() == "pusher")
                return Goal.Pusher;
            if (requestedCharacter.ToLower() == "dve")
                return Goal.DVE;
            return null;
        }

        private LeaderboardDetail GetExamLeaderboard(string character)
        {
            try
            {
                var result = new LeaderboardDetail();
                result.Difficulty = DifficultyHtmlClass.Elite;
                result.RideID = int.Parse(_config[character.ToString().ToLower()]);
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
