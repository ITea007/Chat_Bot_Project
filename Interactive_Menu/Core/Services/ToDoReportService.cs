using Interactive_Menu.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        private IToDoService _toDoService;

        public ToDoReportService(IToDoService repository)
        { 
            _toDoService = repository;
        }

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            int total = _toDoService.GetAllByUserId(userId).Count;
            int active = _toDoService.GetActiveByUserId(userId).Count;
            int completed = total - active;
            return (total, completed, active, DateTime.UtcNow);

        }
    }
}
