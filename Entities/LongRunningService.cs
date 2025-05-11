using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Entities
{
    public class LongRunningService : BackgroundService, ILongRunningService
    {
        public IBackgroundWorkerQueue Queue { get; }

        public LongRunningService(IBackgroundWorkerQueue queue)
        {
            this.Queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await Queue.DequeueAsync(stoppingToken);

                if (workItem == null)
                {
                    continue;
                }
                else
                {
                    await workItem(stoppingToken);
                }
            }
        }

        public async Task<string> CancelAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await Queue.DequeueAsync(stoppingToken);

                    if (workItem == null)
                    {
                        continue;
                    }
                    else
                    {
                        await workItem(stoppingToken);
                        return "Task Cancelled";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return string.Empty;
        }
    }
}
