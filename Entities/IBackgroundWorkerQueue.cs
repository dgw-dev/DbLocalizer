using System;
using System.Threading;
using System.Threading.Tasks;

namespace Entities
{
    public interface IBackgroundWorkerQueue
    {
        bool IsRunning { get; set; }

        bool Clear();
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
    }
}