using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Liam_Watcher_Bot.Services
{
    class LiamWatcherService
    {
        private readonly DiscordSocketClient _discord;

        private ulong liamId = 110866678161645568; // testing mine for now, liam's id is 175078909539254273
        private int liamLeaveCount = 0;
        string filePath = "liamLeaveCount.txt";

        public LiamWatcherService(DiscordSocketClient discord)
        {
            _discord = discord;
            
            LoadData();

            _discord.UserVoiceStateUpdated += _discord_UserVoiceStateUpdated;

            Timer SaveDataTimer = new Timer(async delegate { await SaveData(); }, null, 10000, 10000);
        }

        private async Task SaveData()
        {
            Console.Write("Saving... ");
            await File.WriteAllTextAsync(filePath, liamLeaveCount.ToString());
            Console.WriteLine("Done");
        }

        private async Task LoadData()
        {
            if (File.Exists(filePath))
            {
                var fileContents = await File.ReadAllTextAsync(filePath);
                liamLeaveCount = int.Parse(fileContents);
            }
        }

        private async Task _discord_UserVoiceStateUpdated(SocketUser user, SocketVoiceState stateBefore, SocketVoiceState stateAfter)
        {
            if (user.Id == liamId)
                if (stateAfter.VoiceChannel == null)
                {
                    liamLeaveCount += 1;
                    Console.WriteLine($"Liam leave count incremented to {liamLeaveCount}.");
                }
                    
        }
    }
}
