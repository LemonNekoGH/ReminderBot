using LemonNeko.ReminderBot.Bot.Persistence;
using LemonNeko.ReminderBot.Bot.Persistence.Model;

using Microsoft.EntityFrameworkCore;

namespace LemonNeko.ReminderBot.Test;

public class OperationsTest
{
    [Fact]
    public async void TestAsync()
    {
        PersistenceContext.ConnectionString = Environment.GetEnvironmentVariable("REMINDER_BOT_DATABASE_SOURCE") ?? "";

        int messageId = Random.Shared.Next();
        long operatorUser = Random.Shared.NextInt64();
        const OperationType operationType = OperationType.Create;

        await using var ctx = new PersistenceContext();
        await ctx.Database.MigrateAsync();

        // test create
        await ctx.CreateOperationAsync(messageId, operatorUser, operationType);

        // test get
        Operations? op = await ctx.GetOperationByMessageIdAsync(messageId);
        op.ShouldNotBeNull();
        op.MessageId.ShouldBe(messageId);
        op.OperatorUser.ShouldBe(operatorUser);
        op.OperationType.ShouldBe(operationType);
        op.Completed.ShouldBeFalse();
        op.RemindItemId.ShouldBeNull();

        // test mark as completed
        op.Completed = true;
        await ctx.SaveChangesAsync();

        // test get
        Operations? op2 = await ctx.GetOperationByMessageIdAsync(messageId);
        op2.ShouldNotBeNull();
        op2.Completed.ShouldBeTrue();

        // clear data
        ctx.Remove(op);
        await ctx.SaveChangesAsync();
    }
}
