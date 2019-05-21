using System;
using System.Threading.Tasks;

namespace ThrottlerAndRetrier.Helpers
{
    internal class TaskWrapper
    {
        /// <summary>
        /// Wraps a Func<Task> into a generic Task so we can call a generics based method.
        /// </summary>
        internal static async Task<object> WrapTaskInGenericObject(Func<Task> func)
        {
            await func();

            return null;
        }
    }
}
