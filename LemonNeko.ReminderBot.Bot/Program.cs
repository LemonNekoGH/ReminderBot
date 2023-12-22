using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Config;
using NLog.Layouts;

using LemonNeko.ReminderBot.Bot.Handler;
using LemonNeko.ReminderBot.Bot.Persistence;

using Telegram.Bot;

namespace LemonNeko.ReminderBot.Bot;

internal static class Program
{
    private static readonly CancellationTokenSource cts = new();
    private static string version = "0.1.0-alpha.1";

    private static string GetOs()
    {
        if (OperatingSystem.IsMacOS())
            return "Darwin";

        if (OperatingSystem.IsWindows())
            return "Windows";
        return OperatingSystem.IsLinux() ? "Linux" : "Unknown";
    }

    private static void InitialLogger(ISetupLoadConfigurationBuilder builder)
    {
        LogLevel minLevel = LogLevel.Info;
        if (version.Contains("alpha")
        || version.Contains("beta")
        || version.Contains("rc")
        || version.StartsWith("0."))
            minLevel = LogLevel.Debug;

        builder.ForLogger()
            .FilterMinLevel(minLevel)
            .WriteToConsole(layout: Layout.FromString("${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}|${all-event-properties:includeEmptyValues=true:includeScopeProperties=true}"));
    }

    private static void Main()
    {
        AssemblyInformationalVersionAttribute? versionAttr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        version = versionAttr?.InformationalVersion ?? "0.1.0-alpha.1";

        Console.WriteLine("=======================");
        Console.WriteLine("Reminder Bot");
        Console.WriteLine("");
        Console.WriteLine($"Version:          {version}");
        Console.WriteLine($"Operating System: {GetOs()}");
        Console.WriteLine($"Architecture:     {RuntimeInformation.OSArchitecture}");
        Console.WriteLine($"ProcessorCount:   {Environment.ProcessorCount}");
        Console.WriteLine("=======================");

        Config.LoadConfig();

        LogManager.Setup().LoadConfiguration(InitialLogger);

        var dbContext = new PersistenceContext();
        dbContext.Database.MigrateAsync().Wait();
        dbContext.Dispose();

        var botClient = new TelegramBotClient(Config.BotToken);

        var bot = new Bot(botClient);

        var startCommandHandler = new StartCommandHandler(botClient);

        bot.CommandReceived += startCommandHandler.Handle;

        var createRemindMessageHandler = new CreateRemindMessageHandler(botClient);

        bot.CommonMessageReceived += createRemindMessageHandler.Handle;

        bot.Start(cts.Token);

        // suspend this program
        var waitForStop = new TaskCompletionSource<bool>();
        Console.CancelKeyPress += (_, args) =>
        {
            cts.Cancel();
            args.Cancel = true;

            bot.CommandReceived -= startCommandHandler.Handle;

            waitForStop.TrySetResult(true);
        };
        waitForStop.Task.Wait();
    }
}
