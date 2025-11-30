using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; init; }
        public Guid UserId { get; set; }
        public ToDoUser User { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public DateTime Deadline { get; set; }
        public Guid? ListId { get; set; }
        public ToDoList? List { get; init; }
    }
}
