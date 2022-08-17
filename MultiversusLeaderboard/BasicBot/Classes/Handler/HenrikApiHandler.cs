using System;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading.RateLimiting;
using static BasicBot.Handler.HenrikApi;

namespace BasicBot.Handler;

public static class HenrikApi
{


    

    public enum GameMode
    {
        Singles,
        Duos
    }

    private static string GetGameModeString(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Singles:
                return "1v1";
            case GameMode.Duos:
                return "2v2";
        }

        return "1v1";
    }

    #region Leaderboard
    public static async Task<LeaderBoardPlacement> GetLeaderBoardPlacement(GameMode mode, string id)
    {
        var url = $"https://henrikdev.xyz/multiversus/v1/by-id/leaderboard-placements/{GetGameModeString(mode)}/{id}";

        var Response = await Send(url);

        if (!Response.IsSuccessful)
            return null;

        if (JsonConvert.DeserializeObject<LeaderBoardPlacement>(Response.Content) is not LeaderBoardPlacement value)
        {
            return null;
        }

        return value;
    }

    public class LeaderBoardPlacement
    {
        [JsonProperty("status")]
        public int Status;

        [JsonProperty("data")]
        public Data Stats;
        public class Data
        {
            [JsonProperty("id")]
            public string Id;

            [JsonProperty("username")]
            public string Username;

            [JsonProperty("mode")]
            public string Mode;

            [JsonProperty("rank")]
            public int? Rank;

            [JsonProperty("score")]
            public string Score;


            [JsonProperty("top_rating_character")]
            public TopRatingCharacter Charator;

            public string Fighter()
            {
                if (Charator != null)
                {
                    if (Charator.Name != null)
                        return Charator.Name;
                }

                return "";
            }

            public double ScoreDouble()
            {
                double s = 0;

                double.TryParse(Score, out s);

                return s;
            }

            public class TopRatingCharacter
            {
                [JsonProperty("name")]
                public string Name;

                [JsonProperty("xp")]
                public int Xp;

                [JsonProperty("level")]
                public int Level;
            }
        }
    }
    #endregion Leaderboard

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);



    public static async Task<Account> GetAccountById(string id)
    {
        var url = $"https://api.henrikdev.xyz/multiversus/v1/by-id/account/details/{id}";

        var Response = await Send(url);

        if (!Response.IsSuccessful)
            return null;

        if (JsonConvert.DeserializeObject<Account>(Response.Content) is not Account value)
        {
            return null;
        }

        return value;
    }

    public static async Task<Account> GetAccountByName(string id)
    {
        var url = $"https://api.henrikdev.xyz/multiversus/v1/account/details/{id}";

        var Response = await Send(url);

        if (!Response.IsSuccessful)
            return null;

        if (JsonConvert.DeserializeObject<Account>(Response.Content) is not Account value)
        {
            return null;
        }

        return value;
    }


    public class Account
    {
        [JsonProperty("status")]
        public int Status;

        [JsonProperty("data")]
        public Data Info;

        public class Data
        {
            [JsonProperty("id")]
            public string Id;

            [JsonProperty("username")]
            public string Username;

            [JsonProperty("created_at")]
            public int CreatedAt;

            [JsonProperty("level")]
            public int Level;

            [JsonProperty("xp")]
            public int Xp;

            [JsonProperty("online")]
            public bool Online;

            [JsonProperty("characters")]
            public List<Character> Characters;

            [JsonProperty("stats")]
            public Stats Stat;

            [JsonProperty("gamemodes")]
            public Gamemodes Modes;


            public class Gamemodes
            {
                [JsonProperty("1v1")]
                public _1v1 _1v1;

                [JsonProperty("2v2")]
                public _2v2 _2v2;
            }
            public class Ringouts
            {
                [JsonProperty("total")]
                public int Total;

                [JsonProperty("single_ringouts")]
                public int SingleRingouts;

                [JsonProperty("double_ringouts")]
                public int DoubleRingouts;
            }
            public class Stats
            {
                [JsonProperty("highest_damage_dealt")]
                public int HighestDamageDealt;

                [JsonProperty("assists")]
                public int Assists;

                [JsonProperty("ringouts")]
                public Ringouts Ringouts;

                [JsonProperty("wins")]
                public int Wins;

                [JsonProperty("dodged_attacks")]
                public int DodgedAttacks;

                [JsonProperty("damage")]
                public int Damage;

                [JsonProperty("matches")]
                public int Matches;
            }

            public class _1v1
            {
                [JsonProperty("wins")]
                public int Wins;

                [JsonProperty("win_streak")]
                public int WinStreak;

                [JsonProperty("longest_win_streak")]
                public int LongestWinStreak;

                [JsonProperty("losses")]
                public int Losses;
            }

            public class _2v2
            {
                [JsonProperty("wins")]
                public int Wins;

                [JsonProperty("win_streak")]
                public int WinStreak;

                [JsonProperty("longest_win_streak")]
                public int LongestWinStreak;

                [JsonProperty("losses")]
                public int Losses;
            }

            public class Character
            {
                [JsonProperty("agent")]
                public string Agent;

                [JsonProperty("xp")]
                public int Xp;

                [JsonProperty("level")]
                public int Level;

                [JsonProperty("wins")]
                public int Wins;
            }
        }

    }





    static RateLimiter limiter = null;
    static async Task Ratelimit()
    {
        if (limiter == null)
        {
            int rateLimit = Settings.HenrikRateLimit();
            limiter = new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions(rateLimit, QueueProcessingOrder.OldestFirst, 99999, TimeSpan.FromSeconds(60), rateLimit));
        }

        // Create Lease
        using RateLimitLease lease = await limiter.WaitAsync(permitCount: 1);
        if (!lease.IsAcquired)
        {
            //setup a new task if failed
            await Task.Delay(TimeSpan.FromSeconds(60));
            await Ratelimit();
        }
    }

    private static async Task<RestSharp.RestResponse> Send(string url, Method type = Method.Get)
    {
        var client = new RestClient(url);
        client.AddDefaultHeader("Authorization", Settings.HenrikApiToken());

        var request = new RestRequest();
        request.Method = type;

        //implement a ratelimit so we dont get a invalid request
        await Ratelimit();
        RestResponse response = await client.ExecuteAsync(request);

        return response;
    }
}