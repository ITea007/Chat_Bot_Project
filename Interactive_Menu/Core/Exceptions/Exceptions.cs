using Interactive_Menu.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Interactive_Menu.Core.Exceptions
{
    internal class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int taskCountLimit) : base($"Превышено максимальное количество задач равное {taskCountLimit}")
        {
        }
    }
    internal class ScenarioNotFoundException : Exception
    {
        public ScenarioNotFoundException(ScenarioType scenario) : base($"Сценарий {scenario} не найден")
        {
        }
    }
    internal class UserNotFoundException : Exception
    {
        public UserNotFoundException(long id) : base($"Пользователь с id:{id} не найден")
        {
        }
    }

    internal class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit) : base($"Длина задачи '{taskLength}' превышает максимально допустимое значение {taskLengthLimit}")
        {
        }

    }

    internal class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task) : base($"Задача ‘{task}’ уже существует")
        {
        }
    }

}
