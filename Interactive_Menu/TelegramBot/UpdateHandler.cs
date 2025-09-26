using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
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

        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler? OnHandleEventStarted;
        public event MessageEventHandler? OnHandleEventCompleted;

        private bool _isTaskCountLimitSet { get; set; } = true;
        private bool _isTaskLengthLimitSet { get; set; } = true;
        public List<BotCommand> CommandsAfterRegistration { get; } = new List<BotCommand> {
                    { new BotCommand("/start", "Начинает работу с ботом") }, { new BotCommand("/help", "Показывает справку по командам") },
                    { new BotCommand("/info", "Показывает информацию по боту") }, { new BotCommand("/addtask", "Добавляет задачу")},
                    { new BotCommand("/showtasks", "Показывает все активные задачи")}, { new BotCommand("/removetask", "Удаляет задачу")}, 
                    { new BotCommand("/showalltasks", "Показывает все задачи")},
                    { new BotCommand("/completetask", "Завершает задачу")}, { new BotCommand("/report", "Выводит отчет по задачам")}, 
                    { new BotCommand("/find", "Ищет задачу") }
                };

        public List<BotCommand> CommandsBeforeRegistration { get; } = new List<BotCommand> {
                    { new BotCommand("/start", "Начинает работу с ботом") }, { new BotCommand("/help", "Показывает справку по командам") },
                    { new BotCommand("/info", "Показывает информацию по боту")  }
                };

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
        {
            _userService = userService;
            _toDoService = (ToDoService)toDoService;
            _toDoReportService = toDoReportService;
        }

        private ReplyKeyboardMarkup _keyboardBeforeRegistration = new ReplyKeyboardMarkup(
                    new KeyboardButton[] { "/start" })
        {
            ResizeKeyboard = true,
        };
        private ReplyKeyboardMarkup _keyboardAfterRegistration = new ReplyKeyboardMarkup(
            new KeyboardButton[] { "/showalltasks", "/showtasks", "/report" })
        {
            ResizeKeyboard = true,
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
                

                if (update.Message.Text != null)
                {
                    OnHandleEventStarted?.Invoke(update.Message.Text);
                    await botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'", cancellationToken: ct);
                    var isRegistered = await _userService.GetUser(update.Message.From.Id, ct);
                     if (update.Message.Text != "/start" && isRegistered == null)
                    {
                        await botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы. Нажмите /start для начала.", replyMarkup: _keyboardBeforeRegistration, cancellationToken: ct);
                    }

                    var command = update.Message.Text.Trim().ToLower(); // Получаем текст сообщения
                    var trimmedCommand = command.Split(' ', 2)[0];

                    if (CommandsAfterRegistration.Any(i => i.Command == trimmedCommand) && _isTaskCountLimitSet && _isTaskLengthLimitSet)
                    {
                        await ExecuteCommand(botClient, update, trimmedCommand, ct); // Переходим к выполнению соответствующей команды

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
                    OnHandleEventCompleted?.Invoke(update.Message.Text);
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

        private async Task ExecuteCommand(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
            {
                switch (command)
                {
                    case "/start": await OnStartCommand(botClient, update, ct); break;
                    case "/help": await OnHelpCommand(botClient, update, ct); break;
                    case "/info": await OnInfoCommand(botClient, update, ct); break;
                    //case "/exit": await OnExitCommand(botClient, update, ct); break;
                    case "/addtask": await OnAddTaskCommand(botClient, update, ct); break;
                    case "/showtasks": await OnShowTasksCommand(botClient, update, ct); break;
                    case "/removetask": await OnRemoveTaskCommand(botClient, update, ct); break;
                    case "/showalltasks": await OnShowAllTasksCommand(botClient, update, ct); break;
                    case "/completetask": await OnCompleteTaskCommand(botClient, update, ct); break;
                    case "/report": await OnReportCommand(botClient, update, ct); break; 
                    case "/find": await OnFindCommand(botClient, update, ct); break;
                    default: await botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды.", cancellationToken: ct); break;
                }
            } else
            {
                switch (command)
                {
                    case "/start": await OnStartCommand(botClient, update, ct); break;
                    case "/help": await OnHelpCommand(botClient, update, ct); break;
                    case "/info": await OnInfoCommand(botClient, update, ct); break;
                    //case "/exit": await OnExitCommand(botClient, update, ct); break;
                    default: await botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды.", cancellationToken: ct); break;
                }
            }
            await botClient.SendMessage(update.Message.Chat, "Выберите команду:", replyMarkup: _keyboardAfterRegistration, cancellationToken: ct);
        }

        /// <summary>
        /// Метод возвращает все задачи пользователя, которые начинаются на namePrefix. Обработку команды /find. 
        ///Пример команды: /find Имя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        private async Task OnFindCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.Text is null || update.Message.From is null) throw new ArgumentNullException();
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
                        outputBuilder.AppendLine($"Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - `{tasksList[i].Id}`");
                }
                await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
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
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
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
            if (update.Message is null || update.Message.Text is null) throw new ArgumentNullException();
            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/completetask".Length).Trim();
            Guid taskId = new Guid(task);
            await _toDoService.MarkAsCompleted(taskId, ct);

            await botClient.SendMessage(update.Message.Chat, $"Статус задачи `{taskId}` изменен", cancellationToken: ct);
        }

        private async Task OnShowAllTasksCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
            Guid userId = new Guid();
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            if (user != null)
            {
                userId = user.UserId;
                var tasksList = await _toDoService.GetAllByUserId(userId, ct);
                StringBuilder outputBuilder = new StringBuilder();
                if (tasksList.Count == 0)
                    outputBuilder.AppendLine("Список задач пуст");
                else
                {
                    outputBuilder.AppendLine("Список всех задач:");
                    for (int i = 0; i < tasksList.Count; i++)
                        outputBuilder.AppendLine($"({tasksList[i].State}) Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - `{tasksList[i].Id}`");
                }
                await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
            }
        }

        private async Task OnRemoveTaskCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.Text is null) throw new ArgumentNullException();
            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/removetask".Length).Trim();
            Guid taskId = new Guid(task);
            await _toDoService.Delete(taskId, ct);
            await botClient.SendMessage(update.Message.Chat, $"Задача `{taskId}` удалена", cancellationToken: ct);

        }

        private async Task OnShowTasksCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
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
                            outputBuilder.AppendLine($"Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - `{tasksList[i].Id}`");
                }
                await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
            }
        }

        private async Task OnAddTaskCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.Text is null || update.Message.From is null) throw new ArgumentNullException();
            var user = await _userService.GetUser(update.Message.From.Id, ct);
            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/addtask".Length).Trim();
            if (user != null)
            {
                await _toDoService.Add(user, task, ct);
                await botClient.SendMessage(update.Message.Chat, $"Добавлена задача {task}", cancellationToken: ct);
            }
        }

        private static Task OnExitCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }

        private async Task OnInfoCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException();
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("\r\n" +
                                "*  Текущая версия программы 8.0.  Дата создания 22-09-2025\r\n" +
                                "   Реализована работа телеграмм бота (ДЗ 9) \r\n" +
                                "*  Текущая версия программы 7.0.  Дата создания 16-09-2025\r\n" +
                                "   Реализовано асинхронное выполнение (ДЗ 8) \r\n" +
                                "*  Текущая версия программы 6.0.  Дата создания 06-09-2025\r\n" +
                                "   Реализована команда /report, /find (ДЗ 7) \r\n" +
                                "*  Текущая версия программы 5.0.  Дата создания 02-09-2025\r\n" +
                                "   Удалена команда /echo, реализовна эмуляция работы с ботом (ДЗ 6) \r\n" +
                                "*  Текущая версия программы 4.0.  Дата создания 28-07-2025\r\n" +
                                "   Добавлены команды /completetask, /showalltasks, изменена логика команды /showtasks(ДЗ 5) \r\n" +
                                "*  Текущая версия программы 3.0.  Дата создания 19-06-2025\r\n" +
                                "   Добавлена обработка ошибок через исключения(ДЗ 4) \r\n" +
                                "*  Текущая версия программы 2.0.  Дата создания 16-06-2025\r\n" +
                                "   Добавлены команды /addtask, /showtasks, /removetask\r\n" +
                                "*  Версия программы 1.0.  Дата создания 25-05-2025\r\n" +
                                "   Реализованы команды /start, /help, /info, /echo, /exit");

            await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
        }

        private async Task OnHelpCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append(
                "Cправка по программе:" +
                "\r\nКоманда /start: Регистрация нового пользователя в программе. После регистрации будут доступны основные команды." +
                "\r\nКоманда /help: Отображает краткую справочную информацию о том, как пользоваться программой. Отображается описание доступных команд." +
                "\r\nКоманда /info: Предоставляет информацию о версии программы и дате её создания." //+
                //"\r\nКоманда /exit: Завершить программу."
            );
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
                outputBuilder.AppendLine(
                    "\r\nКоманда /report: Отображает краткую статистику по текущим задачам." +
                    "\r\nКоманда /find: Отображает все задачи пользователя, которые начинаются на заданное слово. Например, команда /find Имя веведет все " +
                    "команды, начинающиеся на Имя" +
                    "\r\nКоманда /addtask: После ввода команды добавьте описание задачи. После добавления задачи выводится сообщение, что задача добавлена." +
                    $"\r\n\tМаксимальная длина задачи:{(_toDoService.TaskLengthLimit == -1 ? "не задано" : _toDoService.TaskLengthLimit)}" +
                    $"\r\n\tМаксимальное количество задач:{(_toDoService.TaskCountLimit == -1 ? "не задано" : _toDoService.TaskCountLimit)}" +
                    "\r\nКоманда /showtasks: После ввода команды отображается список всех активных задач." +
                    "\r\nКоманда /showalltasks: После ввода команды отображается список всех задач." +
                    "\r\nКоманда /removetask: После ввода команды отображается список задач с номерами. Введите номер задачи для её удаления." +
                    "\r\nКоманда /completetask: Используется для завершения задачи. При вводе этой команды с номером задачи " +
                    "\r\n(например, /completetask 0167b785-b830-4d02-b82a-881b0b678034), программа завершает задачу, её статус становится Completed."
            );

            await botClient.SendMessage(update.Message.Chat, outputBuilder.ToString(), cancellationToken: ct);
        }

        private async Task OnStartCommand(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
            if (await _userService.GetUser(update.Message.From.Id, ct) != null)
            {
                await botClient.SendMessage(update.Message.Chat, $"Привет, {update.Message.From.Username}. Вы уже зарегистрированы.", cancellationToken: ct);
            }
            else
            {
                if (update.Message.From.Username != null)
                {
                    await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
                    await botClient.SendMessage(update.Message.Chat, $"Привет, {update.Message.From.Username}", cancellationToken: ct);
                    await botClient.SetMyCommands(CommandsAfterRegistration, cancellationToken: ct);
                }
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
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
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
            if (update.Message is null || update.Message.From is null) throw new ArgumentNullException();
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




