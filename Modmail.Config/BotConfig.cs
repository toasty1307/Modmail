using DSharpPlus.Entities;

namespace Modmail.Config;

public class BotConfig
{
    public string Token { get; init; } = null!;
    public Status[] Statuses { get; init; } = null!;
    public int StatusChangeInterval { get; init; } = 10;
}

public class Status
{
    public ActivityType ActivityType { get; init; } = 0;
    public string Message { get; init; } = null!;
    public UserStatus? StatusType { get; init; } = 0; // i ran out of names
    public string Url { get; init; } = null!;
}