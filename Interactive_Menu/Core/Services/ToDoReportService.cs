using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
