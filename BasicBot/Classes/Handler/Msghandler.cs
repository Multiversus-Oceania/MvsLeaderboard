using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BasicBot.Handler
{
    public static class Msg
    {


        public static int Count(this EmbedField embed) => embed.Name.Count() + embed.Value.Length;



        public static void DeleteAfter(this IMessage msg) => msg.DeleteAfter(TimeSpan.FromSeconds(10));
        public static void DeleteAfter(this IMessage msg, TimeSpan time)
        {
            //only deleted public followup messages
            _ = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(time);
                msg.DeleteAsync();
            });
        }


        public static async Task SendReplyDeleteAfter(this ModuleBase<CommandContext> module, string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
        {
            var msg = await module.Context.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
        }

        public static ComponentBuilder WithSelectMenus(this ComponentBuilder cmp, string customId, List<SelectMenuOptionBuilder> options, string placeholder = null, int minValues = 1, int maxValues = 1, bool disabled = false, int row = 0)
        {
            if (options.Count < 25)
            {
                cmp.WithSelectMenu(customId + ":0", options, placeholder, minValues, maxValues, disabled, row);
                return cmp;
            }


            var opt = options;
            int count = 0;

            while (opt.Count > 0)
            {
                count++;
                var _opt = opt.GetRange(0, Math.Min(25, opt.Count));

                foreach (var a in _opt)
                    opt.RemoveAt(0);

                var p = placeholder;

                if (p != null)
                    p += $" {count}";

                cmp.WithSelectMenu(customId+ $":{count}", _opt, p, 1, 1, disabled, row);
            }

            return cmp;
        }

    }
}
