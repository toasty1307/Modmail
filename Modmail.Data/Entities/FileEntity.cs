using System.ComponentModel.DataAnnotations;

namespace Modmail.Data.Entities;

public class FileEntity
{
    [Key]
    public string Url { get; init; } = null!;
    public byte[] Data { get; init; } = null!;
}