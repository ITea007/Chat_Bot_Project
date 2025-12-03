using Interactive_Menu;
using Interactive_Menu.BackgroundTasks;
using Interactive_Menu.Core.Services;
using Interactive_Menu.Infrastructure.DataAccess;
using Interactive_Menu.Infrastructure.Services;
using Interactive_Menu.TelegramBot.Helpers;
using Interactive_Menu.TelegramBot.Scenarios;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            // Get connection string from environment variable
            string? connectionString = Environment.GetEnvironmentVariable("TODO_DB_CONNECTION_STRING", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(connectionString))
            {
                await Console.Out.WriteLineAsync("Database connection string not found. Please set the TODO_DB_CONNECTION_STRING environment variable.");
                return;
            }

            var receiverOptions = new ReceiverOptions{ AllowedUpdates = Array.Empty<UpdateType>() }; // Получать все типы обновлений 
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            var botClient = new TelegramBotClient(token);

            // Create data context factory
            var dataContextFactory = new DataContextFactory(connectionString); 
            // Create repositories with SQL implementation
            var userRepository = new SqlUserRepository(dataContextFactory);
            var toDoRepository = new SqlToDoRepository(dataContextFactory);
            var toDoListRepository = new SqlToDoListRepository(dataContextFactory);

            //var userRepository = new FileUserRepository("userrep");
            //var toDoRepository = new FileToDoRepository("filerep");
            //var toDoListRepository = new FileToDoListRepository("todolistrep");

            var userService = new UserService(userRepository);
            var toDoService = new ToDoService(toDoRepository);
            var toDoListService = new ToDoListService(toDoListRepository);
            var toDoReportService = new ToDoReportService(toDoService);
            var scenarioContextRepository = new InMemoryScenarioContextRepository();
            var notificationService = new NotificationService(dataContextFactory);
            var helper = new Helper();

            var scenarios = new List<IScenario>();
            scenarios.Add(new AddTaskScenario(userService, toDoService, toDoListService, helper));
            scenarios.Add(new AddListScenario(userService, toDoListService));
            scenarios.Add(new DeleteListScenario(userService, toDoListService, toDoService));
            scenarios.Add(new DeleteTaskScenario(toDoService));

            // Создание и настройка фоновых задач
            var backgroundTaskRunner = new BackgroundTaskRunner();

            // Добавляем задачу сброса сценариев по таймауту (1 час)
            var resetScenarioTask = new ResetScenarioBackgroundTask(
                TimeSpan.FromHours(1),
                scenarioContextRepository,
                botClient,
                helper);

            var todayBackgroundTask = new TodayBackgroundTask(notificationService, userRepository, toDoRepository);
            var deadlineBackgroundTask = new DeadlineBackgroundTask(notificationService, userRepository, toDoRepository);
            var notificationBackgroundTask = new NotificationBackgroundTask(notificationService, botClient);

            backgroundTaskRunner.AddTask(resetScenarioTask);
            backgroundTaskRunner.AddTask(todayBackgroundTask);
            backgroundTaskRunner.AddTask(deadlineBackgroundTask);
            backgroundTaskRunner.AddTask(notificationBackgroundTask);


            // Запускаем фоновые задачи
            backgroundTaskRunner.StartTasks(ct);

            var handler = new UpdateHandler(botClient, userService, toDoService, toDoReportService, scenarios, scenarioContextRepository, toDoListService, helper);
            try
            {
                handler.OnHandleEventStarted += (message, telegramId) => { Console.WriteLine($"Началась обработка сообщения '{message}' от '{telegramId}'"); };
                handler.OnHandleEventCompleted += (message, telegramId) => { Console.WriteLine($"Закончилась обработка сообщения '{message}' от '{telegramId}'"); };
                botClient.StartReceiving(handler, receiverOptions, cancellationToken: ct);
                var me = await botClient.GetMe();
                //Продолжаем ждать нажатие клавиши A для завершения программы
                var waitForAKeyTask = WaitForAKeyAsync(ct, me);
                await Task.WhenAll(waitForAKeyTask);
                if (waitForAKeyTask.IsCompletedSuccessfully)
                {
                    // Останавливаем фоновые задачи перед выходом
                    await backgroundTaskRunner.StopTasks(CancellationToken.None);
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
                // Останавливаем фоновые задачи в finally блоке
                await backgroundTaskRunner.StopTasks(CancellationToken.None);
                backgroundTaskRunner.Dispose();
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
