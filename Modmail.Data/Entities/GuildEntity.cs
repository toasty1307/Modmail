using System.ComponentModel.DataAnnotations;

namespace Modmail.Data.Entities;

public class GuildEntity
{
    [Key]
    public ulong Id { get; init; }
    public string Name { get; init; } = null!;
    public bool Setup { get; set; }
    public ConfigEntity Config { get; init; } = null!;
    public List<ThreadEntity> Threads { get; set; } = new();
    public FileEntity Icon { get; set; } = null!;
}