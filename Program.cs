using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Globalization;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static bool writeDate = false;
    private static bool writeTextReminder = false;
    private static DateTime time;
    static async Task Main()
    {

        _botClient = new TelegramBotClient("6198257844:AAFjIAbe4vpG4zzkT5Upf-y6_B2UapgY2uc");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            },
            ThrowPendingUpdates = true,
        };

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions);

        Console.WriteLine($"Сервер запущен!");

        await Task.Delay(-1);
    }
    
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        var message = update.Message;
                        if (message == null) return;
                        var chat = message.Chat;

                        switch (message.Type)
                        {
                            case MessageType.Text:
                                {
                                    
                                    if (writeDate)
                                    {
                                        writeDate = false;
                                        try
                                        {
                                            time = DateTime.ParseExact(
                                            message.Text,
                                            "dd.MM.yyyy HH:mm:ss",
                                            CultureInfo.InvariantCulture);
                                        }
                                        catch 
                                        {
                                            await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Неправильный формат даты");
                                            return;
                                        }   

                                        if (DateTime.Compare(time, DateTime.Now) > 0)
                                        {
                                            writeTextReminder = true;
                                            await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Введите текст напоминания");
                                            return;

                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Заданное время уже в прошлом ;)");
                                        }
                                        
                                    }

                                    if (writeTextReminder)
                                    {
                                        writeTextReminder = false;

                                        if (DateTime.Compare(time, DateTime.Now) > 0)
                                        {
                                            var timer = new System.Timers.Timer((time - DateTime.Now).TotalMilliseconds);
                                            timer.Elapsed += async (sender, e) => await SendReminder(botClient, chat.Id, message.Text);
                                            timer.AutoReset = false;
                                            timer.Enabled = true;

                                            await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            $"Добавлено напоминание: {time.ToString()}. Напомнить: {message.Text}");
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Заданное время стало прошлым, пока вы придумывали напоминание ;)");
                                        }
                                        return;
                                    }

                                    if (message.Text == "/start")
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Выбери клавиатуру:\n" +
                                            "/inline\n" +
                                            "/reply\n");
                                        return;
                                    }

                                    if (message.Text == "/inline")
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
                                            chat.Id,
                                            "Клавиатура Inline",
                                            replyMarkup: inlineKeyboard);

                                        return;
                                    }

                                    if (message.Text == "/reply")
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

                                        if (writeDate) writeDate = false;

                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Клавиатура Reply",
                                            replyMarkup: replyKeyboard);

                                        return;
                                    }

                                    if (message.Text == "Добавить напоминание")
                                    {
                                        
                                        writeDate = true;
                                        await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        "Введите дату в формате dd.mm.yyyy hh:mm:ss, когда вам нужно напоминание");
                                        return;
                                    }

                                    return;
                                }

                            default:
                                {
                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        "Используй только текст!");
                                    return;
                                }
                        }

                    }

                case UpdateType.CallbackQuery:
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

                                    writeDate = true;
                                    await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Введите дату в формате dd.mm.yyyy hh:mm:ss, когда вам нужно напоминание");
                                    return;
                                }
                        }

                        return;
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public static async Task SendReminder(ITelegramBotClient botClient, long chatId, string message)
    {
        await botClient.SendTextMessageAsync(
            chatId,
            "Напоминие: " + message
        );
    }

}