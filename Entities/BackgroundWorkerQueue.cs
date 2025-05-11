using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Entities
{
    public class BackgroundWorkerQueue : IBackgroundWorkerQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(1, 5);
        public bool IsRunning { get; set; }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

        public bool Clear()
        {
            if (_workItems.Count > 0)
            {
                _workItems.Clear();
                return _workItems.Any() ? true : false;
            }

            return false;
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            try
            {
                _workItems.Enqueue(workItem);
                IsRunning = true;
            }
            catch (Exception)
            {
                IsRunning = false;
            }
            finally
            {
                _signal.Release();
            }
        }
    }
}
