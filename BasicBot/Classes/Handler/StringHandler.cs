using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BasicBot.Handler
{
    public static class String
    {
        public static string CombineCurrentDirectory(string file) =>
            Path.Combine(Environment.CurrentDirectory, file);

        public static EmbedBuilder ToEmbedBuilder(this string text) => TextToEmbedBuilder(text);
        public static EmbedBuilder TextToEmbedBuilder(this string text)
        {
            var embed = new EmbedBuilder().WithDescription(text);

            return embed;
        }

        public static Embed ToEmbed(this string text) => TextToEmbed(text);
        public static Embed TextToEmbed(this string text) => TextToEmbedBuilder(text).Build();

        public static string RemoveFirstString(string replace, string current, bool startWithCheck = false, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            if (startWithCheck)//if user wants to do a startwith check
                if (!current.ToLower().StartsWith(replace.ToLower())) //check if string starts with the replace
                    return current;//return current as replace does not start

            int index = current.IndexOf(replace, stringComparison);//get index of repalce //can return 0 or less if not found
            var newString = (index < 0) ? current //set new string as current if replace text not found
                : current.Remove(index, replace.Length);// return current string - repalce

            return newString;
        }

        public static string TextOnly(this string txt)
        {
            string t = "";

            foreach (var a in txt)
            {
                if (char.IsLetter(a) || a == ' ')
                    t += a;
            }
            return t;
        }

        public static int GetValueOrZero(this int? value)
        {
            if (value.HasValue)
            {
                return value.Value;
            }

            return 0;
        }

        #region Emote
        public static string RemoveEmotesFromText(string text)
        {
            while (GetRawEmoteFromText(text, out text) != null) ;

            text = text.Trim();
            return text;
        }

        public static List<Emote> GetEmotesFromText(string _text)
        {
            return GetEmotesFromText(_text, out var dump);
        }

        public static List<Emote> GetEmotesFromText(string _text, out string text)
        {
            text = _text;

            var emotes = new List<Emote>();

            string emoteRaw = GetRawEmoteFromText(text, out text);
            while (emoteRaw != null)
            {
                Emote emote = null;
                Emote.TryParse(emoteRaw, out emote);

                if (emote != null)
                    emotes.Add(emote);

                emoteRaw = GetRawEmoteFromText(text, out text);
            }

            text = text.Trim();
            return emotes;
        }

        public static string GetRawEmoteFromText(string _text, out string text)
        {

            //check for start of emote
            if (_text.Contains("<:"))
            {
                return GetRawEmoji(_text, out text);
            }
            else if (_text.Contains("<a:"))
            {
                return GetRawAnimatedEmoji(_text, out text);
            }
            else
            {
                text = _text;
                return null;
            }

        }

        private static string GetRawAnimatedEmoji(string _text, out string text)
        {
            text = _text;
            //check end of emote
            if (!text.Substring(text.IndexOf("<a:")).Contains(">"))
                return null;

            var emoteRaw = text.Substring(text.IndexOf("<a:"), text.IndexOf(">") - text.IndexOf("<a:") + 1);

            text = RemoveFirstString(emoteRaw, text);

            text = text.Trim();
            return emoteRaw;
        }

        private static string GetRawEmoji(string _text, out string text)
        {
            text = _text;
            //check end of emote
            if (!text.Substring(text.IndexOf("<:")).Contains(">"))
                return null;

            var emoteRaw = text.Substring(text.IndexOf("<:"), text.IndexOf(">") - text.IndexOf("<:") + 1);

            text = RemoveFirstString(emoteRaw, text);

            text = text.Trim();
            return emoteRaw;
        }

        public static string GetRawEmoteFromText(string text) =>
            GetRawEmoteFromText(text, out text);
        #endregion Emote

    }
}
