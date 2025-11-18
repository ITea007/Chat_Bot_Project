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
        AddTask,
        AddList,
        DeleteList
    }
}
