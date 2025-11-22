using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Dto
{
    internal class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public static new PagedListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new PagedListCallbackDto { Action = string.Empty };

            var parts = input.Split('|');
            var dto = new PagedListCallbackDto { Action = parts[0] };

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid listId))
            {
                dto.ToDoListId = listId;
            }

            if (parts.Length > 2 && int.TryParse(parts[2], out int page))
            {
                dto.Page = page;
            }

            return dto;
        }

        public override string ToString()
        {
            var baseString = base.ToString();
            return ToDoListId.HasValue ? $"{baseString}|{Page}" : $"{Action}||{Page}";
        }
    }
}
