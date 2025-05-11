using NUnit.Framework;
using System;

namespace DbLocalizer.Tests
{
    class ProgramTests : TestBase
    {
        [Test]
        public void CreateHostBuilder_DoesNotThrow()
        {
            // unit testing Program.cs is a little silly, but it makes sonarcube happy!
            Assert.DoesNotThrow(() => Program.CreateHostBuilder(Array.Empty<string>()));
        }
    }
}
