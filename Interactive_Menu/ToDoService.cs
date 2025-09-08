using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{
    /// <summary>
    /// Логика работы с задачами
    /// </summary>
    internal class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _tasks = new List<ToDoItem>();

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

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_tasks.Count == TaskCountLimit) throw new TaskCountLimitException(TaskCountLimit);
            if (name.Length > TaskLengthLimit) throw new TaskLengthLimitException(name.Length, TaskLengthLimit);
            if (_tasks.Where(i => i.Name == name).Count() > 0) throw new DuplicateTaskException(name);

            var task = new ToDoItem(user, name);
            _tasks.Add(task);
            return task;
        }

        public void Delete(Guid id)
        {
            _tasks.RemoveAll(i => i.Id == id);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active).ToList();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(i => i.User.UserId == userId).ToList();
        }

        public void MarkAsCompleted(Guid id)
        {
            var task = _tasks.FirstOrDefault(i => i.Id == id);
            if (task != null)
                task.State = ToDoItemState.Completed;
        }



    }
}
