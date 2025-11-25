using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        private readonly string _connectionString;

        public DataContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext(_connectionString);
        }
    }
}
