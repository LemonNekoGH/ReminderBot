using NLog;

using ReminderBot.Bot.Extensions;
using ReminderBot.Bot.Persistence.Model;
using ReminderBot.Bot.Types;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ReminderBot.Bot.Handler;

public abstract class CommandHandler(ITelegramBotClient client, string commandName, CommandHandler.Options options)
    : Handler<Command>(client)
{
    protected sealed class Options
    {
        public bool CheckAllowAllUsers { get; init; }
    }

    protected abstract void HandleInternal(Command cmd, Logger logger);

    public sealed override async void Handle(Command cmd)
    {
        if (cmd.Name != commandName)
            return;

        Logger logger = this.InitLogger(cmd);
        logger.Info("Command received");

        if (options.CheckAllowAllUsers)
        {
            bool isFromGroup = cmd.Message.Chat.IsGroup();
            bool isAllowAllUsers = false;
            bool isFromAdmin = false;
            if (isFromGroup) // there are no administrators in the private chat
            {
                isAllowAllUsers = await Settings.IsAllowAllUsersAsync(cmd.Message.Chat.Id);
                isFromAdmin = await cmd.Message.IsFromAdminAsync(client);
            }

            if (isFromGroup && !isAllowAllUsers && !isFromAdmin)
            {
                await client.SendTextMessageAsync(cmd.Message.Chat.Id, "此命令仅限管理员可用，管理员可使用 `/settings` 修改设置");
                logger.Info("命令调用被拒绝");
                return;
            }
        }
        this.HandleInternal(cmd, logger);
    }

    protected sealed override Logger InitLogger(Command cmd) =>
        LogManager.GetCurrentClassLogger().WithProperties(new Dictionary<string, object>
        {
            {"ChatID", cmd.Message.Chat.Id},
            {"ChatType", cmd.Message.Chat.Type},
            {"Username", cmd.Message.FromUserName()},
            {"UserID", cmd.Message.FromUserId()},
            {"MessageID", cmd.Message.MessageId},
            {"MessageContent", cmd.Message.Text()}
        });
}

public sealed class StartCommandHandler(ITelegramBotClient client) : CommandHandler(
    client,
    "start",
    new Options
    {
        CheckAllowAllUsers = true
    })
{
    protected override async void HandleInternal(Command cmd, Logger logger)
    {
        string msgPrefix = cmd.Message.Chat.IsGroup() ? "你正在创建一条会发送给所有群组成员的提醒事项" : "你正在为自己创建提醒事项";
        Message message = await client.SendTextMessageAsync(cmd.Message.Chat.Id, $"{msgPrefix}，请将提醒事项名称回复至此消息");
        try
        {
            await Operations.CreateOperationAsync(message.MessageId, cmd.Message.FromUserId(), OperationType.Create);
        }
        catch (Exception e)
        {
            logger.Warn(e);
            await client.EditMessageTextAsync(cmd.Message.Chat.Id, message.MessageId, "出现了一些错误，请稍后再试");
        }
    }
}
