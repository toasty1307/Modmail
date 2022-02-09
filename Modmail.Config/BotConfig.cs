using System.Configuration;
using DSharpPlus.Entities;

namespace Modmail.Config;


public class BotConfig
{
    public string Token { get; init; } = null!;
    public Status[] Statuses { get; init; } = null!;
    public int StatusChangeInterval { get; init; } = 10;
    public string InviteLink { get; init; } = null!;
}

public class Status
{
    public ActivityType ActivityType { get; init; } = ActivityType.Watching;
    public string Message { get; init; } = null!;
    public UserStatus StatusType { get; init; } = UserStatus.Online; // i ran out of names
    public string Url { get; init; } = null!;
}