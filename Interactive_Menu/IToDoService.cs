using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{

    //ToDoService отвечает за бизнес логику приложения, которая не завязана на работу с телеграм


    public interface IToDoService
    {
        //IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        //Возвращает ToDoItem для UserId со статусом Active
        //IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
       // ToDoItem Add(ToDoUser user, string name);
        void MarkAsCompleted(Guid id);
        void Delete(Guid id);
    }

}
