using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot.Services
{
    class CommandHandler : ServiceBase
    {
        public CommandHandler(IServiceProvider servProvider,
                              DiscordSocketClient client,
                              CommandService commandService,
                              IConfigurationRoot config) : base(servProvider, client, commandService, config)
        {
            _client.MessageReceived += OnMessageRecieved;
        }

        private async Task OnMessageRecieved(SocketMessage socketMessage)
        {
            var msg = socketMessage as SocketUserMessage;

            if (msg == null) return;
            if (msg.Author.Id == _client.CurrentUser.Id) return; //Is it myself?

            var context = new SocketCommandContext(_client, msg);

            var argPos = 0;
            if (msg.HasStringPrefix(_config["prefix"], ref argPos))
            {
                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider).ConfigureAwait(true);

                if (!result.IsSuccess)
                {
                    await context.Channel.SendMessageAsync("Command failed. Detailed failure description check in log file");
                    return;
                }
                //Come that day, when i add logging here. But not now 
                Console.WriteLine($"executed {msg.Content}");
            }
        }
    }
}
