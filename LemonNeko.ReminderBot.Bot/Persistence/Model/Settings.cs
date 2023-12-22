using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace LemonNeko.ReminderBot.Bot.Persistence.Model;

[PrimaryKey("ChatId")]
[Table("settings", Schema = "reminder_bot")]
public record Settings
{
    [Column("chat_id")]
    public long ChatId { get; set; }

    [Column("allow_all_users")]
    public bool AllowAllUsers { get; set; }
}
