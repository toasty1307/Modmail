using System.ComponentModel.Design;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modmail.Bot.BotExtensions;
using Modmail.Config;
using Modmail.Data;
using Serilog;

namespace Modmail.Bot;

public class BotService : IHostedService
{
    private readonly BotConfig _config;
    private readonly GuildContext _guildContext;
    private readonly DiscordShardedClient _client;
    private readonly EventsExtension _eventsExtension;

    public BotService(BotConfig config, GuildContext guildContext)
    {
        _config = config;
        _guildContext = guildContext;
        var discordConfig = new DiscordConfiguration
        {
            Token = _config.Token,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog(),
        };
        _client = new DiscordShardedClient(discordConfig);
        _eventsExtension = new EventsExtension();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.StartAsync();
        var slashCommandServices = new ServiceContainer();
        slashCommandServices.AddService(typeof(GuildContext), _guildContext);
        var slashCommandConfig = new SlashCommandsConfiguration
        {
            Services = slashCommandServices
        };
        var slashCommandsExtensions = await _client.UseSlashCommandsAsync(slashCommandConfig);
        foreach (var (_, extension) in slashCommandsExtensions)
        {
            extension.RegisterCommands(GetType().Assembly
#if DEBUG
            , 841890589640359946
#endif
            );
            extension.ContextMenuErrored += _eventsExtension.ContextMenuErrored;
            extension.SlashCommandErrored += _eventsExtension.SlashCommandErrored;
        }

        foreach (var (_, client) in _client.ShardClients)
        {
            client.AddExtension(_eventsExtension);
            client.AddExtension(new StatusExtension(_config.StatusChangeInterval, _config.Statuses));
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }
}