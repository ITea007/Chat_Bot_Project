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
        public int TaskCountLimit { get; set; } = -1;
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
        public int TaskLengthLimit { get; set; } = -1;
        /// <summary>
        /// Минимально допустимая длина строки описания задачи
        /// </summary>
        public int MinTaskLengthLimit { get; } = 1;
        /// <summary>
        /// Максимально допустимая длина строки описания задачи
        /// </summary>
        public int MaxTaskLengthLimit { get; } = 100;



        public ToDoService(IToDoRepository toDoRepository) {
            _toDoRepository = toDoRepository;
        }


        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_toDoRepository.GetAllByUserId(user.UserId).Count == TaskCountLimit) throw new TaskCountLimitException(TaskCountLimit);
            if (name.Length > TaskLengthLimit) throw new TaskLengthLimitException(name.Length, TaskLengthLimit);
            if (_toDoRepository.ExistsByName(user.UserId,name)) throw new DuplicateTaskException(name);

            var task = new ToDoItem(user, name);
            _toDoRepository.Add(task);
            return task;
        }

        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public async IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        public void MarkAsCompleted(Guid id)
        {
            var toDoItemById = _toDoRepository.Get(id);
            if (toDoItemById != null)
                _toDoRepository.Update(toDoItemById);
        }

        /// <summary>
        /// Метод возвращает все задачи пользователя, которые начинаются на namePrefix.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="namePrefix"></param>
        /// <returns></returns>
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId,item => item.Name.StartsWith(namePrefix));
        }
    }
}
