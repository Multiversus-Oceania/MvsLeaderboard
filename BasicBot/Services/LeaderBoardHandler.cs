using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BasicBot.Handler;
using Discord;
using Discord.WebSocket;
using static BasicBot.Handler.MultiversusApiHandler;
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Services
{
    internal static class RankLeaderBoard
    {
        private static Timer timer;
        private static DiscordSocketClient _client;
        private static Dictionary<ulong, PlayerMMR> RankCache = new();
        private static MultiversusApiHandler ApiHandler = new();

        public static void Intilize(DiscordSocketClient client)
        {
            _client = client;
            StartTimers();
        }

        public static void StartTimers()
        {
            Console.WriteLine(DateTime.Now);

            //starts in 10s
            timer = new Timer(_ => OnCallBack(), null, TimeSpan.FromSeconds(10),
                TimeSpan.FromHours(Handler.Settings.LeaderboardUpdateTimeHours()));
        }

        private static void OnCallBack()
        {
            _ = UpdateRank();
        }

        public static async Task UpdateRank()
        {
            //update any config changes
            Handler.Settings.LoadSettings();

            //get guild and channel
            if (_client.GetGuild(Handler.Settings.Guild()) is not SocketGuild gld)
                return;

            if (_client.GetChannel(Handler.Settings.LeaderboardChannel()) is not SocketTextChannel chnl)
                return;

            //create overide cache for later
            var _rankCache = new Dictionary<ulong, PlayerMMR>();


            var SignedUpAccounts = Handler.Settings.GetSettings().UserAccounts;

            //save if discord made, or just save anyway
            Handler.Settings.SaveSettings();

            //run as many as possible at a time
            List<PlayerMMR> players = new List<PlayerMMR>();
            foreach (var user in gld.Users)
            {
                if (SignedUpAccounts.ContainsKey(user.Id))
                {
                    var p = await ApiHandler.GetMMRById(SignedUpAccounts[user.Id], discordId: user.Id);
                    if (p != null)
                        players.Add(p);
                }
            }

            //cache all
            foreach (var a in players)
            {
                if (a is PlayerMMR user)
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

        public static List<PlayerMMR> GetMostSolo()
        {
            return RankCache.Values //.Where(x => x.Duos !=null && x.Singles != null)
                .OrderByDescending(x => x.ScoreValueSolo())
                .ThenByDescending(x => x.RankValueSolo())
                .ToList();
        }

        public static List<PlayerMMR> GetMostDuo()
        {
            return RankCache.Values //.Where(x => x.Duos !=null && x.Singles != null)
                .OrderByDescending(x => x.ScoreValueDuo())
                .ThenByDescending(x => x.RankValueDuo())
                .ToList();
        }


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

        public static MonarkMessage BuildSoloPlacementEmbed(string id, List<PlayerMMR> mostRank)
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

        public static MonarkMessage BuildDuoPlacementEmbed(string id, List<PlayerMMR> mostRank)
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

        private static MonarkMessage MakeSoloEmbed(List<PlayerMMR> users, int startNumb = 0, string UserHighlight = "")
        {
            var embed = new EmbedBuilder()
                .WithColor(115, 103, 240)
                .WithFooter("Updates once a hour")
                .WithTitle("Solo Leaderboard");


            int count = startNumb;
            foreach (var a in users)
            {
                count++;
                string heading = $"{BuildPlacement(count)} {a.Name}";
                string value = "not found";

                if (UserHighlight == a.Id)
                {
                    heading = $":arrow_right: {BuildPlacement(count)} ``{a.Name}``";
                }

                value = $"{a.SoloChar()}{a.Mention}\n{a.SoloRank()}";

                embed.AddField(heading, value, true);
            }

            return embed;
        }

        private static MonarkMessage MakeDuoEmbed(List<PlayerMMR> users, int startNumb = 0, string UserHighlight = "")
        {
            var embed = new EmbedBuilder()
                .WithColor(115, 103, 240)
                .WithFooter("Updates once a hour")
                .WithTitle("Duo Leaderboard");

            int count = startNumb;
            foreach (var a in users)
            {
                count++;
                string heading = $"{BuildPlacement(count)} {a.Name}"; // \n{a.Mention}\n";
                string value = "not found";

                if (UserHighlight == a.Id)
                {
                    heading = $":arrow_right: {BuildPlacement(count)} ``{a.Name}``"; //\n{a.Mention}\n";
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