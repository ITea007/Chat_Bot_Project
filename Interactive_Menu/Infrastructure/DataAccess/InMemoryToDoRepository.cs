using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {

        private readonly List<ToDoItem> _tasks = new List<ToDoItem>();

        public void Add(ToDoItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            _tasks.Add(item);
        }

        public int CountActive(Guid userId)
        {
            return GetActiveByUserId(userId).Count;
        }

        public void Delete(Guid id)
        {
            _tasks.RemoveAll(i => i.Id == id);
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _tasks.Where(i => i.Name == name).Count() > 0 ? true : false;
        }

        /// <summary>
        /// Метод возвращает все задачи пользователя, которые удовлетворяют предикату.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return GetAllByUserId(userId).Where(predicate).ToList();
        }

        public ToDoItem? Get(Guid id)
        {
            return _tasks.FirstOrDefault(i => i.Id == id);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active).ToList();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(i => i.User.UserId == userId).ToList();
        }

        public void Update(ToDoItem item)
        {
            var task = _tasks.Find(i => i.Equals(item));
            
            if (task != null)
                task.State = (task.State == ToDoItemState.Active) ? ToDoItemState.Completed : ToDoItemState.Active;
        }
    }
}
