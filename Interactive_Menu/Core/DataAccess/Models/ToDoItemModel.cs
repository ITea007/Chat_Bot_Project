using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.DataAccess.Models
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("State")]
        public int State { get; set; }

        [Column("StateChangedAt")]
        public DateTime? StateChangedAt { get; set; }

        [Column("Deadline")]
        public DateTime Deadline { get; set; }

        [Column("ListId")]
        public Guid? ListId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; } = null!;

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel? List { get; set; }
    }
}
