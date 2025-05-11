using DbLocalizer.Utility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DbLocalizer.Tests
{
    class HealthCheckerTests : TestBase
    {
        HealthChecker Checker => new HealthChecker(_mockIWebHostEnvironment.Object, ReadTestConfiguration(), _appSettings, _smartlingDataService.Object);

        [Test]
        public void CheckHealthAsync_DoesNotThrow()
        {
            Assert.DoesNotThrowAsync(() => Checker.CheckHealthAsync(new HealthCheckContext(), new System.Threading.CancellationToken()));
        }

        [Test]
        public async Task CheckHealthAsync_ReturnsHealthCheckResultOnException()
        {
            var ctx = new HealthCheckContext();
            ctx.Registration = new HealthCheckRegistration("", new Mock<IHealthCheck>().Object, HealthStatus.Unhealthy, []);

            var result = await Checker.CheckHealthAsync(ctx, new System.Threading.CancellationToken());
            Assert.IsInstanceOf<HealthCheckResult>(result);
        }
    }
}
