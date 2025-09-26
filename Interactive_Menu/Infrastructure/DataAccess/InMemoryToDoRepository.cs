using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {

        private readonly List<ToDoItem> _tasks = new List<ToDoItem>();

        public Task Add(ToDoItem item, CancellationToken ct)
        {
            if (item == null) throw new ArgumentNullException("item");
            _tasks.Add(item);
            return Task.CompletedTask;
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            var activeItems = await GetActiveByUserId(userId, ct);
            return activeItems.Count;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            return Task.FromResult(_tasks.RemoveAll(i => i.Id == id));
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            return Task.FromResult(_tasks.Where(i => i.Name == name && i.User.UserId == userId).Count() > 0 ? true : false);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var allItems = await GetAllByUserId(userId, ct);
            return allItems.Where(predicate).ToList();
        }

        public Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            var toDoItem = _tasks.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(toDoItem);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var list = _tasks.Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(list);
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var list = _tasks.Where(i => i.User.UserId == userId).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<ToDoItem>>(list);
        }

        public Task Update(ToDoItem item, CancellationToken ct)
        {
            var task = _tasks.Find(i => i.Equals(item));

            if (task != null)
                task.State = (task.State == ToDoItemState.Active) ? ToDoItemState.Completed : ToDoItemState.Active;
            return Task.CompletedTask;
        }
    }
}
