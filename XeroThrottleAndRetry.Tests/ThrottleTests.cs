using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using XeroThrottleAndRetry.Helper;

namespace XeroThrottleAndRetry
{
    // We need to assure that these calls are throttled and it takes over a second to run two in a row.
    [TestClass]
    public class ThrottleTests
    {
        [TestMethod]
        public async Task ExecuteWasCalledTest()
        {
            // Arrange
            var executeCalled = false;

            // Act
            await Throttle.DoAsync(async () =>
            {
                executeCalled = true;
                await Task.FromResult<object>(null);
            });

            // Assert
            Assert.IsTrue(executeCalled);
        }

        [TestMethod]
        public async Task ResultReturnedTest()
        {
            // Arrange
            
            // Act
            var result = await Throttle.DoAsync<int>(async () =>
            {
                return await Task.FromResult<int>(123);
            });

            // Assert
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public async Task ThrottlingLimit10CallsPerSecondTest()
        {
            // Arrange
            var tickStart = DateTime.UtcNow.Ticks;

            async Task TestExecute() // Local function thats called by this test.
            {
                await Task.FromResult<object>(null);
            }

            // We need to use an instanced version of it so we can test in isolation.
            var throttle = new ThrottleInstance(2, 1);
            // Act
            await throttle.DoAsync(() => TestExecute());
            await throttle.DoAsync(() => TestExecute());
            await throttle.DoAsync(() => TestExecute());

            // Assert
            long tickEnd = DateTime.UtcNow.Ticks;
            TimeSpan timeSpan = TimeSpan.FromTicks(tickEnd - tickStart);

            Assert.IsTrue(timeSpan.Duration().TotalSeconds >= 1);
        }

    }
}
