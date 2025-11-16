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
            if (string.IsNullOrEmpty(input))
                return new ToDoListCallbackDto { Action = string.Empty };

            var parts = input.Split('|');
            var dto = new ToDoListCallbackDto { Action = parts[0] };

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid listId))
            {
                dto.ToDoListId = listId;
            }

            return dto;
        }

        //возвращает $"{base.ToString()}|{ToDoListId}"
        public override string ToString()
        {
            return ToDoListId.HasValue ? $"{base.ToString()}|{ToDoListId}" : base.ToString();
        }
    }
}
