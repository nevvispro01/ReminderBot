using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;

namespace ReminderBot
{
    public class CommunicationWithClient
    {
        private enum StatucChat
        {
            DEFAULT,
            WRITE_TIME,
            WRITE_TEXT_REMINDER
        }
        private static Dictionary<long, StatucChat> statusChat = new();
        private static Dictionary<long, DateTime> time = new();
        private static TimerReminder timerReminder = new TimerReminder();

        public async Task Main(ITelegramBotClient botClient)
        {
            await timerReminder.Main(botClient);
        }
        public async Task Message(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            if (message == null) return;
            var chat = message.Chat;
            if (chat == null) return;
            if (!statusChat.ContainsKey(chat.Id))
            {
                statusChat.Add(chat.Id, StatucChat.DEFAULT);
            }

            switch (message.Type)
            {
                case MessageType.Text:
                    {
                        switch (statusChat[chat.Id])
                        {
                            case StatucChat.WRITE_TIME:
                                {
                                    await ParseWriteTime(botClient, chat.Id, message.Text);
                                    break;
                                }
                            case StatucChat.WRITE_TEXT_REMINDER:
                                {
                                    await ParseWriteTextReminder(botClient, chat.Id, message.Text);
                                    break;
                                }
                            default:
                                {
                                    await ParseDefault(botClient, chat.Id, message.Text);
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        statusChat[chat.Id] = StatucChat.DEFAULT;
                        await SendMessageForChatId(botClient, chat.Id, "Используй только текст!");
                        return;
                    }
            }
        }
        public async Task CallbackQuery(ITelegramBotClient botClient, Update update)
        {
            var callbackQuery = update.CallbackQuery;
            if (callbackQuery == null) return;
            if (callbackQuery.Message == null) return;
            var chat = callbackQuery.Message.Chat;

            switch (callbackQuery.Data)
            {

                case "addReminder":
                    {

                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                        if (!statusChat.ContainsKey(chat.Id)) statusChat.Add(chat.Id, StatucChat.WRITE_TIME);
                        await SendMessageForChatId(botClient, chat.Id, "Введите дату в формате dd.mm.yyyy hh:mm:ss, когда вам нужно напоминание");
                        break;
                    }
            }
        }

        private static async Task ParseWriteTime(ITelegramBotClient botClient, long chatId, string text)
        {
            statusChat[chatId] = StatucChat.DEFAULT;
            try
            {
                if (!time.ContainsKey(chatId)) time.Add(chatId, new DateTime());
                time[chatId] = DateTime.ParseExact(
                text,
                "dd.MM.yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);
            }
            catch
            {
                await SendMessageForChatId(botClient, chatId, "Неправильный формат даты");
                return;
            }

            if (DateTime.Compare(time[chatId], DateTime.Now) > 0)
            {
                statusChat[chatId] = StatucChat.WRITE_TEXT_REMINDER;
                await SendMessageForChatId(botClient, chatId, "Введите текст напоминания");
                return;

            }
            else
            {
                await SendMessageForChatId(botClient, chatId, "Заданное время уже в прошлом ;)");
            }
        }

        private static async Task ParseWriteTextReminder(ITelegramBotClient botClient, long chatId, string text)
        {

            statusChat[chatId] = StatucChat.DEFAULT;
            if (DateTime.Compare(time[chatId], DateTime.Now) > 0)
            {
                await timerReminder.AddReminder(time[chatId], chatId, text);
                await SendMessageForChatId(botClient, chatId, $"Добавлено напоминание: {time[chatId].ToString()}. Напомнить: {text}");
            }
            else
            {
                await SendMessageForChatId(botClient, chatId, "Заданное время стало прошлым, пока вы придумывали напоминание ;)");
            }
        }

        private static async Task ParseDefault(ITelegramBotClient botClient, long chatId, string text)
        {
            if (text == "/start")
            {
                await SendMessageForChatId(botClient, chatId, "Выбери клавиатуру:\n" +
                    "/inline\n" +
                    "/reply\n");
                return;
            }

            if (text == "/inline")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>()
                    {

                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Добавить напоминание", "addReminder"),
                                        },
                    });

                await botClient.SendTextMessageAsync(
                    chatId,
                    "Клавиатура Inline",
                    replyMarkup: inlineKeyboard);

            }
            else if (text == "/reply")
            {
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Добавить напоминание")

                        }
                    })
                {
                    ResizeKeyboard = true,
                };

                await botClient.SendTextMessageAsync(
                    chatId,
                    "Клавиатура Reply",
                    replyMarkup: replyKeyboard);

            }
            else if (text == "Добавить напоминание")
            {
                statusChat[chatId] = StatucChat.WRITE_TIME;
                await SendMessageForChatId(botClient, chatId, "Введите дату в формате dd.mm.yyyy hh:mm:ss, когда вам нужно напоминание");
            }
        }
        private static async Task SendMessageForChatId(ITelegramBotClient botClient, long chatId, string text)
        {
            await botClient.SendTextMessageAsync(
            chatId,
            text);
            return;
        }
    }
}
