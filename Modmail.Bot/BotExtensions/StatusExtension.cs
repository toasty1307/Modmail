using DSharpPlus;
using DSharpPlus.Entities;
using Modmail.Config;

namespace Modmail.Bot.BotExtensions;

public class StatusExtension : BaseExtension
{
    private readonly int _interval;
    private readonly Status[] _statuses;
    private Timer _timer = null!;
    private int _index = 0;
    
    public StatusExtension(int interval, Status[] statuses)
    {
        _interval = interval;
        _statuses = statuses;
    }
    
    protected override void Setup(DiscordClient client)
    {
        Client = client;
        _timer = new Timer(ChangeStatusAsync, null, 0, _interval);
    }

    public void ChangeStatusAsync(object? state)
    {
        var status = _statuses[_index];
        _index++;
        _index %= _statuses.Length;
        Client.UpdateStatusAsync(new DiscordActivity(status.Message, status.ActivityType)
        {
            StreamUrl = status.Url
        }, status.StatusType).Wait();
    }
}