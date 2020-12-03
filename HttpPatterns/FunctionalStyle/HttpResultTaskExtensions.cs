using System;
using System.Threading.Tasks;

namespace HttpPatterns.FunctionalStyle
{
    public static class HttpResultTaskExtensions
    {
        public static async Task<HttpResult<T2>> SelectMany<T1, T2>(this Task<HttpResult<T1>> task, Func<T1, Task<HttpResult<T2>>> f)
            where T1 : notnull
            where T2 : notnull
            => await (await task).SelectMany(f);
    }
}
