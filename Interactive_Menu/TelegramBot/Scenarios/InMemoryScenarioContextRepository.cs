using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    

    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _scenarioContextDictionary = new ConcurrentDictionary<long, ScenarioContext>();

        //Получить контекст пользователя
        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            ScenarioContext? context = _scenarioContextDictionary.GetValueOrDefault(userId);
            return Task.FromResult(context);
        }

        //Получить все контексты
        public Task<IReadOnlyList<ScenarioContext>> GetContexts(CancellationToken ct)
        {
            var contexts = _scenarioContextDictionary.Values.ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<ScenarioContext>>(contexts);
        }

        //Сбросить (очистить) контекст пользователя
        public Task ResetContext(long userId, CancellationToken ct)
        {
            ScenarioContext? context;
            if (_scenarioContextDictionary.TryGetValue(userId, out context))
                _scenarioContextDictionary.TryRemove(KeyValuePair.Create(userId, context));
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
            {
                if (!_scenarioContextDictionary.TryAdd(userId, context))
                    throw new InvalidOperationException($"Unable to add ScenarioContext {context} to the {_scenarioContextDictionary}");
            }
            return Task.CompletedTask;
        }
    }
}
