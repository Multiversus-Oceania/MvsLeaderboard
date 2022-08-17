using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using RestSharp;

namespace BasicBot.Handler;

public class MultiversusApiHandler
{
    private readonly string steamToken = Settings.GetSettings().HenrikApiToken;
    private const string baseURL = "https://dokken-api.wbagora.com/";
    private string AccessToken = "";

    private RestClient RestClient;

    public MultiversusApiHandler()
    {
        RestClient = new RestClient(baseURL);
        RestClient.AddDefaultHeaders(new Dictionary<string, string>()
        {
            { "x-hydra-api-key", "51586fdcbd214feb84b0e475b130fce0" },
            { "x-hydra-user-agent", "Hydra-Cpp/1.132.0" },
            { "Content-Type", "application/json" }
        });
    }

    public enum GameMode
    {
        Singles,
        Doubles
    }

    private string GetGameModeString(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Singles:
                return "1v1";
            case GameMode.Doubles:
                return "2v2";
        }

        return "1v1";
    }

    public async Task<PlayerMMR> GetMMRByName(string username, ulong discordId = 0)
    {
        var url = $"profiles/search_queries/get-by-username/run?username={username}";

        var Response = await Send(url);

        if (!Response.IsSuccessful)
            return null;

        if (JsonConvert.DeserializeObject<UserSearch>(Response.Content) is not UserSearch value)
        {
            return null;
        }

        if (value.Count == 0)
        {
            return null;
        }

        return await GetMMRById(value.Results[0].Result.Id, discordId);
    }

    public async Task<long> GetLeaderboardPosition(string id, GameMode mode)
    {
        var url = $"leaderboards/{GetGameModeString(mode)}/score-and-rank/{id}";

        var Response = await Send(url);

        if (!Response.IsSuccessful)
            return long.MaxValue;

        if (JsonConvert.DeserializeObject<Leaderboard>(Response.Content) is not Leaderboard value)
        {
            return long.MaxValue;
        }

        return value.Rank;
    }

    public async Task<PlayerMMR> GetMMRById(string id, ulong discordId = 0)
    {
        var profileResponse = await Send($"profiles/{id}");

        if (!profileResponse.IsSuccessful)
            return null;

        if (JsonConvert.DeserializeObject<Profile>(profileResponse.Content) is not Profile profile)
        {
            return null;
        }

        var accountResponse = await Send($"accounts/{id}");

        if (!accountResponse.IsSuccessful)
            return null;

        if (JsonConvert.DeserializeObject<Account>(accountResponse.Content) is not Account account)
        {
            return null;
        }

        var MMR = new PlayerMMR();

        MMR.Id = account.Id;
        MMR.Name = account.Identity.Alternate.WbNetwork[0].Username;
        MMR.DiscordId = discordId;

        if (profile.ServerData.SinglesShuffle != null)
            MMR.Singles = new MMR(profile.ServerData.SinglesShuffle.Zero.TopRating.Mean,
                await GetLeaderboardPosition(id, GameMode.Singles),
                profile.ServerData.SinglesShuffle.Zero.TopRating.Character);

        if (profile.ServerData.DoublesShuffle != null)
            MMR.Doubles = new MMR(profile.ServerData.DoublesShuffle.Zero.TopRating.Mean,
                await GetLeaderboardPosition(id, GameMode.Doubles),
                profile.ServerData.DoublesShuffle.Zero.TopRating.Character);

        return MMR;
    }

    private Task refreshTask;

    private async Task RefreshToken()
    {
        var client = new RestClient();
        var req = new RestRequest(baseURL + "access", Method.Post);
        req.AddHeaders(new Dictionary<string, string>()
        {
            { "x-hydra-api-key", "51586fdcbd214feb84b0e475b130fce0" },
            { "x-hydra-user-agent", "Hydra-Cpp/1.132.0" },
            { "Content-Type", "application/json" },
            { "x-hydra-client-id", "47201f31-a35f-498a-ae5b-e9915ecb411e" }
        });
        req.AddBody($"{{\"auth\":{{\"fail_on_missing\":1,\"steam\":\"{steamToken}\"}},\"options\":[\"wb_network\"]}}");

        await Ratelimit();

        var res = await client.ExecuteAsync(req);

        if (res.IsSuccessful)
        {
            if (JsonConvert.DeserializeObject<Token>(res.Content) is not Token token)
            {
                AccessToken = "";
                return;
            }

            AccessToken = token.TokenToken;
        }
    }

    private static RateLimiter limiter = null;

    private static async Task Ratelimit()
    {
        if (limiter == null)
        {
            int rateLimit = Settings.HenrikRateLimit();
            limiter = new TokenBucketRateLimiter(
                new TokenBucketRateLimiterOptions(rateLimit, QueueProcessingOrder.OldestFirst, 99999,
                    TimeSpan.FromSeconds(60), rateLimit));
        }

        // Create Lease
        using RateLimitLease lease = await limiter.WaitAsync(1);
        if (!lease.IsAcquired)
        {
            //setup a new task if failed
            await Task.Delay(TimeSpan.FromSeconds(60));
            await Ratelimit();
        }
    }

    private async Task<RestResponse> Send(string url, Method method = Method.Get, bool firstTry = true)
    {
        if (AccessToken == "")
        {
            if (refreshTask == null || refreshTask.IsCompleted)
            {
                Console.WriteLine("Fresh Token");
                refreshTask = RefreshToken();
            }

            await refreshTask;
        }

        var request = new RestRequest(url);

        request.AddHeader("x-hydra-access-token", AccessToken);

        request.Method = method;

        await Ratelimit();

        var response = await RestClient.ExecuteAsync(request);

        Console.WriteLine(method.ToString().ToUpper() + ": " + url);

        if (!response.IsSuccessful)
        {
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content);
            await RefreshToken();
            if (firstTry)
                return await Send(url, method, false);
            return response;
        }

        return response;
    }

    public class PlayerMMR : IMentionable
    {
        public string Id;
        public string Name;
        public MMR Singles;
        public MMR Doubles;
        public ulong DiscordId;

        public int RankValueSolo()
        {
            return (int)Singles.Rank;
        }

        public int RankValueDuo()
        {
            return (int)Doubles.Rank;
        }

        public double ScoreValueSolo()
        {
            return MathF.Round((float)Singles.Mmr);
        }

        public double ScoreValueDuo()
        {
            return MathF.Round((float)Doubles.Mmr);
        }

        public string SoloRank()
        {
            return $"MMR: {ScoreValueSolo()} \nRank: {RankValueSolo()}";
        }

        public string DuoRank()
        {
            return $"**MMR:** {ScoreValueDuo()} \n**Rank:** {RankValueDuo()}";
        }

        public string SoloChar()
        {
            return Singles.CharEmoji();
        }

        public string DuoChar()
        {
            return Doubles.CharEmoji();
        }

        public string Mention => $"<@{DiscordId}>";
    }

    public struct MMR
    {
        public MMR(double mmr, long rank, string character)
        {
            Mmr = mmr;
            Rank = rank;
            Character = character;
        }

        public double Mmr;
        public long Rank;
        public string Character;


        public string CharEmoji()
        {
            if (Character == null)
                return "";

            if (Settings.CharatorEmojiPair().ContainsKey(Character.ToLower()))
                return Settings.CharatorEmojiPair()[Character.ToLower()];

            return Character;
        }
    }
}