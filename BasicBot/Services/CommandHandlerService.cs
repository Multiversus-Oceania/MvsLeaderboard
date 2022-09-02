using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.Interactions;
using BasicBot.Commands;
using static BasicBot.Handler.Settings;
using BasicBot.Handler;
using System.Security.Cryptography.X509Certificates;

namespace BasicBot.Services
{
    public class CommandHandlerService
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private readonly IServiceProvider provider;
        public static InteractionService interactionService;

        public async Task InitializeAsync()
        {
            interactionService.AddTypeConverter<TimeSpan>(new TimeSpanConverter());
            interactionService.AddTypeConverter<List<int>>(new ListIntsConverter());
            interactionService.AddTypeConverter<List<string>>(new ListstringConverter());
            interactionService.AddTypeConverter<ulong>(new UlongConverter());
            interactionService.AddTypeConverter<SocketGuild>(new GuildConverter());
            commands.AddTypeReader(typeof(TimeSpan), new TimeSpanTypeReader());
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
            await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);

            //commands.add
        }

        public CommandHandlerService(IServiceProvider _provider, DiscordSocketClient _discord, CommandService _commands)
        {
            provider = _provider;
            discord = _discord;
            commands = _commands;
            interactionService = new InteractionService(discord, new InteractionServiceConfig
            {
                ThrowOnError = false
            }
            );

            discord.MessageReceived += Discord_MessageReceived;
            discord.Log += Discord_Log;
            discord.Ready += Client_Ready;
            discord.InteractionCreated += Client_InteractionCreated;
            discord.UserJoined += Discord_UserJoined;
            discord.GuildAvailable += Discord_GuildAvailable;
        }

        private async Task Discord_GuildAvailable(SocketGuild arg)
        {
            try
            {
                await interactionService.RegisterCommandsToGuildAsync(arg.Id, false);
            }
            catch (Exception ex)
            { 

            }
        }

        private async Task Discord_UserJoined(SocketGuildUser user)
        {

        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {


            switch (arg)
            {
                case SocketSlashCommand:
                    var SlashContext = new SocketInteractionContext<SocketSlashCommand>(discord, arg as SocketSlashCommand);
                    await interactionService.ExecuteCommandAsync(SlashContext, provider);
                    break;
                case SocketMessageCommand:
                    var MessageContext = new SocketInteractionContext<SocketMessageCommand>(discord, arg as SocketMessageCommand);
                    await interactionService.ExecuteCommandAsync(MessageContext, provider);
                    break;
                case SocketMessageComponent:
                    var _MessageContext = new SocketInteractionContext<SocketMessageComponent>(discord, arg as SocketMessageComponent);
                    await interactionService.ExecuteCommandAsync(_MessageContext, provider);
                    break;

                case SocketUserCommand:
                    var UserContext = new SocketInteractionContext<SocketUserCommand>(discord, arg as SocketUserCommand);
                    await interactionService.ExecuteCommandAsync(UserContext, provider);
                    break;
                case SocketModal:
                    var ModalContext = new SocketInteractionContext<SocketModal>(discord, arg as SocketModal);
                    await interactionService.ExecuteCommandAsync(ModalContext, provider);
                    break;

                default:
                    var context = new SocketInteractionContext(discord, arg);

                    var a = await interactionService.ExecuteCommandAsync(context, provider);

                    break;
            }


        }

        public async Task Client_Ready()
        {
            await discord.SetGameAsync("just a bot", type: ActivityType.Playing);
        }

        private Task Discord_Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        private async Task Discord_MessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage.Author.IsBot)
            {
                return;
            }

            if (socketMessage.Channel is SocketGuildChannel ch && ch.Id == 999962321931743322)
            {
                //jank way to not need to write hard to read code, probs a better way to write
                //perforamnce probs sucks but im just using it for ease and looks
                var msg = socketMessage;
                switch (true)
                {
                    //if any content delete
                    case true when msg.Content != "":
                    //if acctachments delete
                    case true when msg.Attachments.Count > 0:
                    //if wrong stickers delete
                    case true when !msg.Stickers.All(x => x.Id == 0):
                        _ = socketMessage.DeleteAsync();
                        break;
                }
            }

            


            if (socketMessage.Channel is IDMChannel chnl)
            {
            }

            else if (socketMessage is IUserMessage message)
            {
                

                var context = new CommandContext(discord, message);
                var botPrefx = GetSettings().BotPrefix;
                var argPos = 0;

                if (message.HasStringPrefix(botPrefx, ref argPos, StringComparison.CurrentCultureIgnoreCase))
                {
                    var result = await commands.ExecuteAsync(context, argPos, provider);
                    if (result.Error != null)
                    {
                        //DO STUFF HERE
                    }
                }
            }
        }
    }
}
