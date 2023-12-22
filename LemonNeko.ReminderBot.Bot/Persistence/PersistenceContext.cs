using Microsoft.EntityFrameworkCore;

using LemonNeko.ReminderBot.Bot.Persistence.Model;

namespace LemonNeko.ReminderBot.Bot.Persistence;

public class PersistenceContext : DbContext
{
    public DbSet<Settings>? Settings { get; set; }
    public DbSet<RemindItem>? RemindItems { get; set; }
    public DbSet<Operations>? Operations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseNpgsql(Config.DbSourceString);
}
