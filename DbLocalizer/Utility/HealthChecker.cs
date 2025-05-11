using Entities;
using Entities.Plugins.TranslationManagement.Smartling;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DbLocalizer.Utility
{
    public class HealthChecker : IHealthCheck
    {
        private IWebHostEnvironment _env;
        protected readonly IConfiguration _configuration;
        private readonly ISmartlingDataService _fileDataService;

        public HealthChecker(IWebHostEnvironment env, IConfiguration configuration, AppSettings appSettings, ISmartlingDataService fileDataService)
        {
            _env = env;
            _configuration = configuration;
            _fileDataService = fileDataService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                ISmartlingConfiguration _tmsConfiguration = new SmartlingConfiguration(_configuration);
                SmartlingAuthResponse smartlingResponse = await _fileDataService.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(_tmsConfiguration.Authorization);

                if (smartlingResponse != null && smartlingResponse.IsSuccessStatusCode)
                {
                    data.Add("Smartling", "Can Connect");
                }
                else if (smartlingResponse != null)
                {
                    data.Add("Smartling", smartlingResponse.StatusCode + " " + smartlingResponse.ReasonPhrase);
                }
                else
                {
                    data.Add("Smartling", "Could not reach end point");
                }

                data.Add("Environment", _env.EnvironmentName);
                return HealthCheckResult.Healthy("Service is running and healthy.", data);
            }
            catch (Exception e)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "Health check failed with " + e.Message);
            }
        }
    }
}
