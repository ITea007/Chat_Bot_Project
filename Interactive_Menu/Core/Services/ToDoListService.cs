using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        //Размер имени списка не может быть больше 10 символом
        //Название списка должно быть уникально в рамках одного ToDoUser

        private readonly IToDoListRepository _toDoListRepository;

        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            // Проверка длины имени
            if (string.IsNullOrEmpty(name) || name.Length > 10)
                throw new ArgumentException("Название списка не может быть пустым или превышать 10 символов");

            // Проверка уникальности имени
            if (await _toDoListRepository.ExistsByName(user.UserId, name, ct))
                throw new InvalidOperationException("Список с таким именем уже существует");

            var list = new ToDoList(user, name);
            await _toDoListRepository.Add(list, ct);
            return list;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            return _toDoListRepository.Delete(id, ct);
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return _toDoListRepository.Get(id, ct);
        }

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return _toDoListRepository.GetByUserId(userId, ct);
        }
    }
}
