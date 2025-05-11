using System.Threading;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface IExportUtility
    {
        Task<string> Export(CancellationToken ct = default);
    }
}
