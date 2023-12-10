using Microsoft.EntityFrameworkCore.Storage;

using NLog;

using ReminderBot.Bot.Extensions;
using ReminderBot.Bot.Persistence;
using ReminderBot.Bot.Persistence.Model;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ReminderBot.Bot.Handler;

public abstract class MessageHandler(ITelegramBotClient client, OperationType type) : Handler<Message>(client)
{
    protected abstract void HandleInternal(Message msg, Logger logger);

    public override async void Handle(Message msg)
    {
        int? msgId = msg.ReplyToMessage?.MessageId;
        if (msgId == null) return;

        Operations? operation = await Operations.GetOperationByMessageIdAsync(msgId.Value);
        if (operation is null) return;

        if (operation.OperationType != type) return;

        Logger logger = this.InitLogger(msg).WithProperties(new Dictionary<string, object>
        {
            { "OperationID", operation.MessageId }
        });

        if (operation.Completed)
        {
            await client.SendTextMessageAsync(msg.Chat.Id, "此操作已经完成", replyToMessageId: msgId);
            logger.Info("操作被拒绝：操作已经完成");
            return;
        }

        if (operation.OperatorUser != (msg.From?.Id ?? 0))
        {
            await client.SendTextMessageAsync(msg.Chat.Id, "这不是你发起的操作", replyToMessageId: msgId);
            logger.Info("操作被拒绝：非本人操作");
            return;
        }

        this.HandleInternal(msg, logger);
    }

    protected sealed override Logger InitLogger(Message message) =>
        LogManager.GetCurrentClassLogger().WithProperties(new Dictionary<string, object>
        {
            {"ChatID", message.Chat.Id},
            {"ChatType", message.Chat.Type},
            {"Username", message.FromUserName()},
            {"UserID", message.FromUserId()},
            {"MessageID", message.MessageId},
            {"MessageContent", message.Text()}
        });
}

public class CreateRemindMessageHandler(ITelegramBotClient client) : MessageHandler(client, OperationType.Create)
{
    protected override async void HandleInternal(Message msg, Logger logger)
    {
        if (msg.Text == null)
        {
            await client.SendTextMessageAsync(msg.Chat.Id, "未能获取到消息的文本", replyToMessageId: msg.MessageId);
            return;
        }

        try
        {
            await using var ctx = new PersistenceContext();
            await using IDbContextTransaction transaction = await ctx.Database.BeginTransactionAsync();
            await RemindItem.CreateAsync(ctx, msg.Text, msg.FromUserId(), msg.Chat.Id);
            await Operations.MarkOperationAsCompletedAsync(ctx, msg.MessageId);
            await transaction.CommitAsync();
            await client.SendTextMessageAsync(msg.Chat.Id, "提醒事项已创建，使用 `/set_period` 设置提醒间隔、使用 `/set_content` 设置提醒内容后才会生效", replyToMessageId: msg.MessageId);
        }
        catch (Exception e)
        {
            logger.Warn(e);
            await client.SendTextMessageAsync(msg.Chat.Id, "出现了一些问题，请稍后再试", replyToMessageId: msg.MessageId);
        }
    }
}
