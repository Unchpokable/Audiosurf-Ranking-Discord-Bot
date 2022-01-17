using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using DiscordRankingBot.Services;
using Microsoft.Extensions.Configuration;

namespace DiscordRankingBot
{
    class Program
    {
        public IConfigurationRoot Configuration { get; private set; }

        private IServiceCollection _services = new ServiceCollection();
        private ServiceProvider _serviceProvider;

        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private async Task MainAsync(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("config.json");
            Configuration = builder.Build();
            ConfigureServices();
            _serviceProvider = _services.BuildServiceProvider();

            _serviceProvider.GetRequiredService<CommandHandler>();

            await _serviceProvider.GetRequiredService<Startup>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices()
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100
            });
            client.Log += (log) =>
            {
                Console.WriteLine($"[BOT:{log.Severity}] {log.Message}");
                return Task.CompletedTask;
            };

            _services.AddSingleton(client);

            _services.AddSingleton(new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async
            }));

            _services.AddSingleton<Startup>().AddSingleton<CommandHandler>().AddSingleton(Configuration);
        }
    }
}
