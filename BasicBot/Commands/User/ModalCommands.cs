using System.Threading.Tasks;
using BasicBot.Handler;
using BasicBot.MonarkTypes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

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

        public static MultiversusApiHandler ApiHandler = new();

        // Responds to the modal.
        [ModalInteraction("register")]
        public async Task MsgModalModal(RegisterModal modal)
        {
            await Context.Interaction.DeferAsync(true);
            var reply = await AddUserToLeaderBoard(Context.Guild, Context.User.Id, modal.Text);
            await reply.SendMessage(Context.Interaction);
        }

        public static async Task<Message.MonarkMessage> AddUserToLeaderBoard(SocketGuild guild, ulong userId,
            string wbName)
        {
            if (await ApiHandler.GetMMRByName(wbName) is not MultiversusApiHandler.PlayerMMR acc)
            {
                return $"Failed to find an account called {wbName}";
            }

            var userAccounts = Handler.Settings.GetSettings().UserAccounts;

            if (userAccounts.ContainsValue(acc.Id))
            {
                return "WB Account is already registerd";
            }

            userAccounts[userId] = acc.Id;
            Handler.Settings.SaveSettings();

            return "Done, please wait for the next leaderboard update";
        }

        public static async Task<Message.MonarkMessage> AddUserIdToLeaderBoard(SocketGuild guild, ulong userId,
            string wbId)
        {
            if (await ApiHandler.GetMMRByName(wbId) is not MultiversusApiHandler.PlayerMMR acc)
            {
                return $"Failed to find an account with Id {wbId}";
            }

            var userAccounts = Handler.Settings.GetSettings().UserAccounts;

            if (userAccounts.ContainsValue(acc.Id))
            {
                return "WB Account is allready registerd";
            }

            userAccounts[userId] = acc.Id;
            Handler.Settings.SaveSettings();

            return "Done, please wait for the next leaderboard update";
        }
    }
}