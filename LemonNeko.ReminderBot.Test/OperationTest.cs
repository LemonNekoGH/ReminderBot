using LemonNeko.ReminderBot.Bot.Persistence;
using LemonNeko.ReminderBot.Bot.Persistence.Model;

namespace LemonNeko.ReminderBot.Test;

public class OperationsTest
{
    [Fact]
    public async void TestGetOperationByMessageIdAsync()
    {
        int messageId = Random.Shared.Next();

        await using var ctx = new PersistenceContext();
        await ctx.AddAsync(new Operations
        {

        });
    }
}
