using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using Discord.WebSocket;
using static BasicBot.Handler.User;
using Discord.Interactions;
using SummaryAttribute = Discord.Interactions.SummaryAttribute;
using static BasicBot.Commands.ModalCommand;
using BasicBot.Settings;
using BasicBot.Handler;

namespace BasicBot.Commands
{
    public class ModalCommand : InteractionModuleBase<SocketInteractionContext<SocketModal>>
    {
        // Defines the modal that will be sent.
        public class RegisterModal : IModal
        {
            public string Title => "Register Modal";
            // Strings with the ModalTextInput attribute will automatically become components.         

            [RequiredInput(true)]
            [InputLabel("WB Name")]
            [ModalTextInput("wbname", TextInputStyle.Short)]
            public string Text { get; set; }
        }

        // Responds to the modal.
        [ModalInteraction("register")]
        public async Task MsgModalModal(RegisterModal modal)
        {
            await Context.Interaction.DeferAsync(true);
            var reply = await AddUserToLeaderBoard(Context.Guild, Context.User.Id, modal.Text);
            await reply.SendMessage(Context.Interaction);

        }

        public static async Task<MonarkTypes.Message.MonarkMessage> AddUserToLeaderBoard(SocketGuild guild, ulong userId, string wbName)
        {
            if (await HenrikApi.GetAccountByName(wbName) is not HenrikApi.Account acc)
            {
                return $"Failed to find an account called {wbName}";
            }

            var userAccounts = Handler.Settings.GetSettings().UserAccounts;

            if (userAccounts.ContainsValue(acc.Info.Id))
            {
                return "WB Account is allready registerd";
            }

            userAccounts[userId] = acc.Info.Id;
            Handler.Settings.SaveSettings();

            return "Done, please wait for the next leaderboard update";
        }

        public static async Task<MonarkTypes.Message.MonarkMessage> AddUserIdToLeaderBoard(SocketGuild guild, ulong userId, string wbId)
        {
            if (await HenrikApi.GetAccountById(wbId) is not HenrikApi.Account acc)
            {
                return $"Failed to find an account with Id {wbId}";
            }

            var userAccounts = Handler.Settings.GetSettings().UserAccounts;

            if (userAccounts.ContainsValue(acc.Info.Id))
            {
                return "WB Account is allready registerd";
            }

            userAccounts[userId] = acc.Info.Id;
            Handler.Settings.SaveSettings();

            return "Done, please wait for the next leaderboard update";
        }
    }
}
