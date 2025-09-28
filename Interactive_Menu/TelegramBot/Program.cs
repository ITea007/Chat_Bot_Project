using Interactive_Menu;
using Interactive_Menu.Core.Services;
using Interactive_Menu.Infrastructure.DataAccess;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Interactive_Menu.TelegramBot
{
    internal class Program
    {
        /// <summary>
        /// Точка запуска программы. Создаём клиента, сервисы, начинаем получать и обрабатывать сообщения.
        /// </summary>
        public static async Task Main()
        {
            Console.InputEncoding = Encoding.GetEncoding("UTF-8");
            // Get token from environment variable
            string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(token))
            {
                await Console.Out.WriteLineAsync("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                return;
            }
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            var botClient = new TelegramBotClient(token);
            var userRepository = new FileUserRepository("userrep");
            var userService = new UserService(userRepository);
            var toDoRepository = new FileToDoRepository("filerep");
            var toDoService = new ToDoService(toDoRepository);
            var toDoReportService = new ToDoReportService(toDoService);
            var handler = new UpdateHandler(botClient, userService, toDoService, toDoReportService);
            try
            {
                await botClient.SetMyCommands(handler.CommandsBeforeRegistration, cancellationToken:ct);
                handler.OnHandleEventStarted += (message, telegramId) => { Console.WriteLine($"Началась обработка сообщения '{message}' от '{telegramId}'"); };
                handler.OnHandleEventCompleted += (message, telegramId) => { Console.WriteLine($"Закончилась обработка сообщения '{message}' от '{telegramId}'"); };
                botClient.StartReceiving(handler, cancellationToken: ct);
                var me = await botClient.GetMe();
                //Продолжаем ждать нажатие клавиши A для завершения программы
                var waitForAKeyTask = WaitForAKeyAsync(ct, me);
                await Task.WhenAll(waitForAKeyTask);
                if (waitForAKeyTask.IsCompletedSuccessfully)
                {
                    cts.Cancel();
                    Environment.Exit(0);
                }
            }
            catch (Exception Ex)
            {
                await Console.Out.WriteLineAsync("Произошла непредвиденная ошибка:");
                await Console.Out.WriteLineAsync($"Тип исключения: {Ex.GetType()}");
                await Console.Out.WriteLineAsync($"Исключение: {Ex.Message}");
                await Console.Out.WriteLineAsync($"Трассировка стека: {Ex.StackTrace}");
                await Console.Out.WriteLineAsync($"Внутреннее исключение: {Ex.InnerException}");
            }
            finally
            {
                handler.OnHandleEventStarted -= (message, telegramId) => { Console.WriteLine($"Началась обработка сообщения '{message}' от '{telegramId}'"); };
                handler.OnHandleEventCompleted -= (message, telegramId) => { Console.WriteLine($"Закончилась обработка сообщения '{message}' от '{telegramId}'"); };
            }
        }

        // Метод для асинхронного ожидания нажатия клавиши A
        static async Task WaitForAKeyAsync(CancellationToken ct, User? me)
        {
            await Console.Out.WriteLineAsync($"Нажмите клавишу A для выхода");
            while (!ct.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.A)
                        return;
                    else
                    {
                        if (me != null)
                            await Console.Out.WriteLineAsync($"Telegram bot is alive - {me.Id} - {me.FirstName} - {me.Username}");
                        await Console.Out.WriteLineAsync($"Нажмите клавишу A для выхода");
                    }
                }
                await Task.Delay(500, ct);
            }
        }
    }


}
