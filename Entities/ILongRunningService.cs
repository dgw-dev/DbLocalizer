using System.Threading;
using System.Threading.Tasks;

namespace Entities
{
    public interface ILongRunningService
    {
        IBackgroundWorkerQueue Queue { get; }

        Task<string> CancelAsync(CancellationToken stoppingToken);
    }
}