using Microsoft.EntityFrameworkCore;
using Modmail.Data.Entities;

namespace Modmail.Data;

public class GuildContext : DbContext
{
    public DbSet<GuildEntity> Guilds { get; set; } = null!;
    public DbSet<ThreadEntity> Threads { get; set; } = null!;
    public DbSet<MessageEntity> Messages { get; set; } = null!;
    public DbSet<FileEntity> Files { get; set; } = null!;
    public DbSet<ConfigEntity> Configs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildEntity>()
            .HasMany(x => x.Threads)
            .WithOne(x => x.GuildEntity)
            .HasForeignKey(x => x.GuildId);
        modelBuilder.Entity<ThreadEntity>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.ThreadEntity)
            .HasForeignKey(x => x.ThreadId);
        modelBuilder.Entity<GuildEntity>()
            .HasOne(x => x.Config)
            .WithOne(x => x.GuildEntity)
            .HasForeignKey<ConfigEntity>(x => x.GuildId);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=Modmail;Username=toasty;Password=toasty;Include Error Detail=true");
    }
}