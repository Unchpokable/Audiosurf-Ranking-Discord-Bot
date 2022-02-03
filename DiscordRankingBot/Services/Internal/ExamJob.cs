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

        public ExamJob(ISocketMessageChannel channel, SocketGuildUser user, LeaderboardDetail details, Scheduler scheduler, Goal goal)
        {
            _channel = channel;
            _user = user;
            _parser = new LeaderboardHtmlParser();
            _details = details;
            _eventScheduler = scheduler;
            _goal = goal;
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

                    var embed = new EmbedBuilder()
                        .WithThumbnailUrl(_user.GetAvatarUrl() ?? _user.GetDefaultAvatarUrl())
                        .WithTitle($"Puzzle Qualification Approve: {_user.Username}")
                        .WithColor(Color.Green)
                        .AddField(new EmbedFieldBuilder() { Name = "Character", Value = $"{stats.Character}" })
                        .AddField(new EmbedFieldBuilder() { Name = "Skill Rating: ", Value = $"{stats.SkillRating}" })
                        .WithCurrentTimestamp()
                        .Build();

                    _channel.SendMessageAsync(null, false, embed).GetAwaiter().GetResult();
                    _eventScheduler.CancelTask(_caller);
                    StaticResoursesContainer.ActiveExams.Remove(_user.Username);
                }
            });
        }
    }
}
