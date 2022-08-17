#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using BasicBot.Handler;
using BasicBot.MonarkTypes;
using BasicBot.Settings;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static BasicBot.Handler.User;

#endregion

namespace BasicBot.Commands;

public class SlashCommand : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    StaffUser staff;

    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        await DeferAsync(true);
        if (Context.User is SocketGuildUser user)
        {
            staff = user;
        }
    }

    [SlashCommand("remove", "remove a user from the leaderbaord")]
    public async Task remove(SocketUser user)
    {
        var reply = RemoveUser(staff, user, Context.Guild);
        await reply.SendMessage(Context.Interaction);
    }

    public MonarkTypes.Message.MonarkMessage RemoveUser(StaffUser staff, SocketUser user, SocketGuild guild)
    {
        if (!staff.IsAdmin())
            return "admin only";

        var userAccounts = Handler.Settings.GetSettings().UserAccounts;

        Handler.Settings.SaveSettings();

        if (!userAccounts.Remove(user.Id))
        {
            return "user is not registerd";
        }

        Handler.Settings.SaveSettings();

        return "Done, please wait for the next leaderboard update";
    }

    [SlashCommand("add", "adds a user to the leaderbaord")]
    public async Task add(SocketUser user, string wbName)
    {
        var reply = await AddUser(staff, user, wbName, Context.Guild);
        await reply.SendMessage(Context.Interaction);
    }

    [SlashCommand("force", "force ads a user via id")]
    public async Task force(SocketUser user, string wbId)
    {
        var reply = await ForceUser(staff, user, wbId, Context.Guild);
        await reply.SendMessage(Context.Interaction);
    }

    public async Task<MonarkTypes.Message.MonarkMessage> AddUser(StaffUser staff, SocketUser user, string wbName, SocketGuild gld)
    {
        if (!staff.IsAdmin())
            return "admin only";

        return await BasicBot.Commands.ModalCommand.AddUserToLeaderBoard(gld, user.Id, wbName);
    }

    public async Task<MonarkTypes.Message.MonarkMessage> ForceUser(StaffUser staff, SocketUser user, string wbId, SocketGuild gld)
    {
        if (!staff.IsAdmin())
            return "admin only";

        return await BasicBot.Commands.ModalCommand.AddUserIdToLeaderBoard(gld, user.Id, wbId);
    }
}