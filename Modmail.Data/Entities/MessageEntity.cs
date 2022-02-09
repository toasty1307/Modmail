using System.ComponentModel.DataAnnotations;

namespace Modmail.Data.Entities;

public enum ThreadMessageType
{
    Internal,
    Starting,
    Closing,
    Reply,
    Recipient
}

// TODO Add Embeds
public class MessageEntity
{
    [Key]
    public ulong MessageId { get; init; }
    public ThreadMessageType Type { get; init; }
    public ulong AuthorId { get; init; }
    public bool Anonymous { get; init; }
    public string AuthorUsername { get; init; } = null!;
    public string AuthorDiscriminator { get; init; } = null!;
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public ulong ThreadId { get; init; }
    public ThreadEntity ThreadEntity { get; init; } = null!;
    public List<FileEntity> Attachments { get; init; } = new();
    public FileEntity AuthorAvatar { get; init; } = null!;
}