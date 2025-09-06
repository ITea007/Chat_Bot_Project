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
        IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);

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

        public int TaskCountLimit { get; set; }
        public int TaskLengthLimit { get; set; }
        public int MinTaskCountLimit { get; }
        public int MaxTaskCountLimit { get; }
        public int MinTaskLengthLimit { get; }
        public int MaxTaskLengthLimit { get; }
    }

}
