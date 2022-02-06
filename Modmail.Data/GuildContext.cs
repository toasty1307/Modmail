using Microsoft.EntityFrameworkCore;

namespace Modmail.Data;

public class GuildContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=Modmail;Username=toasty;Password=toasty");
    }
}