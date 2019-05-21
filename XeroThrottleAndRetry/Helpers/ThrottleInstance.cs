using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XeroThrottleAndRetry.Helpers;

namespace XeroThrottleAndRetry.Helper
{
    /// <summary>
    /// This instance exists so we can write tests around it. You should call the static Throtlle class.
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
