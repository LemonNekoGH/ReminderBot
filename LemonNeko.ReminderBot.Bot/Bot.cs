using NLog;

using LemonNeko.ReminderBot.Bot.Extensions;
using LemonNeko.ReminderBot.Bot.Types;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using NotSupportedException = System.NotSupportedException;

namespace LemonNeko.ReminderBot.Bot;

public class Bot(ITelegramBotClient client)
{
    public delegate void CommandReceivedHandler(Command ctx);

    public delegate void CommonMessageReceivedHandler(Message ctx);

    public delegate void CallbackQueryReceivedHandler(CallbackQuery ctx);

    public event CommandReceivedHandler? CommandReceived;
    public event CommonMessageReceivedHandler? CommonMessageReceived;
    public event CallbackQueryReceivedHandler? CallbackQueryReceived;

    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    public void Start(CancellationToken cts)
    {
        client.Timeout = TimeSpan.FromSeconds(100);
        client.TestApiAsync(cts).Wait(cts);

        client.StartReceiving(this.HandleUpdate, this.HandlePollingError, null, cts);

        this.logger.Info("Bot started");
    }

    private void HandlePollingError(ITelegramBotClient _, Exception e, CancellationToken token) =>
        this.logger.Fatal($"Unexpected error: {e.Message}, type: {e.GetType()}");

    private void HandleUpdate(ITelegramBotClient _, Update update, CancellationToken token)
    {
        switch (update.Type)
        {
            case UpdateType.Message when update.IsCommand():
                {
                    this.CommandReceived?.Invoke(new Command(update.Message!, update.Command()));
                    return;
                }
            case UpdateType.Message:
                {
                    this.CommonMessageReceived?.Invoke(update.Message!);
                    return;
                }
            case UpdateType.CallbackQuery:
                {
                    this.CallbackQueryReceived?.Invoke(new CallbackQuery());

                    return;
                }
            default:
                throw new NotSupportedException($"{update.Type} is not support");
        }
    }
}
