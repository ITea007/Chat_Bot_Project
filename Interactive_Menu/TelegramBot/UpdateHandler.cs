using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using Interactive_Menu.Core.Services;
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
using static System.Net.Mime.MediaTypeNames;

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

        public delegate void MessageEventHandler(string message, long telegramId);
        public event MessageEventHandler? OnHandleEventStarted;
        public event MessageEventHandler? OnHandleEventCompleted;

        public List<BotCommand> CommandsAfterRegistration { get; } = new List<BotCommand> {
                    { new BotCommand("/start", "Начинает работу с ботом") }, { new BotCommand("/help", "Показывает справку по командам") },
                    { new BotCommand("/info", "Показывает информацию по боту") }, { new BotCommand("/addtask", "Добавляет задачу")},
                    { new BotCommand("/show", "Показывает все активные задачи")}, { new BotCommand("/removetask", "Удаляет задачу")},
                    { new BotCommand("/completetask", "Завершает задачу")}, { new BotCommand("/report", "Выводит отчет по задачам")}, 
                    { new BotCommand("/find", "Ищет задачу") }
                };

        public List<BotCommand> CommandsBeforeRegistration { get; } = new List<BotCommand> {
                    { new BotCommand("/start", "Начинает работу с ботом") }, { new BotCommand("/help", "Показывает справку по командам") },
                    { new BotCommand("/info", "Показывает информацию по боту")  }
                };

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, 
            IToDoReportService toDoReportService, IEnumerable<IScenario> scenarios, IScenarioContextRepository contextRepository, IToDoListService toDoListService, Helper helper)
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
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));

            var scenario = await GetScenario(context.CurrentScenario);
            if (await scenario.HandleMessageAsync(_bot, context, update, ct) == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(update.Message.From.Id, ct);
            } else 
            {
               await _contextRepository.SetContext(update.Message.From.Id, context, ct);
            }
        }




        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
                if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
                if (update.Message.Text != null)
                {
                    OnHandleEventStarted?.Invoke(update.Message.Text, update.Message.From.Id);
                    await botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'", ParseMode.Markdown, cancellationToken: ct);
                    var command = update.Message.Text.Trim().ToLower();
                    var trimmedCommand = command.Split(' ', 2)[0];
                    var user = await _userService.GetUser(update.Message.From.Id, ct);
                    if (update.Message.Text != "/start" && user == null)
                    {
                        await botClient.SetMyCommands(CommandsBeforeRegistration, cancellationToken: ct);
                        await botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы. Нажмите /start для начала.", replyMarkup: _helper._keyboardBeforeRegistration, cancellationToken: ct);
                    }

                    // получение ScenarioContext через IScenarioContextRepository перед обработкой команд.
                    var context = await _contextRepository.GetContext(update.Message.From.Id, ct);
                    if (context is not null)
                    {
                        if (trimmedCommand == "/cancel")
                        {
                            await OnCancelCommand(botClient, update, ct);
                            OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
                            return;
                        }
                        //ЕСЛИ ScenarioContext найден, ТО вызвать метод ProcessScenario и завершить обработку
                        await ProcessScenario(context, update, ct);
                        OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
                        return;
                    }

                    if (CommandsAfterRegistration.Any(i => i.Command == trimmedCommand) && _isTaskCountLimitSet && _isTaskLengthLimitSet)
                    {
                        await ExecuteCommand(botClient, update, trimmedCommand, ct);
                    }
                    else if (!_isTaskCountLimitSet)
                    {
                        await SetTaskCountLimit(botClient, update, trimmedCommand, ct);
                    }
                    else if (!_isTaskLengthLimitSet)
                    {
                        await SetTaskLengthLimit(botClient, update, trimmedCommand, ct);
                    }
                    else
                    {
                        await botClient.SendMessage(update.Message.Chat, "Неизвестная команда.", cancellationToken: ct);
                    }
                    OnHandleEventCompleted?.Invoke(update.Message.Text, update.Message.From.Id);
                } else
                {
                    await botClient.SendMessage(update.Message.Chat, "Получил пустой текст.", cancellationToken: ct);
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
        }

        private async Task OnCancelCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user is null) throw new UserNotFoundException(update.Message.From.Id);
            await _contextRepository.ResetContext(user.TelegramUserId, ct);
            await botClient.SendMessage(update.Message.Chat, $"Текущий сценарий отменен", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
        }

        private async Task ExecuteCommand(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
            {
                await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
                switch (command)
                {
                    case "/start": await OnStartCommand(botClient, update, ct); break;
                    case "/help": await OnHelpCommand(botClient, update, ct); break;
                    case "/info": await OnInfoCommand(botClient, update, ct); break;
                    case "/addtask": await OnAddTaskCommand(update, ct); break;
                    case "/show": await OnShowCommand(botClient, update, ct); break;
                    case "/removetask": await OnRemoveTaskCommand(botClient, update, ct); break;
                    case "/completetask": await OnCompleteTaskCommand(botClient, update, ct); break;
                    case "/report": await OnReportCommand(botClient, update, ct); break; 
                    case "/find": await OnFindCommand(botClient, update, ct); break;
                    default: await botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды.", cancellationToken: ct); break;
                }
                if (command != "/addtask")
                {
                    await botClient.SendMessage(update.Message.Chat, "Выберите команду:", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
                }
            } else
            {
                await botClient.SetMyCommands(CommandsBeforeRegistration, cancellationToken: ct);
                var reply = _helper._keyboardBeforeRegistration;
                switch (command)
                {
                    case "/start": 
                        await OnStartCommand(botClient, update, ct); 
                        reply = _helper._keyboardAfterRegistration; 
                        await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct); 
                        break;
                    case "/help": await OnHelpCommand(botClient, update, ct); break;
                    case "/info": await OnInfoCommand(botClient, update, ct); break;
                    //case "/exit": await OnExitCommand(botClient, update, ct); break;
                    default: await botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды.", cancellationToken: ct); break;
                }
                await botClient.SendMessage(update.Message.Chat, "Выберите команду:", replyMarkup: reply, cancellationToken: ct);
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
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.Text is null) throw new ArgumentNullException(nameof(update.Message.Text));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            var task = update.Message.Text.Trim();
            string namePrefix = task.Remove(0, "/find".Length).Trim();
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var tasksList = await _toDoService.Find(user, namePrefix, ct);
                StringBuilder outputBuilder = new StringBuilder();
                if (tasksList.Count == 0 || !tasksList.Any(i => i.State == ToDoItemState.Active))
                    outputBuilder.AppendLine($"Нет задач, начинающихся на {namePrefix}");
                else
                {
                    outputBuilder.AppendLine($"Задачи, начинающиеся на {namePrefix}");
                    for (int i = 0; i < tasksList.Count; i++)
                        outputBuilder.AppendLine($"Имя задачи '{tasksList[i].Name}' - {tasksList[i].CreatedAt} - `{tasksList[i].Id}`");
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
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var result = await _toDoReportService.GetUserStats(user.UserId, ct);
                await botClient.SendMessage(update.Message.Chat,
                    $"Статистика по задачам на {result.generatedAt}. " +
                    $"Всего: {result.total}; Завершенных: {result.completed}; Активных: {result.active};", cancellationToken: ct);
            }
        }

        private async Task OnCompleteTaskCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.Text is null) throw new ArgumentNullException(nameof(update.Message.Text));
            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/completetask".Length).Trim();
            Guid taskId = new Guid(task);
            await _toDoService.MarkAsCompleted(taskId, ct);

            await botClient.SendMessage(update.Message.Chat, $"Статус задачи `{taskId}` изменен", ParseMode.Markdown, cancellationToken: ct);
        }

        private async Task OnShowAllTasksCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var tasksList = await _toDoService.GetAllByUserId(user.UserId, ct);
                StringBuilder outputBuilder = new StringBuilder();
                if (tasksList.Count == 0)
                    outputBuilder.AppendLine("Список задач пуст");
                else
                {
                    outputBuilder.AppendLine("Список всех задач:");
                    for (int i = 0; i < tasksList.Count; i++)
                        outputBuilder.AppendLine($"({tasksList[i].State}) Задача `{tasksList[i].Name}` - создана {tasksList[i].CreatedAt} - `{tasksList[i].Id}`");
                }
                await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(),ParseMode.Markdown, cancellationToken: ct);
            }
        }

        private async Task OnRemoveTaskCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.Text is null) throw new ArgumentNullException(nameof(update.Message.Text));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/removetask".Length).Trim();
            Guid taskId = new Guid(task);
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var items = await _toDoService.GetAllByUserId(user.UserId, ct);
                if (items.Where(i => i.Id == taskId && i.User.UserId == user.UserId).Count() > 0)
                {
                    await _toDoService.Delete(taskId, ct);
                    await botClient.SendMessage(update.Message.Chat, $"Задача `{taskId}` удалена", ParseMode.Markdown, cancellationToken: ct);
                }
            }
        }

        private async Task OnShowCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            //При получении команды / show нужно отправлять сообщение с текстом "Выберите список" и кнопками InlineKeyboardButton(см.Демонстрация работы бота)
            //Для этого нужно использовать класс InlineKeyboardMarkup и добавлять в него кнопки с помощью InlineKeyboardButton.WithCallbackData(string text, string callbackData)
            //Максимальный размер callbackData составляет 64 символа, поэтому в классах CallbackDto мы будем использовать компактный формат приведение к строкам
            //Для "📌Без списка" в callbackData пишем ToDoListCallbackDto.ToString().Action = "show", ToDoListId = null
            //Для остальных списков в callbackData пишем ToDoListCallbackDto.ToString().Action = "show", ToDoListId = Id
            //Для "🆕Добавить" в callbackData пишем "addlist".Для "❌Удалить" в callbackData пишем "deletelist"

            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));

            /*
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                var userId = user.UserId;
                var tasksList = await _toDoService.GetActiveByUserId(userId, ct);

                StringBuilder outputBuilder = new StringBuilder();

                if (tasksList.Count == 0 || !tasksList.Any(i => i.State == ToDoItemState.Active))
                    outputBuilder.AppendLine("Список текущих задач пуст");
                else
                {
                    outputBuilder.AppendLine("Список текущих задач:");
                    for (int i = 0; i < tasksList.Count; i++)
                        if (tasksList[i].State == ToDoItemState.Active)
                            outputBuilder.AppendLine($"Имя задачи `{tasksList[i].Name}` - {tasksList[i].CreatedAt} - `{tasksList[i].Id}`");
                }
                await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), ParseMode.Markdown, cancellationToken: ct);
            } */
        }

        private async Task OnAddTaskCommand(Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            //При получении команды / addtask создать ScenarioContext c ScenarioType.AddTask и вызвать метод ProcessScenario
            var scenarioContext = new ScenarioContext(ScenarioType.AddTask, update.Message.From.Id);
            await ProcessScenario(scenarioContext, update, ct);
        }

        private static Task OnExitCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }

        private async Task OnInfoCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            string newlineSymbol = Environment.NewLine;
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine($"{newlineSymbol}" +
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
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append(
                "Cправка по программе:" +
                $"{newlineSymbol}Команда /start: Регистрация нового пользователя в программе. После регистрации будут доступны основные команды." +
                $"{newlineSymbol}Команда /help: Отображает краткую справочную информацию о том, как пользоваться программой. Отображается описание доступных команд." +
                $"{newlineSymbol}Команда /info: Предоставляет информацию о версии программы и дате её создания." //+
                //"{newlineSymbol}Команда /exit: Завершить программу."
            );
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
                outputBuilder.AppendLine(
                    $"{newlineSymbol}Команда /cancel: Отменяет текущий сценарий." +
                    $"{newlineSymbol}Команда /report: Отображает краткую статистику по текущим задачам." +
                    $"{newlineSymbol}Команда /find: Отображает все задачи пользователя, которые начинаются на заданное слово. Например, команда /find Имя веведет все " +
                    $"команды, начинающиеся на Имя" +
                    $"{newlineSymbol}Команда /addtask: После ввода команды добавьте описание задачи. После добавления задачи выводится сообщение, что задача добавлена." +
                    $"{newlineSymbol}\tМаксимальная длина задачи:{(_toDoService.TaskLengthLimit == -1 ? "не задано" : _toDoService.TaskLengthLimit)}" +
                    $"{newlineSymbol}\tМаксимальное количество задач:{(_toDoService.TaskCountLimit == -1 ? "не задано" : _toDoService.TaskCountLimit)}" +
                    $"{newlineSymbol}Команда /showtasks: После ввода команды отображается список всех активных задач." +
                    $"{newlineSymbol}Команда /removetask: После ввода команды отображается список задач с номерами. Введите номер задачи для её удаления." +
                    $"{newlineSymbol}Команда /completetask: Используется для завершения задачи. При вводе этой команды с номером задачи " +
                    $"{newlineSymbol}(например, /completetask 0167b785-b830-4d02-b82a-881b0b678034), программа завершает задачу, её статус становится Completed."
            );

            await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
        }

        private async Task OnStartCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
            {
                await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
                await botClient.SendMessage(update.Message.Chat, $"Привет, {update.Message.From.Username}. Вы уже зарегистрированы.", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
            }
            else if (update.Message.From.Username != null)
            {
                    await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
                    await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
                    await botClient.SendMessage(update.Message.Chat, $"Привет, {update.Message.From.Username}", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
            }
            

            if (_toDoService.TaskLengthLimit == -1)
                _isTaskLengthLimitSet = false;
            if (_toDoService.TaskCountLimit == -1)
            {
                _isTaskCountLimitSet = false;
                await botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, " +
                    $"введи максимальное количество задач от {_toDoService.MinTaskCountLimit} до {_toDoService.MaxTaskCountLimit}", cancellationToken: ct);
            }    
        }

        /// <summary>
        /// Установка ограничения на максимальное количество задач в диапазоне.
        /// </summary>
        private async Task SetTaskCountLimit(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
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
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
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




