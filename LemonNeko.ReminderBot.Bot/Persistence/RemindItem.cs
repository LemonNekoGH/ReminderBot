using LemonNeko.ReminderBot.Bot.Persistence.Model;

namespace LemonNeko.ReminderBot.Bot.Persistence;

public partial class PersistenceContext
{
    public async Task CreateAsync(string remindName, long owner, long chatId)
    {
        await this.RemindItems!.AddAsync(new RemindItem
        {
            Owner = owner,
            Name = remindName,
            ChatId = chatId
        });
        await this.SaveChangesAsync();
    }
}
