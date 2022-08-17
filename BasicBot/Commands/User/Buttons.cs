using System.Linq;
using System.Threading.Tasks;
using BasicBot.Handler;
using Discord.Interactions;
using Discord.WebSocket;
using static BasicBot.Commands.ModalCommand;
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Commands
{
    public class ButtonCommand : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        [ComponentInteraction("register")]
        public async Task Register()
        {
            await Context.Interaction.RespondWithModalAsync<RegisterModal>("register");
        }

        [ComponentInteraction("solo")]
        public async Task GetRankSolo()
        {
            await Context.Interaction.DeferAsync(true);
            var reply = BasicBot.Services.RankLeaderBoard.GetSoloLeaderboardPlacement(Context.User.Id);
            await reply.SendMessage(Context.Interaction);
        }

        [ComponentInteraction("duo")]
        public async Task GetRankDuo()
        {
            await Context.Interaction.DeferAsync(true);
            var reply = BasicBot.Services.RankLeaderBoard.GetDuoLeaderboardPlacement(Context.User.Id);
            await reply.SendMessage(Context.Interaction);
        }

    }
}