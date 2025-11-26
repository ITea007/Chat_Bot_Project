using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.BackgroundTasks
{
    public interface IBackgroundTask
    {
        Task Start(CancellationToken ct);
    }
}
