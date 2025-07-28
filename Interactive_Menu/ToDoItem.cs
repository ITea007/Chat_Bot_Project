using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{
    internal class ToDoItem
    {
        public Guid Id { get; } //Заполняется в конструкторе. Guid.NewGuid()
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; }//Заполняется в конструкторе. DateTime.UtcNow
        public ToDoItemState State { get; set; }//Заполняется в конструкторе. ToDoItemState.Active
        public DateTime? StateChangedAt { get; set; }

        public ToDoItem(ToDoUser user, string name)
        {
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            Id = Guid.NewGuid();
        }
    }
}
