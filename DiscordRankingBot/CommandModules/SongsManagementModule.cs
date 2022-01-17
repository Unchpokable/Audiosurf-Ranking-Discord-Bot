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

namespace DiscordRankingBot.CommandModules
{
    [Name("Songs Manager")]
    [Summary("Command module for management ranking songs")]
    public class SongsManagementModule : ModuleBase<SocketCommandContext>
    {
        private IConfigurationRoot _config;
        private DiscordSocketClient _client;
        private Regex UrlMatchRegex = new Regex(@"(?i)(http(s)?:\/\/)(\w{2,25}\.)+\w{3}([a-z0-9\-?=$-_.+!*()]+)(?i)", RegexOptions.Singleline);
        public SongsManagementModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("approve")]
        public async Task Approve([Remainder] ulong id)
        {
            Console.WriteLine("approving...");
            var sourceMsg = await Context.Channel.GetMessageAsync(id);
            if (sourceMsg == null)
                return;

            var targetChannel = _client.GetChannel(ulong.Parse(_config["rankedSongsId"])) as IMessageChannel;
            if (sourceMsg.Attachments.Count > 0)
            {
                await Context.Channel.SendMessageAsync("Currently bot API version can not approve offer with directly attached file. Please, use long-term cloud storages like GoogleDrive in your offer");
                return;
            }
            if (UrlMatchRegex.IsMatch(sourceMsg.Content))
            {
                await targetChannel.SendMessageAsync(sourceMsg.Content);
                return;
            }
            await Context.Channel.SendMessageAsync("Approving offers without any cloud storage URL or with Unsafe URL (not https://...) is not allowed");
        }

        [Command("decline")]
        public async Task Decline([Remainder] ulong id)
        {
            var sourceMsg = await Context.Channel.GetMessageAsync(id);
            await Context.Channel.DeleteMessageAsync(id);
        }
    }
}
