using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    //В нем будем хранить все поддерживаемые сценарии
    enum ScenarioType
    {
        None,
        AddTask
    }

    //Класс, который будет хранить информацию о контексте(сессии) пользователя
    internal class ScenarioContext
    {
        public long UserId { get; set; } //Id пользователя в Telegram
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; } //Текущий шаг сценария
        public Dictionary<string, object> Data { get; set; } //Дополнительная инфрмация, необходимая для работы сценария
        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
        }
    }
}
