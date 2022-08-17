using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Services
{
    static class RankLeaderBoard
    {
        private static Timer timer;
        private static DiscordSocketClient _client;
        private static Dictionary<ulong, RankUser> RankCache = new Dictionary<ulong, RankUser>();

        public class RankObject
        {
            public int Rank;
            public double Score;
            public string Character;

            public RankObject(int rank, double score, string character)
            {
                Rank = rank;
                Score = score;
                Character = character;
            }

            public string CharEmoji()
            {
                if (Character == null)
                    return "";

                if (BasicBot.Handler.Settings.CharatorEmojiPair().ContainsKey(Character.ToLower()))
                    return BasicBot.Handler.Settings.CharatorEmojiPair()[Character.ToLower()];

                return Character;
            }

        }

        public class RankUser: IMentionable
        {
            public RankObject Singles;
            public RankObject Duos;
            public string Username;
            public string Id;
            public ulong DiscordId;

            //D is for duo, S is for singles
            public RankUser(ulong discordID, string username, string id, RankObject singles, RankObject duos)
            {
                Singles = singles;
                Duos = duos;
                Username = username;
                Id = id;
                DiscordId = discordID;
            }

            public int RankValueSolo()
            {
                int value = 0;
                if (Singles != null)
                    value += Singles.Rank;
                return value;
            }

            public int RankValueDuo()
            {
                int value = 0;
                if (Duos != null)
                    value += Duos.Rank;
                return value;
            }

            public double ScoreValueSolo()
            {
                double value = 0;
                if (Singles != null)
                    value += Singles.Score;
                return value;
            }

            public double ScoreValueDuo()
            {
                double value = 0;
                if (Duos != null)
                    value += Duos.Score;
                return value;
            }

            public string SoloRank()
            {
                if (Singles != null)
                    return $"MMR: {Singles.Score} \nRank: {Singles.Rank}";              

                return "no rank found";
            }
            public string DuoRank()
            {
                if (Duos != null)
                    return $"**MMR:** {Duos.Score} \n**Rank:** {Duos.Rank}";

                return "no rank found";
            }

            public string SoloChar()
            {
                if (Singles != null)
                    return Singles.CharEmoji();

                return "";
            }
            public string DuoChar()
            {
                if (Duos != null)
                    return Duos.CharEmoji();

                return "";
            }

            public string Mention => $"<@{DiscordId}>";
        }

        public static void Intilize(DiscordSocketClient client)
        {
            _client = client;
            StartTimers();
        }

        public static void StartTimers()
        {
            Console.WriteLine(DateTime.Now);

            //starts in 10s
            timer = new Timer(_ => OnCallBack(), null, TimeSpan.FromSeconds(10), TimeSpan.FromHours(BasicBot.Handler.Settings.LeaderboardUpdateTimeHours())); 
        }

        private static void OnCallBack()
        {
            _ = UpdateRank();
        }

        public static async Task<RankUser> MakeRankUser(SocketGuildUser user, string inGameId)
        {
            if (user.Username.ToLower().StartsWith("baro"))
                Console.WriteLine('i');

            Console.WriteLine(user.Username);
            string username = "unknown";
            string id = "0";
            RankObject solo = null;
            RankObject duos = null;

            //do solo request
            if (await BasicBot.Handler.HenrikApi.GetLeaderBoardPlacement(Handler.HenrikApi.GameMode.Singles, inGameId) is Handler.HenrikApi.LeaderBoardPlacement soloL)
            {
                username = soloL.Stats.Username;
                id = soloL.Stats.Id;
                int rank = 0;

                if (soloL.Stats.Rank.HasValue)
                    rank = soloL.Stats.Rank.Value;

                solo = new RankObject(rank, soloL.Stats.ScoreDouble(), soloL.Stats.Fighter());
            }

            //do duos request
            if (await BasicBot.Handler.HenrikApi.GetLeaderBoardPlacement(Handler.HenrikApi.GameMode.Duos, inGameId) is Handler.HenrikApi.LeaderBoardPlacement duoL)
            {
                username = duoL.Stats.Username;
                id = duoL.Stats.Id;
                int rank = 0;

                if (duoL.Stats.Rank.HasValue)
                    rank = duoL.Stats.Rank.Value;

                duos = new RankObject(rank, duoL.Stats.ScoreDouble(), duoL.Stats.Fighter());
            }

            return new RankUser(user.Id, username, id, solo, duos);
        }

        public static async Task UpdateRank()
        {
            //update any config changes
            BasicBot.Handler.Settings.LoadSettings();

            //get guild and channel
            if (_client.GetGuild(BasicBot.Handler.Settings.Guild()) is not SocketGuild gld)            
                return;         

            if (_client.GetChannel(BasicBot.Handler.Settings.LeaderboardChannel()) is not SocketTextChannel chnl)
                return;

            //create overide cache for later
            var _rankCache = new Dictionary<ulong, RankUser>();


            var SignedUpAccounts = BasicBot.Handler.Settings.GetSettings().UserAccounts;

            //save if discord made, or just save anyway
            BasicBot.Handler.Settings.SaveSettings();

            //run as many as possible at a time
            List<Task<RankUser>> tasks = new List<Task<RankUser>>();
            foreach (var user in gld.Users)
            {
                if (SignedUpAccounts.ContainsKey(user.Id))
                {
                    tasks.Add(MakeRankUser(user, SignedUpAccounts[user.Id]));
                }
            }
            //wait on all
            await Task.WhenAll(tasks);
            //cache all
            foreach (var a in await Task.WhenAll(tasks))
            {
                if (a is RankUser user)
                {
                    _rankCache[user.DiscordId] = user;
                }
            }

            //overide the current cache
            RankCache = _rankCache;

            
            await ClearChannel(chnl);
            await PostSolo(chnl);
            await PostDuo(chnl);
            await PostRegister(chnl);
        }

        private static async Task ClearChannel(SocketTextChannel chnl)
        {
            var msgs = await chnl.GetMessagesAsync(100).FlattenAsync();
            await chnl.DeleteMessagesAsync(msgs);
        }

        public static List<RankUser> GetMostSolo() =>
            RankCache.Values//.Where(x => x.Duos !=null && x.Singles != null)
            .OrderByDescending(x => x.ScoreValueSolo())
            .ThenByDescending(x => x.RankValueSolo())
            .ToList();

        public static List<RankUser> GetMostDuo() =>
            RankCache.Values//.Where(x => x.Duos !=null && x.Singles != null)
            .OrderByDescending(x => x.ScoreValueDuo())
            .ThenByDescending(x => x.RankValueDuo())
            .ToList();


        public static async Task PostRegister(SocketTextChannel chnl)
        {
            var mostRank = GetMostSolo();
            MonarkMessage msg = new EmbedBuilder().WithTitle("Register Below");

            ComponentBuilder buttons = new ComponentBuilder()
                .WithButton("Register/Update", "register", ButtonStyle.Primary);

            msg.Components = buttons.Build();

            await msg.SendMessage(chnl);
        }

        public static async Task PostSolo(SocketTextChannel chnl)
        {
            var mostRank = GetMostSolo();
            var msg = MakeSoloEmbed(mostRank.GetRange(0, Math.Min(20, mostRank.Count)));

            ComponentBuilder buttons = new ComponentBuilder()
                .WithButton("Find your rank", "solo", ButtonStyle.Primary, new Emoji("🏆"));

            msg.Components = buttons.Build();

            await msg.SendMessage(chnl);
        }

        public static async Task PostDuo(SocketTextChannel chnl)
        {
            

            
            var mostRank = GetMostDuo();
            var msg = MakeDuoEmbed(mostRank.GetRange(0, Math.Min(20, mostRank.Count)));

            ComponentBuilder buttons = new ComponentBuilder()
                .WithButton("Find your rank", "duo", ButtonStyle.Primary, new Emoji("🏆"));
            msg.Components = buttons.Build();

            await msg.SendMessage(chnl);
        }

        public static MonarkMessage GetSoloLeaderboardPlacement(ulong userID)
        {
            if (!RankCache.ContainsKey(userID))
            {
                return "currently not on the leaderboard, sign up and/or wait for the next leaderboard post";
            }

            var user = RankCache[userID];

            return BuildSoloPlacementEmbed(user.Id, GetMostSolo());
        }

        public static MonarkMessage GetDuoLeaderboardPlacement(ulong userID)
        {
            if (!RankCache.ContainsKey(userID))
            {
                return "currently not on the leaderboard, sign up and/or wait for the next leaderboard post";
            }

            var user = RankCache[userID];

            return BuildDuoPlacementEmbed(user.Id, GetMostDuo());
        }

        public static MonarkMessage BuildSoloPlacementEmbed(string id, List<RankUser> mostRank)
        {
            if (!mostRank.Any(x => x.Id == id))
                return "Failed to find user in list";


            var user = mostRank.FirstOrDefault(x => x.Id == id);
            var index = mostRank.IndexOf(user);
            index = Math.Max(0, index - 5);
            var _min = Math.Min(mostRank.Count, index + 11);
            var count = _min - index;

            return MakeSoloEmbed(mostRank.GetRange(index, count), index, id);
        }

        public static MonarkMessage BuildDuoPlacementEmbed(string id, List<RankUser> mostRank)
        {
            if (!mostRank.Any(x => x.Id == id))
                return "Failed to find user in list";


            var user = mostRank.FirstOrDefault(x => x.Id == id);
            var index = mostRank.IndexOf(user);
            index = Math.Max(0, index - 5);
            var _min = Math.Min(mostRank.Count, index + 11);
            var count = _min - index;

            return MakeDuoEmbed(mostRank.GetRange(index, count), index, id);
        }

        private static MonarkMessage MakeSoloEmbed(List<RankUser> users, int startNumb = 0, string UserHighlight = "")
        {
            var embed = new EmbedBuilder()
                .WithColor(115, 103, 240)
                .WithFooter("Updates once a hour")
                .WithTitle("Solo Leaderboard");


            int count = startNumb;
            foreach (var a in users)
            {
                count++;
                string heading = $"{BuildPlacement(count)} {a.Username}";
                string value = "not found";

                if (UserHighlight == a.Id)
                {
                    heading = $":arrow_right: {BuildPlacement(count)} ``{a.Username}``";
                }

                value = $"{a.SoloChar()}{a.Mention}\n{a.SoloRank()}";

                embed.AddField(heading, value, true);
            }

            return embed;
        }

        private static MonarkMessage MakeDuoEmbed(List<RankUser> users, int startNumb = 0, string UserHighlight = "")
        {
            var embed = new EmbedBuilder()
                .WithColor(115, 103, 240)
                .WithFooter("Updates once a hour")
                .WithTitle("Duo Leaderboard");

            int count = startNumb;
            foreach (var a in users)
            {
                count++;
                string heading = $"{BuildPlacement(count)} {a.Username}";// \n{a.Mention}\n";
                string value = "not found";

                if (UserHighlight == a.Id)
                {
                    heading = $":arrow_right: {BuildPlacement(count)} ``{a.Username}``";//\n{a.Mention}\n";
                }

                value = $"{a.DuoChar()}{a.Mention}\n{a.DuoRank()}";

                embed.AddField(heading, value, true);
            }

            return embed;
        }

        public static string BuildPlacement(int place, string addbefore = "")
        {
            if (place == 1)
                return $":first_place: {addbefore}";
            else if (place == 2)
                return $":second_place: {addbefore}";
            else if (place == 3)
                return $":third_place: {addbefore}";
            return $"{addbefore}{place}.";
        }
    }
}
