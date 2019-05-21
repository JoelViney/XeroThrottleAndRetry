using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace XeroThrottleAndRetry
{
    [TestClass]
    public class ThrottleAndRetryTests
    {
        // We don't need to test Throttle or Retry as they were tested in their own test cases.
        // so we just need to assure that the two methods were combined correctly.
        [TestMethod]
        public async Task ExecuteWasCalledTest()
        {
            // Arrange
            var executeCalled = false;

            // Act
            await ThrottleAndRetry.DoAsync(async () =>
            {
                executeCalled = true;
                await Task.FromResult<object>(null);
            }
            );

            // Assert
            Assert.IsTrue(executeCalled);
        }
    }
}
