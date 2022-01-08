using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfigurationRoot _config;

        private ulong liamId = 391909581523255296; // these are safe defaults for my test server
        private ulong liamVoiceChannel = 572584426018570243; // they should be overwritten by the config file values
        string filePath = "liamLeaveCount.txt";
        public int liamLeaveCount = 0;

        private bool liamLeftRecently = false;
        Timer liamLeftRecentlyTimer;

        public LiamWatcherService(DiscordSocketClient discord, IConfigurationRoot config, Random random)
        {
            _discord = discord;
            _config = config;
            _random = random;

            liamId = ulong.Parse(_config["liamId"]);
            liamVoiceChannel = ulong.Parse(_config["liamVoiceChannelId"]);

            LoadData();

            _discord.UserVoiceStateUpdated += _discord_UserVoiceStateUpdated;

            _discord.SetActivityAsync(new Game("Liam. Obviously.", ActivityType.Watching));
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
                    liamLeftRecentlyTimer = new Timer(async delegate { Console.WriteLine("Timed out, he's gone."); liamLeftRecently = false; }, null, 300000, Timeout.Infinite);
                }
            }
        }

        private async Task PostAboutLiamRagequit()
        {
            var channel = _discord.GetChannel(liamVoiceChannel) as ITextChannel;

            string message = $"Liam has left voice {liamLeaveCount} times."; // default msg

            // lists for meme posting
            List<string> lowAngerList = new List<string>
            {
                $"Liam has left voice {liamLeaveCount} times.",
                $"Liam has left voice {liamLeaveCount} times. Wow.",
                $"Liam has left voice {liamLeaveCount} times. Has someone considered asking him to calm down?",
                $"That's {liamLeaveCount} times now. Has someone tried petting him? Yknow, like a dog. Yeah?",
                $"Once more doest our capricious Liam disappeareth from the voice channel and into the aether...",
                $"New high score! Liam has racked up {liamLeaveCount} ragequits!",
                $"One of these days I'll think of more lines for this. Just know that he left again. For the {liamLeaveCount}{GetCorrectOrdinal(liamLeaveCount)} time.",
                $"Wiam has weft voice {liamLeaveCount} times. Make suwe tuwu uwu at him now thawt he's bawck.",
                $"Once again, Liam has left voice. This is the {liamLeaveCount}{GetCorrectOrdinal(liamLeaveCount)} time. Oh shit, he's back, stop talking.",
                $"For the {liamLeaveCount}{GetCorrectOrdinal(liamLeaveCount)} time, Liam has ragequit from voice.",
            };
            List<string> highAngerList = new List<string>
            {
                $"This motherfucker has left {liamLeaveCount} times. Jesus christ.",
                $"Do you realize you've made me post this goddamned message {liamLeaveCount} times?",
                $"Go get some goddamned therapy. This is how many times? Oh wait, I've been tracking. {liamLeaveCount} times. What the fuck.",
                $"Liam has left voice {liamLeaveCount} times. This isn't ragequitlogs, bro.",
                $"Have you tried not being mad? Like, have you actually tried? ({liamLeaveCount} ragequits)",
                $"I know you're reading this. I'm not angry, I'm just disappointed. ({liamLeaveCount} ragequits)",
                $"There is no point pretending that you're not unmistakably fucked. Think about your life. ({liamLeaveCount} ragequits)",
                $"He's back! Okay, bro, deep breaths... deeeeep breaths.... ({liamLeaveCount} ragequits)",
            };

            var chanceToPostMemes = _random.Next(0, 100);
            if (CheckIfBetweenRange(chanceToPostMemes, 0, 90)) 
            {
                var memeListPick = _random.Next(0, lowAngerList.Count);
                message = lowAngerList[memeListPick];
            }
            if (CheckIfBetweenRange(chanceToPostMemes, 91, 99)) // rare
            {
                var memeListPick = _random.Next(0, highAngerList.Count);
                message = highAngerList[memeListPick];
            }

            // overrides for dumb shit
            if (liamLeaveCount.ToString().Contains("69") || liamLeaveCount.ToString().Contains("420"))
                message = $"That was ragequit number {liamLeaveCount}. Nice.";
            if (Math.Abs(liamLeaveCount) % 100 == 0) // every 100th
                message = $"This is cause for celebration. Liam has abandoned his friends in their time of comedic need {liamLeaveCount} times.";

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
                case 0:
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
