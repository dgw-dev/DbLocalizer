using Quartz;
using System.Net.Http;
using System.Threading.Tasks;

namespace Entities
{
    public class ExportScheduler : IJob
    {

        private HttpResponseMessage response;

        public async Task Execute(IJobExecutionContext context)
        {
            HttpClient client = new HttpClient();

            JobDataMap dataMap = context.MergedJobDataMap;
            string exportEndpoint = dataMap.GetString("exportEndpoint");

            response = await client.PostAsync(exportEndpoint, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
