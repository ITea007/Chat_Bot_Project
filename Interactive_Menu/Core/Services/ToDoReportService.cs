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

        public ToDoReportService(IToDoService toDoService)
        { 
            _toDoService = toDoService;
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct)
        {
            var totalList = await _toDoService.GetAllByUserId(userId, ct);
            int total = totalList.Count;
            var activeList = await _toDoService.GetActiveByUserId(userId, ct);
            int active = activeList.Count;
            int completed = total - active;
            return (total, completed, active, DateTime.UtcNow);

        }
    }
}
