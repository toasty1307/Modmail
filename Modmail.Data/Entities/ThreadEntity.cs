using System.ComponentModel.DataAnnotations;

namespace Modmail.Data.Entities;

public class ThreadEntity
{
    [Key]
    public ulong Id { get; init; }
    public ulong RecipientId { get; init; }
    public bool Closed { get; set; }
    public ulong GuildId { get; init; }
    public DateTime Created { get; init; }
    public GuildEntity GuildEntity { get; init; } = null!;
    public List<MessageEntity> Messages { get; init; } = new();
    public FileEntity RecipientAvatar { get; init; } = null!;
}