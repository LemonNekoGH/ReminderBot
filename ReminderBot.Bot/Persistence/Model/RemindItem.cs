using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ReminderBot.Bot.Persistence.Model;

[PrimaryKey("Id")]
[Table("remind_items", Schema = "reminder_bot")]
public record class RemindItem
{
    [Column("id")]
    public string Id = new Guid().ToString();

    [Column("owner")]
    public long Owner { get; set; }

    [Column("period")]
    public string? Period { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("content")]
    public string? Content { get; set; }

    [Column("chat_id")]
    public long ChatId { get; set; }

    // TODO: need test
    public static async Task CreateAsync(PersistenceContext ctx, string remindName, long owner, long chatId)
    {
        await ctx.RemindItems.AddAsync(new RemindItem
        {
            Owner = owner,
            Name = remindName,
            ChatId = chatId
        });
        await ctx.SaveChangesAsync();
    }
}
