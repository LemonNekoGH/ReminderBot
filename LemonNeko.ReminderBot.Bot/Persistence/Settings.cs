using LemonNeko.ReminderBot.Bot.Persistence.Model;

using Microsoft.EntityFrameworkCore;

namespace LemonNeko.ReminderBot.Bot.Persistence;

public partial class PersistenceContext
{
    public async Task<bool> IsAllowAllUsersAsync(long chatId)
    {
        Settings? settings = await this.Settings!.FirstOrDefaultAsync(x => x.ChatId == chatId);
        return settings?.AllowAllUsers ?? false;
    }
}
