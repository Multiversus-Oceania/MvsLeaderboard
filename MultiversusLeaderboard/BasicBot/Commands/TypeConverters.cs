using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using Discord.Interactions;
using TypeReader = Discord.Commands.TypeReader;
using static BasicBot.MonarkTypes.TypeStatics;
using static BasicBot.Handler.User;

namespace BasicBot.Commands
{
    internal sealed class UlongConverter : TypeConverter<ulong>
    {
        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;
        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            if (ulong.TryParse((string)option.Value, out var result))
                return Task.FromResult(TypeConverterResult.FromSuccess(result));

            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"Value {option.Value} cannot be converted to ulong"));
        }

        public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
        {
            properties.Type = ApplicationCommandOptionType.String;
        }
    }
    internal sealed class GuildConverter : TypeConverter<SocketGuild>
    {
        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;
        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var @string = option.Value as string;
            ulong id = 0;
            if (!ulong.TryParse(@string, out id) || Program.discordClient == null)
            {
                _ = context.Interaction.SendMsg("Failed to parse socketguild", ephemeral: true);
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"Value {option.Value} cannot be converted to SocketGuild"));
            }
            var gld = Program.discordClient.GetGuild(id);
            if (gld == null)
            {
                _ = context.Interaction.SendMsg("Failed to find guild", ephemeral: true);
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"Value {option.Value} cannot be converted to SocketGuild"));
            }

            return Task.FromResult(TypeConverterResult.FromSuccess(gld));
        }

        public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
        {
            properties.Type = ApplicationCommandOptionType.String;
            properties.Choices = new List<ApplicationCommandOptionChoiceProperties> {
                CreateChoiceProperties("Overwatch OCE", "362611092893073408"),
                CreateChoiceProperties("Fortnite OCE", "362606684352413706"),
                CreateChoiceProperties("Valorant OCE", "537887361292304385"),
            };
        }
    }


    public class TimeSpanTypeReader : Discord.Commands.TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            return Task.FromResult(TypeReaderResult.FromSuccess(new TimeSpanConverter().Read(input)));
        }
    }
    public class TimeSpanConverter : TypeConverter<TimeSpan>
    {
        private readonly Dictionary<string, Func<string, TimeSpan>> _callback = new();

        private readonly Regex _regex = new(@"(\d*)\s*([a-zA-Z]*)\s*(?:and|,)?\s*", RegexOptions.Compiled);

        public TimeSpanConverter()
        {
            _callback["second"] = Seconds;
            _callback["seconds"] = Seconds;
            _callback["sec"] = Seconds;
            _callback["s"] = Seconds;

            _callback["minute"] = Minutes;
            _callback["minutes"] = Minutes;
            _callback["min"] = Minutes;
            _callback["m"] = Minutes;

            _callback["hour"] = Hours;
            _callback["hours"] = Hours;
            _callback["h"] = Hours;

            _callback["day"] = Days;
            _callback["days"] = Days;
            _callback["d"] = Days;

            _callback["week"] = Weeks;
            _callback["weeks"] = Weeks;
            _callback["w"] = Weeks;

            _callback["month"] = Months;
            _callback["months"] = Months;

            _callback["season"] = Seasons;
            _callback["seasons"] = Seasons;

            _callback["fortnite"] = Fortnits;
            _callback["fortnites"] = Fortnits;
            _callback["fortnight"] = Fortnits;
            _callback["fortnights"] = Fortnits;
            _callback["f"] = Fortnits;

            _callback["year"] = Years;
            _callback["years"] = Years;
            _callback["y"] = Years;

            _callback["perm"] = Perm;
            _callback["perma"] = Perm;
        }

        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var @string = option.Value as string;
            return Task.FromResult(TypeConverterResult.FromSuccess(Read(@string)));
        }

        public TimeSpan Read(string @string)
        {
            @string = @string?.ToLower().Trim();
            if (@string == "perma" || @string == "perm")
            {
                return TimeSpan.MaxValue;
            }

            TimeSpan span = new TimeSpan();
            
            MatchCollection matches = _regex.Matches(@string ?? string.Empty);
            if (matches.Any())
                foreach (Match match in matches)
                {
                    if (_callback.TryGetValue(match.Groups[2].Value, out var result))
                        span += result(match.Groups[1].Value);
                }

            return span;
        }

        private TimeSpan Seconds(string match)
            => TimeSpan.FromSeconds(int.Parse(match));

        private TimeSpan Minutes(string match)
            => TimeSpan.FromMinutes(int.Parse(match));

        private TimeSpan Hours(string match)
            => TimeSpan.FromHours(int.Parse(match));

        private TimeSpan Days(string match)
            => TimeSpan.FromDays(int.Parse(match));

        private TimeSpan Weeks(string match)
            => TimeSpan.FromDays(int.Parse(match) * 7);

        private TimeSpan Months(string match)
            => TimeSpan.FromDays(int.Parse(match) * 30);

        private TimeSpan Seasons(string match)
            => TimeSpan.FromDays(int.Parse(match) * 30 * 3);

        private TimeSpan Fortnits(string match)
            => TimeSpan.FromDays(int.Parse(match) * 7 * 2);

        private TimeSpan Years(string match)
            => TimeSpan.FromDays(int.Parse(match) * 365);



        //set ban forever
        private TimeSpan Perm(string match) =>
            TimeSpan.MaxValue;

        public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
        {
            properties.Type = ApplicationCommandOptionType.String;
            properties.Description += " Use the options above or write custom ones below";


            /*properties.Choices = new List<ApplicationCommandOptionChoiceProperties> {
                CreateChoiceProperties("30 Minutes"),
                CreateChoiceProperties("1 Hour"),
                CreateChoiceProperties("12 Hours"),
                CreateChoiceProperties("1 Day"),
                CreateChoiceProperties("3 Days"),
                CreateChoiceProperties("7 Days"),
                CreateChoiceProperties("14 Days"),
                CreateChoiceProperties("30 Days"),
                CreateChoiceProperties("Perm (Admin Only)", "perm"),
            };*/
        }
    }

    public class ListIntsConverter : TypeConverter<List<int>>
    {
        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var @string = option.Value as string;

            var value = Read(@string);

            if (value.Count == 0)
            {
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"Value {option.Value} cannot be converted to List<Int>"));
            }

            return Task.FromResult(TypeConverterResult.FromSuccess(value));
        }

        public List<int> Read(string @string)
        {
            List<int> span = new List<int>();
            @string = @string?.ToLower().Trim();
            var matches = @string.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            if (matches.Any())
                foreach 
                    (string match 
                    in matches)
                    if (GetInt(match) is int result)
                        span.Add(result);

            return span;
        }

        public int? GetInt(string str)
        {
            if (int.TryParse(str.Replace(" ", ""), out var result))
            {
                return result;
            }


            return null;
        }

        public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
        {
            properties.Type = ApplicationCommandOptionType.String;
            //properties.Description += " Use the options above or write custom ones below";


            /*properties.Choices = new List<ApplicationCommandOptionChoiceProperties> {
                CreateChoiceProperties("30 Minutes"),
                CreateChoiceProperties("1 Hour"),
                CreateChoiceProperties("12 Hours"),
                CreateChoiceProperties("1 Day"),
                CreateChoiceProperties("3 Days"),
                CreateChoiceProperties("7 Days"),
                CreateChoiceProperties("14 Days"),
                CreateChoiceProperties("30 Days"),
                CreateChoiceProperties("Perm (Admin Only)", "perm"),
            };*/
        }
    }
    public class ListstringConverter : TypeConverter<List<string>>
    {
        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var @string = option.Value as string;

            var value = Read(@string);

            if (value.Count == 0)
            {
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"Value {option.Value} cannot be converted to List<Int>"));
            }

            return Task.FromResult(TypeConverterResult.FromSuccess(value));
        }

        public List<string> Read(string @string)
        {
            List<string> span = new List<string>();
            @string = @string?.Trim();
            var matches = @string.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            if (matches.Any())
                foreach
                    (string match
                    in matches)
                    if (GetString(match) is string result)
                        span.Add(result);

            return span;
        }

        public string? GetString(string str)
        {
            str = str.Trim();
            if (str.Count() > 0)
            {
                return str;
            }


            return null;
        }

        public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
        {
            properties.Type = ApplicationCommandOptionType.String;
            //properties.Description += " Use the options above or write custom ones below";


            /*properties.Choices = new List<ApplicationCommandOptionChoiceProperties> {
                CreateChoiceProperties("30 Minutes"),
                CreateChoiceProperties("1 Hour"),
                CreateChoiceProperties("12 Hours"),
                CreateChoiceProperties("1 Day"),
                CreateChoiceProperties("3 Days"),
                CreateChoiceProperties("7 Days"),
                CreateChoiceProperties("14 Days"),
                CreateChoiceProperties("30 Days"),
                CreateChoiceProperties("Perm (Admin Only)", "perm"),
            };*/
        }
    }
}


