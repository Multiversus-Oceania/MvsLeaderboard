using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BasicBot.Commands.ModalCommand;
using BasicBot.Commands;
using Discord.Interactions;
using Discord.Rest;

namespace BasicBot.Handler
{
    public static class Discord
    {
        public static bool Contains(this IEnumerable<ulong> list, IEntity<ulong> entity)
            => list.Contains(entity.Id);

        public static bool Contains(this IEnumerable<ulong> list, IEnumerable<IEntity<ulong>> entitys)
            => entitys.Any(x => list.Contains(x.Id));

        public static bool Contains(this IEnumerable<ulong> list, IEnumerable<ulong> entitys)
            => entitys.Any(x => list.Contains(x));


        public static string DiscordTime(this TimeSpan time)
        {
            var str = new List<string>();

            if (time == TimeSpan.MaxValue)
            {
                return "Perm";
            }

            if (time.TotalDays >= 1)
            {
                var days = (int)time.TotalDays % 365;
                var years = (int)time.TotalDays / 365;

                if (years > 0)
                {
                    str.Add(DiscordTimeTime("year", years));
                }
                if (days > 0)
                {
                    str.Add(DiscordTimeTime("day", days));
                }
            }
            if (time.Hours > 0)
            {
                str.Add(DiscordTimeTime("hour", time.Hours));
            }
            if (time.Minutes > 0)
            {
                str.Add(DiscordTimeTime("minute", time.Minutes));
            }
            if (time.Seconds > 0)
            {
                str.Add(DiscordTimeTime("seccond", time.Seconds));
            }


            if (str.Count == 0)
            {
                return "Failed to parse";
            }


            return System.String.Join(", ", str);
        }

        public static string ReturnEmptyIfLess(string value, double size, int lowestPossible) =>
            ReturnEmptyIfLess(value, (int)size, lowestPossible);


        public static string ReturnEmptyIfLess(string value, int size, int lowestPossible)
        {
            if (size > lowestPossible)
            {
                return value;
            }

            return "";
        }

        public static string DiscordTimeTime(string time, int value)
        {
            return $"{value} {time}{ReturnEmptyIfLess("s", value, 1)}";
        }


        
    }
}
