using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    

    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        //В качестве хранилища использовать Dictionary<long, ScenarioContext>

        private readonly Dictionary<long, ScenarioContext> _scenarioContextDictionary = new Dictionary<long, ScenarioContext>();

        //Получить контекст пользователя
        //Добавить async await 
        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            ScenarioContext? context = _scenarioContextDictionary.GetValueOrDefault(userId);
            return Task.FromResult(context);
        }
        //Сбросить (очистить) контекст пользователя
        public Task ResetContext(long userId, CancellationToken ct)
        {
            _scenarioContextDictionary.Remove(userId);
            return Task.CompletedTask;
        }
        //Задать контекст пользователя
        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            ScenarioContext? value;
            if (_scenarioContextDictionary.TryGetValue(userId, out value))
            {
                if (!value.Equals(context))
                    _scenarioContextDictionary[userId] = context;
            }
            else
                _scenarioContextDictionary.Add(userId, context);
            return Task.CompletedTask;
        }
    }
}
