using System.Globalization;
using Telegram.Bot;

namespace ReminderBot
{
    public class TimerReminder
    {

        private static DataBase dataBase = new();
        private static ITelegramBotClient botClient;
        public async Task Main(ITelegramBotClient _botClient)
        {
            botClient = _botClient;
            foreach (var reminder in dataBase.CheckReminder(botClient))
            {
                var time = DateTime.ParseExact(
                reminder.Time,
                "dd.MM.yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);
                var a = await Test();
                
                await InstallTimer(time, reminder.ChatId, reminder.Text, reminder.Id.ToString());
            }
        }

        private async Task<int> Test()
        {
            return 1;
        }

        public async Task AddReminder(DateTime time, long chatId, string text)
        {
            var idReminder = dataBase.AddReminder(chatId, text, time.ToString());
            await InstallTimer(time, chatId, text, idReminder);
        }

        public async Task InstallTimer(DateTime time, long chatId, string text, string idReminder)
        {
            
            var timer = new System.Timers.Timer((time - DateTime.Now).TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await SendReminder(chatId, text, idReminder);
            timer.AutoReset = false;
            timer.Enabled = true;
        }

        public static async Task SendReminder(long chatId, string message, string idReminder)
        {
            dataBase.RemoveReminder(idReminder);

            await botClient.SendTextMessageAsync(
                chatId,
                "Напоминие: " + message
            );
        }
    }
}
