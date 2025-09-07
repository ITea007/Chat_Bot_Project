using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{

    /// <summary>
    /// IToDoService отвечает за бизнес логику работы с задачами, которая не завязана на работу с телеграм
    /// </summary>
    internal interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        /// <summary>
        /// Возвращает ToDoItem для UserId со статусом Active
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns></returns>
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkAsCompleted(Guid id);
        void Delete(Guid id);

    }

}
