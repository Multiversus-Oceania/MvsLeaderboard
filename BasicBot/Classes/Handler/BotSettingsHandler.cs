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
                    BotToken = ${{ secrets.BotToken }}, 
                    BotOwners = new List<ulong> { 0, 1 }, 
                    BotPrefix = "?", 
                    CharatorsEmojiPair = new Dictionary<string, string>(), 
                    GuildId = 0, 
                    HenrikApiToken = ${{ secrets.HenrikApiToken }}, 
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
