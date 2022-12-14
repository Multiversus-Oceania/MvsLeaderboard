// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do one of these:
//
//    using BasicBot.Handler;
//
//    var leaderboard = Leaderboard.FromJson(jsonString);
//    var token = Token.FromJson(jsonString);
//    var account = Account.FromJson(jsonString);
//    var profile = Profile.FromJson(jsonString);
//    var userSearch = UserSearch.FromJson(jsonString);

namespace BasicBot.Handler
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Leaderboard
    {
        [JsonProperty("rank")]
        public long Rank { get; set; }
    }
    
    public partial class Token
    {
        [JsonProperty("token")]
        public string TokenToken { get; set; }

        [JsonProperty("wb_network")]
        public TokenWbNetwork WbNetwork { get; set; }
    }

    public partial class TokenWbNetwork
    {
        [JsonProperty("network_token")]
        public string NetworkToken { get; set; }
    }
    
    public partial class Account
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("identity")]
        public Identity Identity { get; set; }
    }

    public partial class Identity
    {
        [JsonProperty("alternate")]
        public Alternate Alternate { get; set; }
    }

    public partial class Alternate
    {
        [JsonProperty("wb_network")]
        public List<WbNetwork> WbNetwork { get; set; }
    }

    public partial class WbNetwork
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public object Avatar { get; set; }
    }

    public partial class Profile
    {
        [JsonProperty("server_data")]
        public ServerData ServerData { get; set; }
    }

    public partial class ServerData
    {
        [JsonProperty("2v2shuffle")]
        public Shuffle DoublesShuffle { get; set; }

        [JsonProperty("1v1shuffle")]
        public Shuffle SinglesShuffle { get; set; }
    }

    public partial class Shuffle
    {
        [JsonProperty("0")]
        public Zero Zero { get; set; }
    }

    public partial class Zero
    {
        [JsonProperty("topRating")]
        public TopRating TopRating { get; set; }
    }

    public partial class TopRating
    {
        [JsonProperty("mean")]
        public double Mean { get; set; }

        [JsonProperty("deviance")]
        public double Deviance { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("streak")]
        public long Streak { get; set; }

        [JsonProperty("lastUpdateTimestamp")]
        public long LastUpdateTimestamp { get; set; }

        [JsonProperty("character")]
        public string Character { get; set; }
    }

    public partial class UserSearch
    {
        [JsonProperty("cursor")]
        public object Cursor { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("results")]
        public List<ResultElement> Results { get; set; }
    }

    public partial class ResultElement
    {
        [JsonProperty("score")]
        public object Score { get; set; }

        [JsonProperty("result")]
        public ResultResult Result { get; set; }
    }

    public partial class ResultResult
    {
        [JsonProperty("account_id")]
        public string Id { get; set; }
    }

    public partial class Leaderboard
    {
        public static Leaderboard FromJson(string json) => JsonConvert.DeserializeObject<Leaderboard>(json, BasicBot.Handler.Converter.Settings);
    }

    public partial class Token
    {
        public static Token FromJson(string json) => JsonConvert.DeserializeObject<Token>(json, BasicBot.Handler.Converter.Settings);
    }

    public partial class Account
    {
        public static Account FromJson(string json) => JsonConvert.DeserializeObject<Account>(json, BasicBot.Handler.Converter.Settings);
    }

    public partial class Profile
    {
        public static Profile FromJson(string json) => JsonConvert.DeserializeObject<Profile>(json, BasicBot.Handler.Converter.Settings);
    }

    public partial class UserSearch
    {
        public static UserSearch FromJson(string json) => JsonConvert.DeserializeObject<UserSearch>(json, BasicBot.Handler.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Leaderboard self) => JsonConvert.SerializeObject(self, BasicBot.Handler.Converter.Settings);
        public static string ToJson(this Token self) => JsonConvert.SerializeObject(self, BasicBot.Handler.Converter.Settings);
        public static string ToJson(this Account self) => JsonConvert.SerializeObject(self, BasicBot.Handler.Converter.Settings);
        public static string ToJson(this Profile self) => JsonConvert.SerializeObject(self, BasicBot.Handler.Converter.Settings);
        public static string ToJson(this UserSearch self) => JsonConvert.SerializeObject(self, BasicBot.Handler.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}