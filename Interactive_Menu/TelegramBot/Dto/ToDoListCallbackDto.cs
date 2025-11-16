using Interactive_Menu.Core.Entities;
using Interactive_Menu.TelegramBot.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Dto
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        //Помимо Action, есть ToDoListId Свойства: 
        public Guid? ToDoListId { get; set; }
        //На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...".
        //Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
        public static new ToDoListCallbackDto FromString(string input)
        {
            return new ToDoListCallbackDto();
        }
        //Переопределить метод.Он должен возвращать $"{base.ToString()}|{ToDoListId}"
        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
