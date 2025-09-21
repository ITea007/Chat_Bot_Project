using Interactive_Menu;
using Interactive_Menu.Core.Services;
using Interactive_Menu.Infrastructure.DataAccess;
using System;
using System.ComponentModel;
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
            Console.InputEncoding = Encoding.GetEncoding("UTF-16");
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
            var userRepository = new InMemoryUserRepository();
            var userService = new UserService(userRepository);
            var toDoRepository = new InMemoryToDoRepository();
            var toDoService = new ToDoService(toDoRepository);
            var toDoReportService = new ToDoReportService(toDoService);
            var handler = new UpdateHandler(botClient, userService, toDoService, toDoReportService);
            try
            {
                handler.OnHandleEventStarted += (message) => { Console.WriteLine($"Началась обработка сообщения '{message}'"); };
                handler.OnHandleEventCompleted += (message) => { Console.WriteLine($"Закончилась обработка сообщения '{message}'"); };
                botClient.StartReceiving(handler, cancellationToken: ct);

                var ping = await botClient.GetMe();
                await Console.Out.WriteLineAsync($"Нажмите клавишу A для выхода");
                var key = Console.ReadKey(intercept: true);
                if (key.KeyChar == 'a' || key.KeyChar == 'A' || key.KeyChar == 'а' || key.KeyChar == 'А')
                {
                    cts.Cancel();
                    Environment.Exit(0);
                }
                else
                {
                    await Console.Out.WriteLineAsync($"Telegram bot is alive - {ping.FirstName} - {ping.Username}");
                }
                //Продолжаем ждать нажатие клавиши A для завершения программы
                var waitForAKeyTask = WaitForAKeyAsync(ct);
                await Task.WhenAll(waitForAKeyTask);
                if (waitForAKeyTask.IsCompletedSuccessfully)
                {
                    cts.Cancel();
                    Environment.Exit(0);
                }
                await Task.Delay(Timeout.Infinite, ct);
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
                handler.OnHandleEventStarted -= (message) => { Console.WriteLine($"Началась обработка сообщения '{message}'"); };
                handler.OnHandleEventCompleted -= (message) => { Console.WriteLine($"Закончилась обработка сообщения '{message}'"); };
            }
        }

        // Метод для асинхронного ожидания нажатия клавиши A
        static async Task WaitForAKeyAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.A)
                        return;
                }
                await Task.Delay(100, ct);
            }
        }
    }


}
