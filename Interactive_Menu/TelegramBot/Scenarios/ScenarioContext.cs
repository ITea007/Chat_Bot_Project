using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    //для получения результата выполнения сценария
    enum ScenarioResult
    {
        Transition, // - Переход к следующему шагу. Сообщение обработано, но сценарий еще не завершен
        Completed   // - Сценарий завершен
    }


    //В нем будем хранить все поддерживаемые сценарии
    enum ScenarioType
    {
        None,
        AddTask
    }

    //Класс, который будет хранить информацию о контексте(сессии) пользователя
    internal class ScenarioContext
    {
        public long UserId { get; init; } //Id пользователя в Telegram
        public ScenarioType CurrentScenario { get; init; }
        public string? CurrentStep { get; set; } //Текущий шаг сценария
        public Dictionary<string, object> Data { get; set; } //Дополнительная инфрмация, необходимая для работы сценария
        public ScenarioContext(ScenarioType scenario, long userId)
        {
            CurrentScenario = scenario;
            Data = new Dictionary<string, object>();
            UserId = userId;
        }
    }
}
