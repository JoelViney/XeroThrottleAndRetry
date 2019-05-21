using System;
using System.Threading.Tasks;

namespace XeroThrottleAndRetry
{
    /// <summary>
    /// Provides Retry on error functionality with minimal impact on the calling code.
    /// 
    /// How to use:
    /// await ThrottleAndRetry.DoAsync(() => this.MyMethod());
    /// or 
    /// var result = await ThrottleAndRetry.DoAsync<int>(() => this.MyMethod());
    /// </summary>
    /// <summary>Combines the Throttle and Retry functionality into a single call.</summary>
    public class ThrottleAndRetry
    {
        public static async Task DoAsync(Func<Task> func)
        {
            await Retry.DoAsync(() => Throttle.DoAsync(func));
        }

        public static async Task<T> DoAsync<T>(Func<Task<T>> func)
        {
            return await Retry.DoAsync<T>(() => Throttle.DoAsync<T>(func));
        }
    }
}
