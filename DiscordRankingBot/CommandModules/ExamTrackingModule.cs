using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using DiscordRankingBot.Services.Internal;
using EventScheduler.Events;
using EventScheduler.Service;
using DiscordRankingBot.Services;

using StaticResource = DiscordRankingBot.StaticResoursesContainer;
using DiscordRankingBot.Network.Service;

namespace DiscordRankingBot.CommandModules
{
    [Name("Exams manager")]
    [Summary("Command module for scheduling and tracking user qualification exams")]
    public class ExamTrackingModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private IConfigurationRoot _config;
        private Dictionary<string, IScheduledEvent> _scheduledExams;
        private Scheduler _eventScheduler;

        public ExamTrackingModule(DiscordSocketClient client, IConfigurationRoot config)
        {
            _client = client;
            _config = config;
            _scheduledExams = new Dictionary<string, IScheduledEvent>();
            _eventScheduler = new Scheduler();
        }

        [Command("exam")]
        public async Task StartExam([Remainder] string character)
        {
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


            var examJob = new ExamJob(Context.Channel, Context.Guild.GetUser(Context.User.Id), details, _eventScheduler, goal);
            var task = _eventScheduler.NewTask(examJob.Execute, new TimeSpan(0, 10, 0), 24);
            examJob.PinCaller(task);
            StaticResource.ActiveExams.Add(Context.User.Username, task);

            await Context.Channel.SendMessageAsync($"Your exam has started, {Context.User.Username}! I'll be check leaderboard for characted that you've selected every hour for next 24 hours. Good luck, samurai!");
        }

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
