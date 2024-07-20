using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using ReminderBot;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static CommunicationWithClient communicationWithClient = new();
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

        await communicationWithClient.Main(_botClient);

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
                        await communicationWithClient.Message(botClient, update);
                        break;

                    }

                case UpdateType.CallbackQuery:
                    {
                        await communicationWithClient.CallbackQuery(botClient, update);
                        break;
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
}