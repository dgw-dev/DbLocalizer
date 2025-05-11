using Entities.Plugins.TranslationManagement.Smartling;
using NUnit.Framework;

namespace DbLocalizer.Tests.Smartling
{
    class SmartlingLocaleIdTests : TestBase
    {
        [Test]
        public void Constructor_DoesNotThrow()
        {
            ISmartlingConfiguration smartlingConfiguration = new SmartlingConfiguration(_configuration);
            var localeId = new SmartlingLocaleId(smartlingConfiguration);
            Assert.AreEqual(smartlingConfiguration.WorkflowUid, localeId.workflowUid);
        }
    }
}
