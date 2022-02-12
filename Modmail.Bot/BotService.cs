using System.ComponentModel.Design;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modmail.Bot.Extensions;
using Modmail.Bot.Stuff;
using Modmail.Config;
using Modmail.Data;
using Serilog;

namespace Modmail.Bot;

public class BotService : IHostedService
{
    private readonly BotConfig _config;
    private readonly DiscordShardedClient _client;
    private readonly Events _events;
    private readonly ILogger<BotService> _logger;
    private int _index;

    public BotService(BotConfig config)
    {
        _logger = new Logger<BotService>(new LoggerFactory().AddSerilog());
        _logger.LogInformation("Initializing Bot");
        _config = config;
        var discordConfig = new DiscordConfiguration
        {
            Token = config.Token,
            MinimumLogLevel = LogLevel.Trace,
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog(),
        };
        _client = new DiscordShardedClient(discordConfig);
        _events = new Events(new Logger<Events>(new LoggerFactory().AddSerilog()));
        _client.GuildDownloadCompleted += _events.OnGuildDownloadCompleted;
        _client.ChannelDeleted += _events.OnChannelDeleted;
        
        // wonder if this will compile the model and help with the time of the first query
        _ = new GuildContext().Files.FirstOrDefault();
        // it did work

        var e = new GuildContext();
        var time = DateTime.Now - new DateTime(1970, 1, 1);
        _ = e.Guilds.ToList();
        _logger.LogInformation("Time taken to fetch guild from database and not ram: {Time}", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds - time.TotalMilliseconds);
        
        time = DateTime.Now - new DateTime(1970, 1, 1);
        _ = e.Guilds.ToList();
        _logger.LogInformation("Time taken to fetch guild from ram: {Time}", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds - time.TotalMilliseconds);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot starting");
        var slashCommandServices = new ServiceContainer();
        slashCommandServices
            .AddService(_config);
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
            extension.ContextMenuErrored += _events.ContextMenuErrored;
            extension.SlashCommandErrored += _events.SlashCommandErrored;
        }

        await _client.StartAsync();

        foreach (var (_, client) in _client.ShardClients)
        {
            client.AddExtension(new ModmailExtension(new Logger<ModmailExtension>(new LoggerFactory().AddSerilog())));
        }

        _client.GuildDownloadCompleted += async (_, _) => await CycleStatusAsync();
        
        _logger.LogInformation("Bot started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping bot");
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
        // this is how i remake the timer thing but async
        Task.Delay(_config.StatusChangeInterval * 1000).ContinueWith(async _ => await CycleStatusAsync());
#pragma warning restore CS4014
    }
}