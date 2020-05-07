using NUnit.Framework;
using RadFramework.Abstractions.Configuration;

namespace RadFramework.Configuration.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var provider = new ConfigurationProvider("config.json");

            var testSection = provider.GetSection<TestSection>();
            
            Assert.AreEqual("Test", testSection.Value1);
        }
    }
}