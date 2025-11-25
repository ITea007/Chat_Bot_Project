using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.DataAccess.Models
{
    [Table("ToDoUser")]
    internal class ToDoUserModel
    {
        [PrimaryKey]
        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("TelegramUserId")]
        public long TelegramUserId { get; set; }

        [Column("TelegramUserName")]
        public string TelegramUserName { get; set; } = string.Empty;

        [Column("RegisteredAt")]
        public DateTime RegisteredAt { get; set; }
    }
}
