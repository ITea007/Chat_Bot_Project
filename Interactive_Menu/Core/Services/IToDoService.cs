using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{

    /// <summary>
    /// IToDoService отвечает за бизнес логику работы с задачами, которая не завязана на работу с телеграм
    /// </summary>
    internal interface IToDoService
    {
        //Метод возвращает все задачи пользователя, которые начинаются на namePrefix. Для этого нужно использовать метод IToDoRepository.Find
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct);

        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);
        /// <summary>
        /// Возвращает ToDoItem для UserId со статусом Active
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns></returns>
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);
        Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct);
        Task MarkAsCompleted(Guid id, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
        Task<ToDoItem?> Get(Guid toDoItemId, CancellationToken ct);

    }
}
