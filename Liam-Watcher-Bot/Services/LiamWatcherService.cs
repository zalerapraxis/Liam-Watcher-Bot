using Discord;
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
    public class LiamWatcherService
    {
        private readonly DiscordSocketClient _discord;
        private readonly Random _random;

        private ulong liamId = 175078909539254273; // testing mine for now, liam's id is 175078909539254273
        private ulong liamVoiceChannel = 783345721100337184;
        string filePath = "liamLeaveCount.txt";
        public int liamLeaveCount = 0;

        private bool liamLeftRecently = false;
        Timer liamLeftRecentlyTimer;

        public LiamWatcherService(DiscordSocketClient discord, Random random)
        {
            _discord = discord;
            _random = random;
            
            LoadData();

            _discord.UserVoiceStateUpdated += _discord_UserVoiceStateUpdated;
        }

        private async Task SaveData()
        {
            Console.Write("Saving... ");
            await File.WriteAllTextAsync(filePath, liamLeaveCount.ToString());
            Console.WriteLine("Done.");
        }

        private async Task LoadData()
        {
            Console.Write("Loading... ");
            if (File.Exists(filePath))
            {
                var fileContents = await File.ReadAllTextAsync(filePath);
                liamLeaveCount = int.Parse(fileContents);
            }
            Console.WriteLine("Done.");
        }

        private async Task _discord_UserVoiceStateUpdated(SocketUser user, SocketVoiceState stateBefore, SocketVoiceState stateAfter)
        {
            if (user.Id == liamId)
            {
                // has joined voice channel and had recently disconnected
                if (stateBefore.VoiceChannel == null && liamLeftRecently == true)
                {
                    Console.WriteLine($"Liam left a voice channel and then rejoined. Incrementing counter.");
                    liamLeaveCount += 1;

                    // reset timer and state
                    liamLeftRecently = false;
                    liamLeftRecentlyTimer.Dispose();

                    // save data
                    SaveData();
                    PostAboutLiamRagequit();
                }

                // has disconnected from voice channel
                if (stateAfter.VoiceChannel == null)
                {
                    Console.WriteLine($"Liam left a voice channel. Starting timer...");
                    liamLeftRecently = true;
                    liamLeftRecentlyTimer = new Timer(async delegate { liamLeftRecently = false; }, null, 300000, Timeout.Infinite);
                }
            }
        }

        private async Task PostAboutLiamRagequit()
        {
            var channel = _discord.GetChannel(liamVoiceChannel) as ITextChannel;
            string message = $"Liam has left voice {liamLeaveCount} times.";

            List<string> lowAngerList = new List<string>
            {
                $"Liam has left voice {liamLeaveCount} times. Wow.",
                $"Liam has left voice {liamLeaveCount} times. Has someone considered asking him to calm down?",
                $"Aaaand there he goes again. That's {liamLeaveCount} times now.",
                $"Once more doest our capricious Liam disappeareth from the voice channel and into the aether.",
                $"New high score! Liam has wracked up {liamLeaveCount} ragequits!",
                $"One of these days I'll think of more lines for this. Just know that he left again. For the {liamLeaveCount}{GetCorrectOrdinal(liamLeaveCount)} time."
            };

            List<string> highAngerList = new List<string>
            {
                $"This motherfucker has left {liamLeaveCount} times. Jesus christ.",
                $"Do you realize you've made me post this goddamned message {liamLeaveCount} times?",
                $"Go get some goddamned therapy. This is how many times? Oh wait, I've been tracking. {liamLeaveCount} times. What the fuck.",
                $"Once again, Liam has left voice. This is the {liamLeaveCount}{GetCorrectOrdinal(liamLeaveCount)} time.",
                $"For the {liamLeaveCount}{GetCorrectOrdinal(liamLeaveCount)} time, Liam has ragequit from voice.",
                $"Liam has left voice {liamLeaveCount} times. This isn't ragequitlogs, bro.",
            };

            var chanceToPostMemes = _random.Next(0, 100);
            if (CheckIfBetweenRange(chanceToPostMemes, 70, 95)) // moderately rare but not terribly so
            {
                var memeListPick = _random.Next(0, lowAngerList.Count);
                message = lowAngerList[memeListPick];
            }
            if (CheckIfBetweenRange(chanceToPostMemes, 96, 99)) // rare
            {
                var memeListPick = _random.Next(0, highAngerList.Count);
                message = highAngerList[memeListPick];
            }


            await channel.SendMessageAsync(message);
        }

        private bool CheckIfBetweenRange(int numberToCheck, int bottom, int top)
        {
            return (numberToCheck >= bottom && numberToCheck <= top);
        }

        private string GetCorrectOrdinal(int numberToCheck)
        {
            int lastNumber = Math.Abs(numberToCheck) % 10; // get last digit of number

            switch (lastNumber)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return "th";

            }

            return "";
        }
    }
}
