using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using DiscordRankingBot.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data.SQLite;

namespace DiscordRankingBot
{
    class Program
    {
        public IConfigurationRoot Configuration { get; private set; }

        private SQLiteConnection _database;
        private IServiceCollection _services = new ServiceCollection();
        private ServiceProvider _serviceProvider;

        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private async Task MainAsync(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("config.json");
            Configuration = builder.Build();
            InitializeDatabase();
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

            _services.AddSingleton<Startup>().AddSingleton<CommandHandler>().AddSingleton(Configuration).AddSingleton(_database);
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(Configuration["default_database"]))
                SQLiteConnection.CreateFile(Configuration["default_database"]);

            _database = new SQLiteConnection($"Data Source={Configuration["default_database"]}; Version=3;");
            var queryGeneralInit = @"
            PRAGMA foreign_keys=on;

            CREATE TABLE IF NOT EXISTS Users (
            [id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            [nickname] TEXT NOT NULL,
            [perf_rating] INTEGER NOT NULL CHECK (PERF_RATING >= 0),
            [is_apprentice] BOOLEAN NOT NULL CHECK (is_apprentice in (0, 1)) )
";
            var command = new SQLiteCommand(queryGeneralInit, _database);
            _database.Open();
            command.ExecuteNonQuery();
            _database.Close();
        }
    }
}
