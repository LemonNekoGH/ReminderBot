using LemonNeko.ReminderBot.Bot.Persistence.Model;

using Microsoft.EntityFrameworkCore;

namespace LemonNeko.ReminderBot.Bot.Persistence;

public partial class PersistenceContext
{
    public async Task<Operations?> GetOperationByMessageIdAsync(int messageId) =>
        await this.Operations!.Where(it => it.MessageId == messageId).FirstOrDefaultAsync();

    public async Task CreateOperationAsync(int messageId, long userId, OperationType type, string? remindItemId = null)
    {
        await this.Operations!.AddAsync(new Operations
        {
            MessageId = messageId,
            OperatorUser = userId,
            OperationType = type,
            RemindItemId = remindItemId
        });
        await this.SaveChangesAsync();
    }

    public async Task MarkOperationAsCompletedAsync(int messageId)
    {
        Operations? operation = await this.Operations!.Where(it => it.MessageId == messageId).FirstOrDefaultAsync();
        operation!.Completed = true;
        await this.SaveChangesAsync();
    }
}
