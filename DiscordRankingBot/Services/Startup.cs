using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordRankingBot.Services
{
    class Startup : ServiceBase
    {
        private readonly string _defaultConfigTokenKey = "token";

        public Startup(IServiceProvider servProvider,
                       DiscordSocketClient client,
                       CommandService commandService,
                       IConfigurationRoot config) 
            : base(servProvider, client, commandService, config) {  }

        public async Task StartAsync()
        {
            string token = _config[_defaultConfigTokenKey];
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Can not found token in configuration file. Please, check config key that contains token");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }
    }
}
