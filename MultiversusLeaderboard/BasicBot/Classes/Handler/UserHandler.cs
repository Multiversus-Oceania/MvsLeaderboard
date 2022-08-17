using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BasicBot.Handler.Settings;

namespace BasicBot.Handler
{
    public static class User
    {
        public interface IStaffType
        {
            public StaffType StaffType();
        }
        public enum StaffType
        {
            NoPerms = 0,
            Support,
            Managment,
            Admin,
            ServerOwner,
            BotOwner
        }

        public static bool IsTypeOrHigher(this StaffType type, StaffType checking) =>
            (int)type >= (int)checking;

        public static bool IsBotOwner(this IStaffType staff) =>
            IsBotOwner(staff.StaffType());
        public static bool IsBotOwner(this StaffType type) =>
            IsTypeOrHigher(type, StaffType.BotOwner);
        public static bool IsServerOwner(this IStaffType staff) =>
            IsServerOwner(staff.StaffType());
        public static bool IsServerOwner(this StaffType type) =>
            IsTypeOrHigher(type, StaffType.ServerOwner);
        public static bool IsAdmin(this IStaffType staff) =>
            IsAdmin(staff.StaffType());
        public static bool IsAdmin(this StaffType type) =>
            IsTypeOrHigher(type, StaffType.Admin);
        public static bool IsManagment(this IStaffType staff) =>
            IsManagment(staff.StaffType());
        public static bool IsManagment(this StaffType type) =>
            IsTypeOrHigher(type, StaffType.Managment);
        public static bool IsSupport(this IStaffType staff) =>
            IsSupport(staff.StaffType());
        public static bool IsSupport(this StaffType type) =>
            IsTypeOrHigher(type, StaffType.Support);



        public class StaffUser : IStaffType
        {
            public IGuildUser User;
            public StaffType Type = StaffType.NoPerms;
            public ulong id {
                get
                {
                    if (User == null)
                        return 0;

                    return User.Id;
                }
            }
            public IGuild Guild => User.Guild;

            private void SetType()
            {
                if (Handler.Settings.IsBotOwner(User.Id))
                {
                    Type = StaffType.BotOwner;
                }
                else if (User.Hierarchy == int.MaxValue)
                {
                    Type = StaffType.ServerOwner;
                }
                else if (User.GuildPermissions.Administrator)
                {
                    Type = StaffType.Admin;
                }
            }

            StaffType IStaffType.StaffType() => Type;

            public static implicit operator StaffUser(SocketGuildUser user)
            {
                return new StaffUser(user);
            }
            public StaffUser() { }
            public StaffUser(IGuildUser user)
            {
                this.User = user;
                SetType();
            }


        }

        public static async Task AddRole(this SocketGuildUser user, SocketRole role) => await AddRole(user, role.Id);
        public static async Task AddRole(this SocketGuildUser user, ulong roleId)
        {
            if (!user.Roles.Any(x => x.Id == roleId))
            {
                try
                {
                    await user.AddRoleAsync(roleId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static async Task RemoveRole(this SocketGuildUser user, SocketRole role) => await RemoveRole(user, role.Id);
        public static async Task RemoveRole(this SocketGuildUser user, ulong roleId)
        {
            if (user.Roles.Any(x => x.Id == roleId))
            {
                try
                {
                    await user.RemoveRoleAsync(roleId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public enum ToggleRoleState
        {
            error,
            remove,
            add
        }

        public static async Task<ToggleRoleState> ToggleRole(this SocketGuildUser user, SocketRole role) => await ToggleRole(user, role.Id);
        public static async Task<ToggleRoleState> ToggleRole(this SocketGuildUser user, ulong roleId)
        {
            if (user.Roles.Any(x => x.Id == roleId))
            {
                await user.RemoveRole(roleId);
                return ToggleRoleState.remove;
            }
            else
            {
                await user.AddRole(roleId);
                return ToggleRoleState.add;
            }
        }

        public static async Task TryRemoveRoles(this SocketGuildUser user, IEnumerable<SocketRole> roles) =>
           await TryRemoveRolesDelay(user, roles, TimeSpan.Zero);
        public static async Task TryRemoveRolesDelay(this SocketGuildUser user, IEnumerable<SocketRole> roles, TimeSpan time)
        {
            foreach (var r in roles)
            {
                await user.TryRemoveRole(r);

                if (time > TimeSpan.Zero)
                    await Task.Delay(time);
            }
        }


        public static async Task<bool> TryRemoveRole(this SocketGuildUser user, IRole role) =>
            await user.TryRemoveRole(role.Id);
        public static async Task<bool> TryRemoveRole(this SocketGuildUser user, ulong roleid)
        {
            if (user.Roles.Any(x => x.Id == roleid))
            {
                try
                {
                    await user.RemoveRoleAsync(roleid);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return false;
        }

        public static async Task<bool> TryAddRole(this SocketGuildUser user, IRole role) =>
            await user.TryAddRole(role.Id);


        public static async Task<bool> TryAddRole(this SocketGuildUser user, ulong roleid)
        {
            if (!user.Roles.Any(x => x.Id == roleid))
            {
                try
                {
                    await user.AddRoleAsync(roleid);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return false;
        }

        public static async Task SendMsg(this IDiscordInteraction interaction, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            if (!interaction.HasResponded)
            {
                await interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
            }
            else
            {
                await interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
            }
        }

        public static async Task SendMsgFiles(this IDiscordInteraction interaction, IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            if (!interaction.HasResponded)
            {
                await interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
            }
            else
            {
                await interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
            }
        }
    }
}
