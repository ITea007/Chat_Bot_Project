using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using Interactive_Menu.Core.Services;
using Interactive_Menu.TelegramBot.Dto;
using Interactive_Menu.TelegramBot.DTO;
using Interactive_Menu.TelegramBot.Helpers;
using Interactive_Menu.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Xsl;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace Interactive_Menu.TelegramBot
{
    /// <summary>
    /// UpdateHandler отвечает за обработку обновлений.
    /// </summary>
    internal class UpdateHandler : IUpdateHandler
    {
        private IUserService _userService;
        private ToDoService _toDoService;
        private IToDoReportService _toDoReportService;
        private Helper _helper;
        private bool _isTaskCountLimitSet { get; set; } = true;
        private bool _isTaskLengthLimitSet { get; set; } = true;
        private IEnumerable<IScenario> _scenarios;
        private IScenarioContextRepository _contextRepository;
        private ITelegramBotClient _bot;
        private IToDoListService _toDoListService;
        private static int _pageSize = 5;

        public delegate void MessageEventHandler(string message, long telegramId);
        public event MessageEventHandler? OnHandleEventStarted;
        public event MessageEventHandler? OnHandleEventCompleted;

        public List<BotCommand> CommandsAfterRegistration { get; } = new List<BotCommand> {
                    { new BotCommand("/start", "Начинает работу с ботом") }, { new BotCommand("/help", "Показывает справку по командам") },
                    { new BotCommand("/info", "Показывает информацию по боту") }, { new BotCommand("/addtask", "Добавляет задачу")},
                    { new BotCommand("/show", "Показывает все активные задачи")}, { new BotCommand("/report", "Выводит отчет по задачам")}, 
                    { new BotCommand("/find", "Ищет задачу") }
                };

        public List<BotCommand> CommandsBeforeRegistration { get; } = new List<BotCommand> {
                    { new BotCommand("/start", "Начинает работу с ботом") }, { new BotCommand("/help", "Показывает справку по командам") },
                    { new BotCommand("/info", "Показывает информацию по боту")  }
                };

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService,
            IToDoReportService toDoReportService, IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository, IToDoListService toDoListService, Helper helper)
        {
            _bot = botClient;
            _userService = userService;
            _toDoService = (ToDoService)toDoService;
            _toDoReportService = toDoReportService;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
            _helper = helper;
            _toDoListService = toDoListService;
        }

        // Метод для обработки CallbackQuery
        private async Task OnCallbackQuery(Update update, CallbackQuery callbackQuery, CancellationToken ct)
        {
            if (callbackQuery.Message is null || callbackQuery.From is null)
            {
                await _bot.AnswerCallbackQuery(callbackQuery.Id, "Ошибка обработки запроса", cancellationToken: ct);
                return;
            }

            // Проверка регистрации пользователя
            var user = await _userService.GetUser(callbackQuery.From.Id, ct);
            if (user is null)
            {
                await _bot.AnswerCallbackQuery(callbackQuery.Id, "Вы не зарегистрированы. Используйте /start", cancellationToken: ct);
                return;
            }

            // Проверка на активный сценарий
            var context = await _contextRepository.GetContext(callbackQuery.From.Id, ct);
            if (context is not null)
            {
                await ProcessScenario(context, update, ct);
                await _bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Подтверждаем обработку
                return;
            }

            // Обработка callback данных
            var callbackData = CallbackDto.FromString(callbackQuery.Data ?? "");

            switch (callbackData.Action)
            {
                case "show":
                case "show_completed":
                    await HandleShowListCallback(callbackQuery, ct);
                    break;
                case "addlist":
                    await StartAddListScenario(callbackQuery, ct);
                    break;
                case "deletelist":
                    await StartDeleteListScenario(callbackQuery, ct);
                    break;
                case "select":
                    await HandleListSelectionCallback(callbackQuery, ct);
                    break;
                case "showtask":
                    await HandleShowTaskCallback(callbackQuery, ct);
                    break;
                case "completetask":
                    await HandleCompleteTaskCallback(callbackQuery, ct);
                    break;
                case "deletetask":
                    await StartDeleteTaskScenario(callbackQuery, ct);
                    break;
                default:
                    await _bot.AnswerCallbackQuery(callbackQuery.Id, "Неизвестное действие", cancellationToken: ct);
                    break;
            }

            await _bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Всегда подтверждаем обработку
        }

        private async Task HandleShowTaskCallback(CallbackQuery callbackQuery, CancellationToken ct)
        {
            if (callbackQuery.Message is null) return;

            var itemCallback = ToDoItemCallbackDto.FromString(callbackQuery.Data ?? "");
            var task = await _toDoService.Get(itemCallback.ToDoItemId, ct);

            if (task is null)
            {
                await _bot.EditMessageText(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    "Задача не найдена.",
                    cancellationToken: ct);
                return;
            }

            var taskText = new StringBuilder();
            taskText.AppendLine($"📝 *Задача: {task.Name}*");
            taskText.AppendLine($"📅 *Срок: {task.Deadline:dd.MM.yyyy}*");
            taskText.AppendLine($"🔄 *Статус: {(task.State == ToDoItemState.Active ? "Активна" : "Выполнена")}*");
            if (task.List != null)
                taskText.AppendLine($"📋 *Список: {task.List.Name}*");
            taskText.AppendLine($"🆔 `{task.Id}`");

            var completeCallback = new ToDoItemCallbackDto { Action = "completetask", ToDoItemId = task.Id };
            var deleteCallback = new ToDoItemCallbackDto { Action = "deletetask", ToDoItemId = task.Id };

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Выполнить", completeCallback.ToString()),
                    InlineKeyboardButton.WithCallbackData("❌ Удалить", deleteCallback.ToString())
                }
            });

            await _bot.EditMessageText(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                taskText.ToString(),
                ParseMode.Markdown,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);
        }

        private async Task HandleCompleteTaskCallback(CallbackQuery callbackQuery, CancellationToken ct)
        {
            if (callbackQuery.Message is null) return;

            var itemCallback = ToDoItemCallbackDto.FromString(callbackQuery.Data ?? "");

            try
            {
                await _toDoService.MarkAsCompleted(itemCallback.ToDoItemId, ct);

                await _bot.EditMessageText(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    $"✅ Задача отмечена как выполненная!",
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await _bot.EditMessageText(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    $"❌ Ошибка: {ex.Message}",
                    cancellationToken: ct);
            }
        }

        private async Task StartDeleteTaskScenario(CallbackQuery callbackQuery, CancellationToken ct)
        {
            var scenarioContext = new ScenarioContext(ScenarioType.DeleteTask, callbackQuery.From.Id);
            var itemCallback = ToDoItemCallbackDto.FromString(callbackQuery.Data ?? "");
            scenarioContext.Data["ToDoItemId"] = itemCallback.ToDoItemId;
            await ProcessScenario(scenarioContext, new Update { CallbackQuery = callbackQuery }, ct);
        }

        private async Task HandleListSelectionCallback(CallbackQuery callbackQuery, CancellationToken ct)
        {
            // Ищем активный сценарий для пользователя
            var context = await _contextRepository.GetContext(callbackQuery.From!.Id, ct);
            if (context is not null)
            {
                await ProcessScenario(context, new Update { CallbackQuery = callbackQuery }, ct);
            }
            else
            {
                await _bot.AnswerCallbackQuery(callbackQuery.Id, "Нет активного сценария", cancellationToken: ct);
            }
        }

        private async Task HandleShowListCallback(CallbackQuery callbackQuery, CancellationToken ct)
        {
            //if (callbackQuery.Message is null) throw new ArgumentNullException(nameof(callbackQuery.Message));
            if (callbackQuery.Message is null)
                return;
            var listCallback = PagedListCallbackDto.FromString(callbackQuery.Data ?? "");
            var user = await _userService.GetUser(callbackQuery.From.Id, ct);

            if (user is null) return;

            IReadOnlyList<ToDoItem> tasks;
            string title;

            if (listCallback.Action == "show_completed")
            {
                tasks = (await _toDoService.GetAllByUserId(user.UserId, ct))
                    .Where(t => t.State == ToDoItemState.Completed && t.List?.Id == listCallback.ToDoListId)
                    .ToList();
                title = "Выполненные задачи в списке:";
            }
            else
            {
                tasks = await _toDoService.GetByUserIdAndList(user.UserId, listCallback.ToDoListId, ct);
                title = "Задачи в списке:";
            }

            var tasksText = new StringBuilder();
            tasksText.AppendLine(title);

            if (tasks.Count == 0)
            {
                tasksText.AppendLine("Список пуст");
            }
            else
            {
                var taskButtons = tasks.Select(task =>
                {
                    var callback = new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = task.Id };
                    var statusIcon = task.State == ToDoItemState.Completed ? "✅" : "⏳";
                    return KeyValuePair.Create($"{statusIcon} {task.Name} - {task.Deadline:dd.MM.yyyy}", callback.ToString());
                }).ToList();

                var inlineKeyboard = BuildPagedButtons(taskButtons, listCallback);

                await _bot.EditMessageText(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    tasksText.ToString(),
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct);
                return;
            }
            // Если задач нет, показываем только кнопки управления
            var controlButtons = new List<KeyValuePair<string, string>>();
            if (listCallback.Action == "show")
            {
                var completedCallback = new PagedListCallbackDto { Action = "show_completed", ToDoListId = listCallback.ToDoListId, Page = 0 };
                controlButtons.Add(KeyValuePair.Create("☑️ Посмотреть выполненные", completedCallback.ToString()));
            }

            var backCallback = new PagedListCallbackDto { Action = "show", ToDoListId = null, Page = 0 };
            controlButtons.Add(KeyValuePair.Create("🔙 Назад к спискам", backCallback.ToString()));

            var finalKeyboard = BuildPagedButtons(controlButtons, listCallback);

            await _bot.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, tasksText.ToString(), replyMarkup: finalKeyboard, cancellationToken: ct);
        }

        private InlineKeyboardMarkup BuildPagedButtons(IReadOnlyList<KeyValuePair<string, string>> callbackData, PagedListCallbackDto listDto)
        {
            var totalPages = (int)Math.Ceiling(callbackData.Count / (double)_pageSize);
            var currentPageButtons = callbackData.GetBatchByNumber(_pageSize, listDto.Page).ToList();

            var keyboardButtons = new List<InlineKeyboardButton[]>();

            // Добавляем кнопки задач
            foreach (var button in currentPageButtons)
            {
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(button.Key, button.Value) });
            }

            // Добавляем кнопки навигации
            var navigationButtons = new List<InlineKeyboardButton>();

            if (listDto.Page > 0)
            {
                var prevCallback = new PagedListCallbackDto { Action = listDto.Action, ToDoListId = listDto.ToDoListId, Page = listDto.Page - 1 };
                navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", prevCallback.ToString()));
            }

            if (listDto.Page < totalPages - 1 && totalPages > 1)
            {
                var nextCallback = new PagedListCallbackDto { Action = listDto.Action, ToDoListId = listDto.ToDoListId, Page = listDto.Page + 1 };
                navigationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", nextCallback.ToString()));
            }

            if (navigationButtons.Any())
            {
                keyboardButtons.Add(navigationButtons.ToArray());
            }

            // Добавляем кнопки действий (только для основной страницы списков)
            if (listDto.Action == "show" && listDto.Page == 0 && !listDto.ToDoListId.HasValue)
            {
                keyboardButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("🆕 Добавить", "addlist"),
                    InlineKeyboardButton.WithCallbackData("❌ Удалить", "deletelist")
                });
            }

            return new InlineKeyboardMarkup(keyboardButtons);
        }

        private async Task StartAddListScenario(CallbackQuery callbackQuery, CancellationToken ct)
        {
            var scenarioContext = new ScenarioContext(ScenarioType.AddList, callbackQuery.From.Id);
            await ProcessScenario(scenarioContext, new Update { CallbackQuery = callbackQuery }, ct);
        }

        private async Task StartDeleteListScenario(CallbackQuery callbackQuery, CancellationToken ct)
        {
            var scenarioContext = new ScenarioContext(ScenarioType.DeleteList, callbackQuery.From.Id);
            await ProcessScenario(scenarioContext, new Update { CallbackQuery = callbackQuery }, ct);
        }

        //Возвращает соответствующий сценарий. Если сценарий не найден, то выбрасывает исключение ScenarioNotFoundException.
        public Task<IScenario> GetScenario(ScenarioType scenario)
        {
            var output = _scenarios.Where(s => s.CanHandle(scenario)).FirstOrDefault();
            if (output != null)
                return Task.FromResult(output);
            else
                throw new ScenarioNotFoundException(scenario);
        }

        public async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message?.From is null && update.CallbackQuery?.From is null)
                return;

            var userId = update.Message?.From?.Id ?? update.CallbackQuery!.From!.Id;

            var scenario = await GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(_bot, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(userId, ct);
            }
            else
            {
                await _contextRepository.SetContext(userId, context, ct);
            }
        }




        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                await (update switch
                {
                    { Message: { } message } => HandleMessageUpdate(update, ct),
                    { CallbackQuery: { } callbackQuery } => OnCallbackQuery(update, callbackQuery, ct),
                    _ => Task.CompletedTask
                });
            }
            catch (Exception Ex)
            {
                await Console.Out.WriteLineAsync("Произошла непредвиденная ошибка:");
                await Console.Out.WriteLineAsync($"Тип исключения: {Ex.GetType()}");
                await Console.Out.WriteLineAsync($"Исключение: {Ex.Message}");
                await Console.Out.WriteLineAsync($"Трассировка стека: {Ex.StackTrace}");
                await Console.Out.WriteLineAsync($"Внутреннее исключение: {Ex.InnerException}");
            }
        }
 

        private async Task HandleMessageUpdate(Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) return;

            if (update.Message.Text != null)
            {
                OnHandleEventStarted?.Invoke(update.Message.Text, update.Message.From.Id);

                var command = update.Message.Text.Trim().ToLower();
                var trimmedCommand = command.Split(' ', 2)[0];
                var user = await _userService.GetUser(update.Message.From.Id, ct);

                // Проверка регистрации пользователя (кроме команды /start)
                if (trimmedCommand != "/start" && user == null)
                {
                    await _bot.SetMyCommands(CommandsBeforeRegistration, cancellationToken: ct);
                    await _bot.SendMessage(
                        update.Message.Chat,
                        "Вы не зарегистрированы. Нажмите /start для начала.",
                        replyMarkup: _helper._keyboardBeforeRegistration,
                        cancellationToken: ct);

                    OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
                    return;
                }

                // Проверка активного сценария
                var context = await _contextRepository.GetContext(update.Message.From.Id, ct);
                if (context is not null)
                {
                    if (trimmedCommand == "/cancel")
                    {
                        await OnCancelCommand(_bot, update, ct);
                        OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
                        return;
                    }

                    // Если сценарий активен - обрабатываем через ProcessScenario
                    await ProcessScenario(context, update, ct);
                    OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
                    return;
                }

                // Обработка команд для зарегистрированных пользователей
                if (user != null && CommandsAfterRegistration.Any(i => i.Command == trimmedCommand) && _isTaskCountLimitSet && _isTaskLengthLimitSet)
                {
                    await ExecuteCommand(_bot, update, trimmedCommand, ct);
                }
                // Обработка установки лимитов
                else if (!_isTaskCountLimitSet)
                {
                    await SetTaskCountLimit(_bot, update, trimmedCommand, ct);
                }
                else if (!_isTaskLengthLimitSet)
                {
                    await SetTaskLengthLimit(_bot, update, trimmedCommand, ct);
                }
                // Обработка команд для незарегистрированных пользователей
                else if (user == null && CommandsBeforeRegistration.Any(i => i.Command == trimmedCommand))
                {
                    await ExecuteCommand(_bot, update, trimmedCommand, ct);
                }
                else
                {
                    await _bot.SendMessage(update.Message.Chat, "Неизвестная команда.", cancellationToken: ct);
                }

                OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
            }
            else
            {
                await _bot.SendMessage(update.Message.Chat, "Получил пустой текст.", cancellationToken: ct);
                OnHandleEventCompleted?.Invoke("Empty message", update.Message.From.Id);
            }
        }

        private async Task OnCancelCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null)
                return;

            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user is null) throw new UserNotFoundException(update.Message.From.Id);
            await _contextRepository.ResetContext(user.TelegramUserId, ct);
            await botClient.SendMessage(update.Message.Chat, $"Текущий сценарий отменен", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
        }

        private async Task ExecuteCommand(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null)
                return;

            var user = await _userService.GetUser(update.Message.From.Id, ct);
            var isRegistered = user != null;

            // Установка соответствующих команд меню
            if (isRegistered)
            {
                await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
            }
            else
            {
                await botClient.SetMyCommands(CommandsBeforeRegistration, cancellationToken: ct);
            }

            switch (command)
            {
                case "/start":
                    await OnStartCommand(botClient, update, ct);
                    break;
                case "/help":
                    await OnHelpCommand(botClient, update, ct);
                    break;
                case "/info":
                    await OnInfoCommand(botClient, update, ct);
                    break;
                case "/addtask":
                    if (isRegistered)
                        await OnAddTaskCommand(update, ct);
                    else
                        await botClient.SendMessage(update.Message.Chat, "Эта команда доступна только зарегистрированным пользователям.", cancellationToken: ct);
                    break;
                case "/show":
                    if (isRegistered)
                        await OnShowCommand(botClient, update, ct);
                    else
                        await botClient.SendMessage(update.Message.Chat, "Эта команда доступна только зарегистрированным пользователям.", cancellationToken: ct);
                    break;
                case "/report":
                    if (isRegistered)
                        await OnReportCommand(botClient, update, ct);
                    else
                        await botClient.SendMessage(update.Message.Chat, "Эта команда доступна только зарегистрированным пользователям.", cancellationToken: ct);
                    break;
                case "/find":
                    if (isRegistered)
                        await OnFindCommand(botClient, update, ct);
                    else
                        await botClient.SendMessage(update.Message.Chat, "Эта команда доступна только зарегистрированным пользователям.", cancellationToken: ct);
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды.", cancellationToken: ct);
                    break;
            }

            // Показ соответствующей клавиатуры
            if (isRegistered && command != "/addtask")
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    "Выберите команду:",
                    replyMarkup: _helper._keyboardAfterRegistration,
                    cancellationToken: ct);
            }
            else if (!isRegistered && command != "/start")
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    "Выберите команду:",
                    replyMarkup: _helper._keyboardBeforeRegistration,
                    cancellationToken: ct);
            }
        }

        /// <summary>
        /// Метод возвращает все задачи пользователя, которые начинаются на namePrefix. Обработку команды /find. 
        ///Пример команды: /find Имя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        private async Task OnFindCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.Text is null|| update.Message.From is null)
                return;


            var task = update.Message.Text.Trim();
            string namePrefix = task.Remove(0, "/find".Length).Trim();
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var tasksList = await _toDoService.Find(user, namePrefix, ct);
                StringBuilder outputBuilder = new StringBuilder();
                if (tasksList.Count == 0 || !tasksList.Any(i => i.State == ToDoItemState.Active))
                    outputBuilder.AppendLine($"Нет активных задач, начинающихся на {namePrefix}");
                else
                {
                    outputBuilder.AppendLine($"Задачи, начинающиеся на {namePrefix}");
                    foreach (var taskItem in tasksList.Where(t => t.State == ToDoItemState.Active))
                        outputBuilder.AppendLine($"Имя задачи '{taskItem.Name}' - {taskItem.CreatedAt} - `{taskItem.Id}`");
                }
                await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), ParseMode.Markdown, cancellationToken: ct);
            }
        }

        /// <summary>
        /// Обработка команды /report.Используется IToDoReportService. Пример вывода: 
        /// Статистика по задачам на 01.01.2025 00:00:00. Всего: 10; Завершенных: 7; Активных: 3;
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        private async Task OnReportCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            //if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            //if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            if (update.Message is null || update.Message.From is null)
                return;
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var result = await _toDoReportService.GetUserStats(user.UserId, ct);
                await botClient.SendMessage(update.Message.Chat,
                    $"Статистика по задачам на {result.generatedAt}. " +
                    $"Всего: {result.total}; Завершенных: {result.completed}; Активных: {result.active};", cancellationToken: ct);
            }
        }

        private async Task OnShowCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) return;

            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user is null) return;

            var userLists = await _toDoListService.GetUserLists(user.UserId, ct);
            var callbackData = new List<KeyValuePair<string, string>>();

            // Кнопка "Без списка"
            var noListCallback = new PagedListCallbackDto { Action = "show", ToDoListId = null, Page = 0 };
            callbackData.Add(KeyValuePair.Create("📌 Без списка", noListCallback.ToString()));

            // Кнопки для каждого списка
            foreach (var list in userLists)
            {
                var listCallback = new PagedListCallbackDto { Action = "show", ToDoListId = list.Id, Page = 0 };
                callbackData.Add(KeyValuePair.Create($"📋 {list.Name}", listCallback.ToString()));
            }

            var inlineKeyboard = BuildPagedButtons(callbackData, new PagedListCallbackDto { Action = "show", Page = 0 });

            await botClient.SendMessage(
                update.Message.Chat.Id,
                "Выберите список:",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);
        }

        private async Task OnAddTaskCommand(Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null)
                return;

            //При получении команды / addtask создать ScenarioContext c ScenarioType.AddTask и вызвать метод ProcessScenario
            var scenarioContext = new ScenarioContext(ScenarioType.AddTask, update.Message.From.Id);
            await ProcessScenario(scenarioContext, update, ct);
        }

        private async Task OnInfoCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            string newlineSymbol = Environment.NewLine;
            if (update.Message is null )
                return;
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine($"{newlineSymbol}" +
                                $"*  Текущая версия программы 12.0.  Дата создания 20-11-2025{newlineSymbol}" +
                                $"   Реализована пагинация, просмотр выполненных задач, удаление задач через сценарии (ДЗ 13) {newlineSymbol}" +
                                $"*  Текущая версия программы 11.0.  Дата создания 10-11-2025{newlineSymbol}" +
                                $"   Реализована работа cо списками для задач (ДЗ 12) {newlineSymbol}" +
                                $"*  Текущая версия программы 10.0.  Дата создания 09-10-2025{newlineSymbol}" +
                                $"   Реализована работа cо сценариями и команда отмены сценария /cancel (ДЗ 11) {newlineSymbol}" +
                                $"*  Текущая версия программы 9.0.  Дата создания 24-09-2025{newlineSymbol}" +
                                $"   Реализована работа c файлами (ДЗ 10) {newlineSymbol}" +
                                $"*  Версия программы 8.0.  Дата создания 22-09-2025{newlineSymbol}" +
                                $"   Реализована работа телеграмм бота (ДЗ 9) {newlineSymbol}" +
                                $"*  Версия программы 7.0.  Дата создания 16-09-2025{newlineSymbol}" +
                                $"   Реализовано асинхронное выполнение (ДЗ 8) {newlineSymbol}" +
                                $"*  Версия программы 6.0.  Дата создания 06-09-2025{newlineSymbol}" +
                                $"   Реализована команда /report, /find (ДЗ 7) {newlineSymbol}" +
                                $"*  Версия программы 5.0.  Дата создания 02-09-2025{newlineSymbol}" +
                                $"   Удалена команда /echo, реализовна эмуляция работы с ботом (ДЗ 6) {newlineSymbol}" +
                                $"*  Версия программы 4.0.  Дата создания 28-07-2025{newlineSymbol}" +
                                $"   Добавлены команды /completetask, /showalltasks, изменена логика команды /showtasks (ДЗ 5) {newlineSymbol}" +
                                $"*  Версия программы 3.0.  Дата создания 19-06-2025{newlineSymbol}" +
                                $"   Добавлена обработка ошибок через исключения (ДЗ 4) {newlineSymbol}" +
                                $"*  Версия программы 2.0.  Дата создания 16-06-2025{newlineSymbol}" +
                                $"   Добавлены команды /addtask, /showtasks, /removetask{newlineSymbol}" +
                                $"*  Версия программы 1.0.  Дата создания 25-05-2025{newlineSymbol}" +
                                "   Реализованы команды /start, /help, /info, /echo, /exit");

            await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
        }

        private async Task OnHelpCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            string newlineSymbol = Environment.NewLine;
            if (update.Message is null || update.Message.From is null)
                return;
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append(
                "Cправка по программе:" +
                $"{newlineSymbol}Команда /start: Регистрация нового пользователя в программе. После регистрации будут доступны основные команды." +
                $"{newlineSymbol}Команда /help: Отображает краткую справочную информацию о том, как пользоваться программой. Отображается описание доступных команд." +
                $"{newlineSymbol}Команда /info: Предоставляет информацию о версии программы и дате её создания."
            );
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
                outputBuilder.AppendLine(
                    $"{newlineSymbol}Команда /cancel: Отменяет текущий сценарий." +
                    $"{newlineSymbol}Команда /report: Отображает краткую статистику по текущим задачам." +
                    $"{newlineSymbol}Команда /find: Отображает все задачи пользователя, которые начинаются на заданное слово. Например, команда /find Имя веведет все " +
                    $"команды, начинающиеся на Имя" +
                    $"{newlineSymbol}Команда /addtask: После ввода команды добавьте описание задачи. После добавления задачи выводится сообщение, что задача добавлена." +
                    $"{newlineSymbol}\t- Максимальная длина задачи:{(_toDoService.TaskLengthLimit == -1 ? "не задано" : _toDoService.TaskLengthLimit)}" +
                    $"{newlineSymbol}\t- Максимальное количество задач:{(_toDoService.TaskCountLimit == -1 ? "не задано" : _toDoService.TaskCountLimit)}" +
                    $"{newlineSymbol}Команда /show: Показывает списки задач с возможностью просмотра, выполнения и удаления задач через интерактивные кнопки." +
                    $"{newlineSymbol}\t- Нажмите на задачу для просмотра деталей" +
                    $"{newlineSymbol}\t- Используйте кнопки '✅ Выполнить' и '❌ Удалить' для управления задачами" +
                    $"{newlineSymbol}\t- Просматривайте выполненные задачи через кнопку '☑️ Посмотреть выполненные'" +
                    $"{newlineSymbol}\t- Используйте пагинацию для навигации по большому количеству задач"
            );

            await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
        }

        private async Task OnStartCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null)
                return;

            var existingUser = await _userService.GetUser(update.Message.From.Id, ct);

            if (existingUser != null)
            {
                await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"Привет, {update.Message.From.Username}. Вы уже зарегистрированы.",
                    replyMarkup: _helper._keyboardAfterRegistration,
                    cancellationToken: ct);
            }
            else if (update.Message.From.Username != null)
            {
                await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
                await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"Привет, {update.Message.From.Username}. Регистрация завершена!",
                    replyMarkup: _helper._keyboardAfterRegistration,
                    cancellationToken: ct);
            }

            // Проверка и установка лимитов
            if (_toDoService.TaskLengthLimit == -1)
            {
                _isTaskLengthLimitSet = false;
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"{update.Message.From.Username}, введи максимальную длину задачи от {_toDoService.MinTaskLengthLimit} до {_toDoService.MaxTaskLengthLimit}",
                    cancellationToken: ct);
            }
                

            if (_toDoService.TaskCountLimit == -1)
            {
                _isTaskCountLimitSet = false;
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"{update.Message.From.Username}, введи максимальное количество задач от {_toDoService.MinTaskCountLimit} до {_toDoService.MaxTaskCountLimit}",
                    cancellationToken: ct);
            }
        }

        /// <summary>
        /// Установка ограничения на максимальное количество задач в диапазоне.
        /// </summary>
        private async Task SetTaskCountLimit(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null)
                return;
            _toDoService.TaskCountLimit = ParseAndValidateInt(command, _toDoService.MinTaskCountLimit, _toDoService.MaxTaskCountLimit);
            _isTaskCountLimitSet = true;
            await botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, установлено максимальное количество задач: {_toDoService.TaskCountLimit}", cancellationToken: ct);

            await botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, введи максимальную длину задачи от {_toDoService.MinTaskLengthLimit} до {_toDoService.MaxTaskLengthLimit}", cancellationToken: ct);
        }

        /// <summary>
        /// Установка ограничения на допустимую длину задачи.
        /// </summary>
        private async Task SetTaskLengthLimit(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
        {
            //if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            //if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            if (update.Message is null || update.Message.From is null)
                return;
            _toDoService.TaskLengthLimit = ParseAndValidateInt(command, _toDoService.MinTaskLengthLimit, _toDoService.MaxTaskLengthLimit);
            _isTaskLengthLimitSet = true;
            await botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, установлена максимальная длина задачи: {_toDoService.TaskLengthLimit}", cancellationToken: ct);
        }

        /// <summary>
        /// Метод приводит полученную строку к int и проверяет, что оно находится в диапазоне min и max. 
        /// В противном случае выбрасывает ArgumentException с сообщением.
        /// </summary>
        /// <param name="str">Строка для обработки</param>
        /// <param name="min">Минимальное значение диапазона</param>
        /// <param name="max">Максимальное значение диапазона</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        static int ParseAndValidateInt(string? str, int min, int max)
        {
            int result = 0;
            bool isParsed = int.TryParse(str, out result);
            if (isParsed && result >= min && result <= max)
                return result;
            else
                throw new ArgumentException($"Значение должно быть в диапазоне от {min} до {max}");
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync($"Ошибка произошла: {exception.Message}, источник: {source}");
        }
    }
}




