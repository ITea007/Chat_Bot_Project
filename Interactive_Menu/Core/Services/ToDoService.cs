using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    /// <summary>
    /// Логика работы с задачами
    /// </summary>
    internal class ToDoService : IToDoService
    {
        
        private IToDoRepository _toDoRepository;

        /// <summary>
        /// Заданное максимальное количество задач
        /// </summary>
        public int TaskCountLimit { get; set; } = 100;
        /// <summary>
        /// Минимально допустимое количество задач
        /// </summary>
        public int MinTaskCountLimit { get; } = 1;
        /// <summary>
        /// Максимально допустимое количество задач
        /// </summary>
        public int MaxTaskCountLimit { get; } = 100;
        /// <summary>
        /// Заданная максимальная длина строки описания задачи
        /// </summary>
        public int TaskLengthLimit { get; set; } = 100;
        /// <summary>
        /// Минимально допустимая длина строки описания задачи
        /// </summary>
        public int MinTaskLengthLimit { get; } = 1;
        /// <summary>
        /// Максимально допустимая длина строки описания задачи
        /// </summary>
        public int MaxTaskLengthLimit { get; } = 100;

        public ToDoService(IToDoRepository toDoRepository) 
        {
            _toDoRepository = toDoRepository;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            return await _toDoRepository.GetActiveByUserId(userId, ct);
        }

        public Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            return _toDoRepository.Find(user.UserId, item => item.Name.StartsWith(namePrefix), ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var list =  await _toDoRepository.GetAllByUserId(userId, ct);
            return list;
        }

        public async Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, CancellationToken ct)
        {
            var listAll = await _toDoRepository.GetAllByUserId(user.UserId, ct);
            if (listAll.Count == TaskCountLimit) throw new TaskCountLimitException(TaskCountLimit);
            if (name.Length > TaskLengthLimit) throw new TaskLengthLimitException(name.Length, TaskLengthLimit);
            if (await _toDoRepository.ExistsByName(user.UserId, name, ct)) throw new DuplicateTaskException(name);

            var task = new ToDoItem(user, name, deadline);
            await _toDoRepository.Add(task, ct);
            return task;
        }

        public async Task MarkAsCompleted(Guid id, CancellationToken ct)
        {
            var toDoItemById = await _toDoRepository.Get(id, ct);
            if (toDoItemById != null)
                await _toDoRepository.Update(toDoItemById, ct);
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            return _toDoRepository.Delete(id,ct);
        }
    }
}
