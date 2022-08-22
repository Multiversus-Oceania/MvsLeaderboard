using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicBot.Settings;
using static BasicBot.Handler.String;

namespace BasicBot.Handler
{
    public static class Settings
    {
        private static Bot BotSettings;

        private static readonly string SettingsFile = CombineCurrentDirectory("settings.json");

        #region bot settings
        public static Bot LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                var jsonText = File.ReadAllText(SettingsFile);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    return JsonConvert.DeserializeObject<Bot>(jsonText);
                }
            }
            else
            {
                BotSettings = new Bot { 
                    BotToken = "MTAxMTIzMDU4MzIxODc3NDE0Ng.GAeBJY.7HW5qm2ZRMBSKzJNO-61hSdIJKwLByDDNc9xPw", 
                    BotOwners = new List<ulong> { 0, 1 }, 
                    BotPrefix = "?", 
                    CharatorsEmojiPair = new Dictionary<string, string>(), 
                    GuildId = 0, 
                    HenrikApiToken = "080110d4da82d0011800203a2a7007ad8225ae31014b53c68fc7b2c65b8df85b9a2c85c5de8a0047b70792da6881eb60054267032e517a260536491e6b2e44adf1d1c950a82f8b56a4fcd969bee7e256ee2b4926d99e21deaf6eba769846c2a8e2656a946f14e906f6642c92ab53b87f0c785a21beb249b5d753f234ccea", 
                    HenrikRateLimit = 400, 
                    LeaderboardChannel = 0,
                    LeaderboardUpdateTimeHours = 2,
                    UserAccounts = new Dictionary<ulong, string>()
                };
                SaveSettings();
            }
            return null;
        }

        public static Bot GetSettings()
        {
            if (BotSettings == null)
            {
                BotSettings = LoadSettings();
            }
            return BotSettings;
        }

        public static bool SaveSettings()
        {
            if (BotSettings != null)
            {
                var jsonText = JsonConvert.SerializeObject(BotSettings, Formatting.Indented);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    File.WriteAllText(SettingsFile, jsonText);
                    return true;
                }
            }
            return false;
        }

        public static string GetPrefix() =>
            GetSettings().BotPrefix;

        public static bool IsBotOwner(ulong id) =>
            GetSettings().BotOwners.Any(x => x == id);

        public static List<ulong> GetBotOwners() =>
            GetSettings().BotOwners;

        public static int HenrikRateLimit() =>
            GetSettings().HenrikRateLimit;

        public static string HenrikApiToken() =>
            GetSettings().HenrikApiToken;

        public static ulong LeaderboardChannel() =>
            GetSettings().LeaderboardChannel;

        public static ulong Guild() =>
            GetSettings().GuildId;

        public static Dictionary<string, string> CharatorEmojiPair() =>
             GetSettings().CharatorsEmojiPair;
        public static int LeaderboardUpdateTimeHours() =>
             GetSettings().LeaderboardUpdateTimeHours;

        #endregion bot settings
    }
}
