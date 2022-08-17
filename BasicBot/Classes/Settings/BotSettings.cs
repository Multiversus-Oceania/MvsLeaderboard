using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicBot.Settings
{
    public class Bot
    {
        [JsonProperty]
        public string BotToken { get; internal set; }
        [JsonProperty]
        public string BotPrefix { get; internal set; }
        [JsonProperty]
        public List<ulong> BotOwners { get; internal set; }
        [JsonProperty]
        public string HenrikApiToken { get; internal set; }
        [JsonProperty]
        public int HenrikRateLimit { get; internal set; }
        [JsonProperty]
        public int LeaderboardUpdateTimeHours { get; internal set; }
        [JsonProperty]
        public ulong GuildId { get; internal set; }
        [JsonProperty]
        public ulong LeaderboardChannel { get; internal set; }
        [JsonProperty]
        public Dictionary<string, string> CharatorsEmojiPair = new Dictionary<string, string>();

        [JsonProperty]
        public Dictionary<ulong, string> UserAccounts = new();
    }
}
