using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    //В качестве хранилища использовать Dictionary<long, ScenarioContext>

    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        //Получить контекст пользователя
        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        //Сбросить (очистить) контекст пользователя
        public Task ResetContext(long userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        //Задать контекст пользователя
        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
