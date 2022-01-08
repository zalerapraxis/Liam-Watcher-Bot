using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Liam_Watcher_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liam_Watcher_Bot.Modules
{
    public class LiamWatcherModule : ModuleBase<SocketCommandContext>
    {
        public LiamWatcherService LiamWatcherService {get; set;}

        [Command("leave")]
        [Alias("liam")]
        [Summary("Get how many times Liam has left voice.")]
        public async Task ReportLiamLeaveCount()
        {
            await ReplyAsync($"Liam has left voice {LiamWatcherService.liamLeaveCount} times.");
        }
    }
}
