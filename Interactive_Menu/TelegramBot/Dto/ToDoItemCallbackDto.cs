using Interactive_Menu.TelegramBot.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Dto
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoItemCallbackDto { Action = string.Empty };

            var parts = input.Split('|');
            var dto = new ToDoItemCallbackDto { Action = parts[0] };

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid itemId))
            {
                dto.ToDoItemId = itemId;
            }

            return dto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }
    }
}
