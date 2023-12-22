using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace LemonNeko.ReminderBot.Bot.Persistence.Model;

public enum OperationType
{
    Unknown,
    Create,
    SetName,
    SetContent,
    SetPeriod,
}

[PrimaryKey("MessageId")]
[Table("operations", Schema = "reminder_bot")]
public record Operations
{
    [Column("id")]
    public int MessageId { get; set; }

    [Column("operator_user")]
    public long OperatorUser { get; set; }

    [Column("completed")]
    public bool Completed { get; set; }

    [Column("remind_item_id", TypeName = "text")]
    public string? RemindItemId { get; set; } = null;

    [Column("type")]
    public OperationType OperationType { get; set; }
}
