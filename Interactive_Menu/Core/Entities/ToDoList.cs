using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Entities
{
    internal class ToDoList
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public ToDoUser User { get; set; } = null!;
        public DateTime CreatedAt { get; init; }

    }
}
