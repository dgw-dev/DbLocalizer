using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DbLocalizer.Tests
{
    class StartupTests : TestBase
    {
        [Test, Category("Startup")]
        public void ConfigureServices_DoesNotThrow()
        {
            // unit testing Startup.cs is a little silly, but it makes sonarcube happy!
            var startup = new Startup(ReadTestConfiguration());
            Assert.DoesNotThrow(() => startup.ConfigureServices(new Mock<IServiceCollection>().Object));
        }
    }
}
