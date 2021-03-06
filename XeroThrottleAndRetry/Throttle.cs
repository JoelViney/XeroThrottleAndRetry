﻿using System;
using System.Threading;
using System.Threading.Tasks;
using XeroThrottleAndRetry.Helper;
using XeroThrottleAndRetry.Helpers;

namespace XeroThrottleAndRetry
{
    /// <summary>
    /// Limits the amount of calls for a set amount of time. e.g. 60 calls in 60 seconds.
    /// 
    /// How to use:
    /// await Throttle.DoAsync(() => this.MyMethod());
    /// or 
    /// var result = await Throttle.DoAsync<int>(() => this.MyMethod());
    /// </summary>
    public static class Throttle
    {
        private const int CallLimit = 60;        // 60 calls in...
        private const int CallLimitSeconds = 60; // 60 seconds.

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private static ThrottleInstance _throttleInstance;

        public static async Task<T> DoAsync<T>(Func<Task<T>> func)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_throttleInstance == null)
                {
                    _throttleInstance = new ThrottleInstance(CallLimit, CallLimitSeconds);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return await _throttleInstance.DoAsync(func);
        }

        /// <summary>Attempts to execute the provided delegate Task 3 times.</summary>
        public static async Task DoAsync(Func<Task> func)
        {
            Task<object> teeFunc() => TaskWrapper.WrapTaskInGenericObject(func);

            await DoAsync<object>(teeFunc);
        }
    }
}
