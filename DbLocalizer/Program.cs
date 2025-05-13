using Entities.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace DbLocalizer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();
            await builder.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((cxt, services) =>
                {
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();
                    });

                    services.AddQuartzHostedService(opt =>
                    {
                        opt.WaitForJobsToComplete = true;
                    });
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.ClearProviders();

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddConsole();
                    }
                    builder.ClearProviders();
                    builder.AddProvider(new SQLiteLoggerProvider("Data Source=logs.db"));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateOnBuild = true;
                    options.ValidateScopes = true;
                })
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.AddJsonFile("databases.json", optional: true, reloadOnChange: true);
                    configuration.AddJsonFile("cultures.json", optional: true, reloadOnChange: true);
                    configuration.AddJsonFile("smartlingSettings.json", optional: true, reloadOnChange: true);

                    configuration.Build();
                });

    }
}
