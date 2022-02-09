using System.ComponentModel.DataAnnotations;

namespace Modmail.Data.Entities;

public class ConfigEntity
{
    [Key]
    public ulong GuildId { get; set; }
    public GuildEntity GuildEntity { get; set; } = null!;
    public ulong LogChannelId { get; set; }
    public ulong CategoryChannelId { get; set; }
    public ulong LogAccessRoleId { get; set; }
    public bool MoveChannelsToCategory { get; set; } = true;
}