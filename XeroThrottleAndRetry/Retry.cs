using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XeroThrottleAndRetry.Helpers;

namespace XeroThrottleAndRetry
{
    /// <summary>
    /// Provides Retry on error functionality with minimal impact on the calling code.
    /// 
    /// How to use:
    /// await Retry.DoAsync(() => this.MyMethod());
    /// or 
    /// var result = await Retry.DoAsync<int>(() => this.MyMethod());
    /// </summary>
    public static class Retry
    {
        private const int RetryIntervalMilliseconds = 1000; // By default wait 4 second, then 9 seconds before attempting again...
        private const int MaximumAttempts = 3;

        /// <summary>Attempts to execute the provided delegate Task 3 times and returns the specified value.</summary>
        public static async Task<T> DoAsync<T>(Func<Task<T>> func, int retryIntervalMilliseconds, int maximumAttempts)
        {
            var exceptions = new List<Exception>();

            for (int attempts = 1; attempts <= maximumAttempts; attempts++)
            {
                try
                {
                    if (attempts > 1)
                    {
                        await Task.Delay(retryIntervalMilliseconds * ((attempts + 1) * (attempts + 1)));
                    }

                    return await func();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }


        /// <summary>Attempts to execute the provided delegate Task 3 times and returns the specified value.</summary>
        public static async Task<T> DoAsync<T>(Func<Task<T>> func)
        {
            return await DoAsync(func, RetryIntervalMilliseconds, MaximumAttempts);
        }

        /// <summary>Attempts to execute the provided delegate Task 3 times.</summary>
        public static async Task DoAsync(Func<Task> func)
        {
            Task<object> teeFunc() => TaskWrapper.WrapTaskInGenericObject(func);

            await DoAsync<object>(teeFunc);
        }

        /// <summary>Attempts to execute the provided delegate Task 3 times.</summary>
        public static async Task DoAsync(Func<Task> func, int retryIntervalMilliseconds, int maximumAttempts)
        {
            Task<object> teeFunc() => TaskWrapper.WrapTaskInGenericObject(func);

            await DoAsync<object>(teeFunc, retryIntervalMilliseconds, maximumAttempts);
        }

    }
}
