using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace XeroThrottleAndRetry
{
    // Test that the retry method actually attempts to retry on failure.
    [TestClass]
    public class RetryAsyncTests
    {
        private const int MaximumAttempts = 3;
        private const int RetryIntervalMilliseconds = 10;

        [TestMethod]
        public async Task ExecuteWasCalledTest()
        {
            // Arrange
            var executeCalled = false;

            // Act
            await Retry.DoAsync(async () =>
            {
                executeCalled = true;
                await Task.FromResult<object>(null);
            }
            );

            // Assert
            Assert.IsTrue(executeCalled);
        }

        [TestMethod]
        public async Task ResultWasReturnedTest()
        {
            // Arrange

            // Act
            var result = await Retry.DoAsync<int>(async () =>
            {
                return await Task.FromResult<int>(123);
            });

            // Assert
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public async Task FailOnceButCompletedTest()
        {
            // Arrange
            var exceptionCount = 0;
            
            // Act
            await Retry.DoAsync(async () => {
                if (exceptionCount == 0)
                {
                    exceptionCount++;
                    throw new Exception("I fell over and can't get up.");
                }
                await Task.FromResult<object>(null);
            }, RetryIntervalMilliseconds, MaximumAttempts);

            // Assert
            Assert.AreEqual(1, exceptionCount);
        }

        [TestMethod]
        [ExpectedException(typeof(OutOfMemoryException))]
        public async Task FailedWithSameExceptionsReturnsNonAggregateExceptionTest()
        {
            // Arrange
            Task ExecuteAndFailAsync()
            {
                throw new OutOfMemoryException("I fell over and can't get up.");
            }

            // Act
            await Retry.DoAsync(() => ExecuteAndFailAsync(), RetryIntervalMilliseconds, MaximumAttempts);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public async Task ExceptionsAggregatedTest()
        {
            // Arrange
            var attempts = 0;

            // Act
            await Retry.DoAsync(() =>
            {
                attempts++;

                if (attempts == 1)
                {
                    throw new Exception("I fell down and I cant get up.");
                }
                else
                {
                    throw new Exception("He fell down and he cant get up.");
                }
            }, RetryIntervalMilliseconds, MaximumAttempts);

            // Assert
        }


        [TestMethod]
        [ExpectedException(typeof(StackOverflowException))]
        public async Task OnlySendOneIdenticalExceptionTest()
        {
            // Arrange
            var attempts = 0;

            // Act
            await Retry.DoAsync(() =>
            {
                attempts++;

                throw new StackOverflowException("I fell down and I cant get up.");
            }, RetryIntervalMilliseconds, MaximumAttempts);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(OutOfMemoryException))]
        public async Task Failed3TimesWithSameExceptionsReturnsSingleExceptionTest()
        {
            // Arrange
            Task ExecuteAndFailAsync()
            {
                throw new OutOfMemoryException("I fell over and can't get up.");
            }

            // Act
            await Retry.DoAsync(() => ExecuteAndFailAsync(), RetryIntervalMilliseconds, MaximumAttempts);
        }

        [TestMethod]
        public async Task FailAndThrewAggregateExceptionTest()
        {
            // Arrange
            var exceptionCount = 0;

            Task ExecuteAndFailAsync()
            {
                if (exceptionCount == 0)
                {
                    exceptionCount++;
                    throw new OutOfMemoryException("I fell over and can't umm, wossit... You know.");
                }
                throw new IndexOutOfRangeException("I fell over there and can't get up.");
            }

            // Act
            try
            {
                await Retry.DoAsync(() => ExecuteAndFailAsync(), RetryIntervalMilliseconds, MaximumAttempts);
            }
            catch (AggregateException ex)
            {
                // Assert
                Assert.AreEqual(3, ex.InnerExceptions.Count);
            }
        }
    }
}
