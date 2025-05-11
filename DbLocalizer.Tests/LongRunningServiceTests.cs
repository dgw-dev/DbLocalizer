using Entities;
using NUnit.Framework;

namespace DbLocalizer.Tests
{
    class LongRunningServiceTests : TestBase
    {
        LongRunningService Service => new LongRunningService(_backgroundJobQueue.Object);

        [Test]
        public void Queue_IsAccessible()
        {
            Assert.IsNotNull(Service.Queue);
        }
    }
}
