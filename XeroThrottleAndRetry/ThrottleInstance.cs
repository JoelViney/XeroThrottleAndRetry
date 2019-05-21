using ThrottlerAndRetrier.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlerAndRetrier
{
    /// <summary>
    /// Throttles the calls that pass through this helper to 1 call a second.
    /// 
    /// There are limits to the number of API calls that your application can make against a particular Xero organisation.
    ///     * Minute Limit: 60 calls in a rolling 60 second window
    ///     * Daily Limit: 5000 calls in a rolling 24 hour window (200 calls an hour, 3.4xx calls a minute)...
    ///     
    /// If you exceed either rate limit you will receive a HTTP 503 (Service Unavailable) response with the following message in the http response body:
    /// 
    /// https://developer.xero.com/documentation/auth-and-limits/xero-api-limits
    /// </summary>
    public class ThrottleInstance
    {
        private readonly int _callLimit;        // 60 calls in...
        private readonly int _callLimitSeconds; // 60 seconds.

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Queue<long> _callTickStack = new Queue<long>(60);

        public ThrottleInstance(int callLimit, int callLimitSeconds)
        {
            _callLimit = callLimit;
            _callLimitSeconds = callLimitSeconds;
        }

        public async Task<T> DoAsync<T>(Func<Task<T>> func)
        {
            await _semaphore.WaitAsync();

            if (_callTickStack.Count >= _callLimit)
            {
                long nowTicks = DateTime.UtcNow.Ticks;
                var callAtLimitTicks = _callTickStack.Peek();

                TimeSpan timeSpan = TimeSpan.FromTicks(nowTicks - callAtLimitTicks);

                var callLimitMilliseconds = _callLimitSeconds * 1000;
                if (timeSpan.TotalMilliseconds < callLimitMilliseconds)
                {
                    var millisecondsDelay = (int)(callLimitMilliseconds - Math.Round(timeSpan.TotalMilliseconds));
                    await Task.Delay(millisecondsDelay);
                }
            }

            var executionTimeTicks = DateTime.UtcNow.Ticks;
            _callTickStack.Enqueue(executionTimeTicks);
            if (_callTickStack.Count > _callLimit)
            {
                _callTickStack.Dequeue(); // We only need to track the last 60 calls
            }

            var task = await func();

            _semaphore.Release();

            return task;
        }

        /// <summary>Attempts to execute the provided delegate Task 3 times.</summary>
        public async Task DoAsync(Func<Task> func)
        {
            Task<object> teeFunc() => TaskWrapper.WrapTaskInGenericObject(func);

            await this.DoAsync<object>(teeFunc);
        }
    }
}
