using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface IImportDal
    {
        Task<int> ImportData(string connectionString, string sql);
    }
}
