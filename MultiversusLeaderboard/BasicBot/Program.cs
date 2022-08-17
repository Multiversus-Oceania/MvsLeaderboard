﻿#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BasicBot.Handler;
using BasicBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using static BasicBot.Handler.Settings;

#endregion

namespace BasicBot;

internal class Program
{
    private static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public static DiscordSocketClient discordClient;
    public SocketSelfUser selfUser => discordClient != null ? discordClient.CurrentUser : null;

    public async Task MainAsync()
    {
        discordClient = new DiscordSocketClient(new DiscordSocketConfig
        { LogLevel = LogSeverity.Verbose, GatewayIntents = GatewayIntents.All, AlwaysDownloadUsers = true });


        var token = GetSettings().BotToken;
        Handler.Settings.SaveSettings();

        await discordClient.LoginAsync(TokenType.Bot, token);
        await discordClient.StartAsync();

        var services = ConfigureServices();

        await services.GetRequiredService<CommandHandlerService>().InitializeAsync();
        BasicBot.Services.RankLeaderBoard.Intilize(discordClient);

        await Task.Delay(-1);
    }

    private IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(discordClient)
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>()
            .BuildServiceProvider();
    }
}