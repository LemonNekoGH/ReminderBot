using DotNetEnv;

using Microsoft.EntityFrameworkCore;

using LemonNeko.ReminderBot.Bot.Persistence.Model;

namespace LemonNeko.ReminderBot.Bot.Persistence;

public partial class PersistenceContext : DbContext
{
    // only for test
    public static string ConnectionString { get; set; } = "";

    public DbSet<Settings>? Settings { get; set; }
    public DbSet<RemindItem>? RemindItems { get; set; }
    public DbSet<Operations>? Operations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string dataSource = ConnectionString != "" ? ConnectionString : Config.DbSourceString;
        options.UseNpgsql(dataSource);
    }
}
