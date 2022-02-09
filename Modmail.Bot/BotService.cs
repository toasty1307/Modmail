using System.ComponentModel.Design;
using DSharpPlus;
using DSharpPlus.Entities;
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
    private int _index;

    public BotService(BotConfig config, GuildContext guildContext)
    {
        _config = config;
        _guildContext = guildContext;
        var discordConfig = new DiscordConfiguration
        {
            Token = config.Token,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog(),
        };
        _client = new DiscordShardedClient(discordConfig);
        _eventsExtension = new EventsExtension();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var slashCommandServices = new ServiceContainer();
        slashCommandServices.AddService(typeof(GuildContext), _guildContext);
        slashCommandServices.AddService(typeof(BotConfig), _config);
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

        await _client.StartAsync();

        foreach (var (_, client) in _client.ShardClients)
        {
            client.AddExtension(_eventsExtension);
            client.AddExtension(new ModmailExtension());
        }

        _client.GuildDownloadCompleted += async (_, _) => await CycleStatusAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    public async Task CycleStatusAsync()
    {
        var status = _config.Statuses[_index];
        _index++;
        _index %= _config.Statuses.Length;
        await _client.UpdateStatusAsync(new DiscordActivity(status.Message, status.ActivityType)
        {
            StreamUrl = status.Url
        }, status.StatusType);
#pragma warning disable CS4014
        Task.Delay(_config.StatusChangeInterval * 1000).ContinueWith(async _ => await CycleStatusAsync());  // this is how i remake the timer thing but async
#pragma warning restore CS4014
    }
}