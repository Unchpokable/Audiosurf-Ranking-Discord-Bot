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
    internal class ServiceBase
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly DiscordSocketClient _client;
        protected readonly CommandService _commandService;
        protected readonly IConfigurationRoot _config;

        public ServiceBase(IServiceProvider servProvider,
                       DiscordSocketClient client,
                       CommandService commandService,
                       IConfigurationRoot config)
        {
            _serviceProvider = servProvider;
            _client = client;
            _commandService = commandService;
            _config = config;
        }
    }
}
