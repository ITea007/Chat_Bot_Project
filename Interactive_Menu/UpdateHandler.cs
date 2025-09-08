using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Interactive_Menu
{
    /// <summary>
    /// UpdateHandler отвечает за обработку обновлений.
    /// </summary>
    internal class UpdateHandler : IUpdateHandler
    {
        private IUserService _userService;
        private ToDoService _toDoService;
        private bool _isAllCommandsAvailable = false;
        private bool _isTaskCountLimitSet = true;
        private bool _isTaskLengthLimitSet = true;
        private Dictionary<int, string> _commands = new Dictionary<int, string> {
                    { 1, "/start" }, { 2, "/help" },
                    { 3, "/info" }, { 4, "/exit" }, { 5, "/addtask"},
                    { 6, "/showtasks"}, { 7, "/removetask"}, { 8, "/showalltasks"},
                    { 9, "/completetask"}
                };

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = (ToDoService)toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'");

                var command = update.Message.Text.Trim().ToLower(); // Получаем текст сообщения
                var trimmedCommand = command.Split(' ', 2)[0];

                if (_commands.ContainsValue(trimmedCommand) && _isTaskCountLimitSet && _isTaskLengthLimitSet)
                {
                    ExecuteCommand(botClient, update, trimmedCommand); // Переходим к выполнению соответствующей команды
                }
                else if (!_isTaskCountLimitSet)
                {
                    SetTaskCountLimit(botClient, update, trimmedCommand);
                } else if (!_isTaskLengthLimitSet)
                {
                    SetTaskLengthLimit(botClient, update, trimmedCommand);
                }

                else
                {
                    botClient.SendMessage(update.Message.Chat, "Неизвестная команда.");
                }
            }
            catch (Exception Ex)
            {
                botClient.SendMessage(update.Message.Chat, "Произошла непредвиденная ошибка:");
                botClient.SendMessage(update.Message.Chat, $"Тип исключения: {Ex.GetType()}");
                botClient.SendMessage(update.Message.Chat, $"Исключение: {Ex.Message}");
                botClient.SendMessage(update.Message.Chat, $"Трассировка стека: {Ex.StackTrace}");
                botClient.SendMessage(update.Message.Chat, $"Внутреннее исключение: {Ex.InnerException}");
            }
        }

        private void ExecuteCommand(ITelegramBotClient botClient, Update update, string command)
        {
            if (_isAllCommandsAvailable)
            {
                switch (command)
                {
                    case "/start": OnStartCommand(botClient, update); break;
                    case "/help": OnHelpCommand(botClient, update); break;
                    case "/info": OnInfoCommand(botClient, update); break;
                    case "/exit": OnExitCommand(botClient, update); break;
                    case "/addtask": OnAddTaskCommand(botClient, update); break;
                    case "/showtasks": OnShowTasksCommand(botClient, update); break;
                    case "/removetask": OnRemoveTaskCommand(botClient, update); break;
                    case "/showalltasks": OnShowAllTasksCommand(botClient, update); break;
                    case "/completetask": OnCompleteTaskCommand(botClient, update); break;
                    default: botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды."); break;
                }
            } else
            {
                switch (command)
                {
                    case "/start": OnStartCommand(botClient, update); break;
                    case "/help": OnHelpCommand(botClient, update); break;
                    case "/info": OnInfoCommand(botClient, update); break;
                    case "/exit": OnExitCommand(botClient, update); break;
                    default: botClient.SendMessage(update.Message.Chat, "Ошибка обработки команды."); break;
                }
            }
        }

        private void OnCompleteTaskCommand(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "OnCompleteTaskCommand.");

            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/completetask".Length).Trim();
            Guid taskId = new Guid(task);
            _toDoService.MarkAsCompleted(taskId);

            botClient.SendMessage(update.Message.Chat, $"Задача {taskId} выполнена");
        }

        private void OnShowAllTasksCommand(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "OnShowAllTasksCommand.");

            Guid userId = new Guid();

            if (_userService.GetUser(update.Message.From.Id) != null)
                userId = _userService.GetUser(update.Message.From.Id).UserId;
            
            var tasksList = _toDoService.GetAllByUserId(userId);
            
            StringBuilder outputBuilder = new StringBuilder();

            if (tasksList.Count == 0)
                outputBuilder.AppendLine("Список задач пуст");
            else
            {
                outputBuilder.AppendLine("Список всех задач:");
                for (int i = 0; i < tasksList.Count; i++)
                    outputBuilder.AppendLine($"({tasksList[i].State}) Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - {tasksList[i].Id}");
            }
            botClient.SendMessage(update.Message.Chat, outputBuilder.ToString());
            
        }

        private void OnRemoveTaskCommand(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "OnRemoveTaskCommand.");

            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/removetask".Length).Trim();
            Guid taskId = new Guid(task);
            _toDoService.Delete(taskId);

            botClient.SendMessage(update.Message.Chat, $"Задача {taskId} удалена");

        }

        private void OnShowTasksCommand(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "OnShowTasksCommand.");

            Guid userId = _userService.GetUser(update.Message.From.Id).UserId;
            var tasksList = _toDoService.GetActiveByUserId(userId);

            StringBuilder outputBuilder = new StringBuilder();

            if ((tasksList.Count == 0) || (!tasksList.Any(i => i.State == ToDoItemState.Active)))
                outputBuilder.AppendLine("Список текущих задач пуст");
            else
            {
                outputBuilder.AppendLine("Список текущих задач:");
                for (int i = 0; i < tasksList.Count; i++)
                    if (tasksList[i].State == ToDoItemState.Active)
                        outputBuilder.AppendLine($"Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - {tasksList[i].Id}");
            }
            botClient.SendMessage(update.Message.Chat, outputBuilder.ToString());
        }

        private void OnAddTaskCommand(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "OnAddTaskCommand.");
            var user = _userService.GetUser(update.Message.From.Id);
            var task = update.Message.Text.Trim();
            task = task.Remove(0, "/addtask".Length).Trim();
            if (user != null)
            {
                _toDoService.Add(user, task);
                botClient.SendMessage(update.Message.Chat, $"Добавлена задача {task}");
            }


        }

        private void OnExitCommand(ITelegramBotClient botClient, Update update)
        {
            Environment.Exit(0);
        }

        private void OnInfoCommand(ITelegramBotClient botClient, Update update)
        {
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("\r\n" +
                                "*  Текущая версия программы 5.0.  Дата создания 02-09-2025\r\n" +
                                "   Удалена команда /echo, реализовна эмуляция работы с ботом (ДЗ 6) \r\n" +
                                "*  Текущая версия программы 4.0.  Дата создания 28-07-2025\r\n" +
                                "   Добавлены команды /completetask, /showalltasks, изменена логика команды /showtasks(ДЗ 5) \r\n" +
                                "*  Текущая версия программы 3.0.  Дата создания 19-06-2025\r\n" +
                                "   Добавлена обработка ошибок через исключения(ДЗ 4) \r\n" +
                                "*  Текущая версия программы 2.0.  Дата создания 16-06-2025\r\n" +
                                "   Добавлены команды /addtask, /showtasks, /removetask\r\n" +
                                "*  Версия программы 1.0.  Дата создания 25-05-2025\r\n" +
                                "   Реализованы команды /start, /help, /info, /echo, /exit\r\n");

            botClient.SendMessage(update.Message.Chat, outputBuilder.ToString());
        }

        private void OnHelpCommand(ITelegramBotClient botClient, Update update)
        {
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("Cправка по программе:\r\nКоманда /start: Если пользователь вводит команду /start, программа просит его ввести своё имя." +
                "\r\nКоманда /help: Отображает краткую справочную информацию о том, как пользоваться программой." +
                "\r\nКоманда /info: Предоставляет информацию о версии программы и дате её создания." +
                "\r\nКоманда /addtask: После ввода команды добавьте описание задачи. После добавления задачи выводится сообщение, что задача добавлена." +
                $"\r\n\tМаксимальная длина задачи:{((_toDoService.TaskLengthLimit == -1) ? "не задано" : _toDoService.TaskLengthLimit)}" +
                $"\r\n\tМаксимальное количество задач:{((_toDoService.TaskCountLimit == -1) ? "не задано" : _toDoService.TaskCountLimit)}" +
                "\r\nКоманда /showtasks: После ввода команды отображается список всех активных задач." +
                "\r\nКоманда /showalltasks: После ввода команды отображается список всех задач." +
                "\r\nКоманда /removetask: После ввода команды отображается список задач с номерами. Введите номер задачи для её удаления." +
                "\r\nКоманда /completetask: Используется для завершения задачи. При вводе этой команды с номером задачи " +
                "\r\n(например, /completetask 0167b785-b830-4d02-b82a-881b0b678034), программа завершает задачу, её статус станосится Completed." +
                "\r\nКоманда /exit: Завершить программу." +
                "\r\nВводите команды строчными буквами для корректной работы приложения.\r\n");

            botClient.SendMessage(update.Message.Chat, outputBuilder.ToString());
        }

        private void OnStartCommand(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "OnStartCommand.");

            if (_userService.GetUser(update.Message.From.Id) != null)
            {
                _isAllCommandsAvailable = true;
                botClient.SendMessage(update.Message.Chat, $"Привет, {update.Message.From.Username}");
            }
            else
            {
                if (update.Message.From.Username != null)
                {
                    _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
                    botClient.SendMessage(update.Message.Chat, $"Привет, {update.Message.From.Username}");
                    _isAllCommandsAvailable = true;
                }
            }
            if (_toDoService.TaskCountLimit == -1)
                _isTaskCountLimitSet = false;
            if (_toDoService.TaskLengthLimit == -1)
                _isTaskLengthLimitSet = false;
            botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, введи максимальное количество задач от {_toDoService.MinTaskCountLimit} до {_toDoService.MaxTaskCountLimit}");
        }

        /// <summary>
        /// Установка ограничения на максимальное количество задач в диапазоне.
        /// </summary>
        private void SetTaskCountLimit(ITelegramBotClient botClient, Update update, string command)
        {
            _toDoService.TaskCountLimit = ParseAndValidateInt(command, _toDoService.MinTaskCountLimit, _toDoService.MaxTaskCountLimit);
            _isTaskCountLimitSet = true;
            botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, установлено максимальное количество задач: {_toDoService.TaskCountLimit}");

            botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, введи максимальную длину задачи от {_toDoService.MinTaskLengthLimit} до {_toDoService.MaxTaskLengthLimit}");
        }

        /// <summary>
        /// Установка ограничения на допустимую длину задачи.
        /// </summary>
        private void SetTaskLengthLimit(ITelegramBotClient botClient, Update update, string command)
        {
            _toDoService.TaskLengthLimit = ParseAndValidateInt(command, _toDoService.MinTaskLengthLimit, _toDoService.MaxTaskLengthLimit);
            _isTaskLengthLimitSet = true;
            botClient.SendMessage(update.Message.Chat, $"{update.Message.From.Username}, установлена максимальная длина задачи: {_toDoService.TaskLengthLimit}");

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
    }

}




