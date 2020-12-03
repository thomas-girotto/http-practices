using HttpPatterns.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.ErrorCodeStyle
{
    public static class HttpCallHelper
    {
        public static async Task<(T? user, ErrorKind? error)> FromHttpCall<T>(Func<Task<HttpResponseMessage>> httpCall, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return (default(T), ErrorKind.ClientClosedRequest);
                }

                var httpResponseMessage = await httpCall();

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<T>();
                    return (result, null);
                }

                return httpResponseMessage.StatusCode switch
                {
                    HttpStatusCode.NotFound => (default(T), ErrorKind.NotFound),
                    _ => (default(T), ErrorKind.BackendError),
                };
            }
            catch (OperationCanceledException)
            {
                return (default(T), ErrorKind.Timeout);
            }
            catch (Exception)
            {
                return (default(T), ErrorKind.TechnicalError);
            }
        }

        public static IActionResult ToActionResult<T>(T? response, ErrorKind? error)
        {
            return (error, response) switch
            {
                var (_, result) when result is not null => new OkObjectResult(result),
                (ErrorKind.NotFound, _) => new NotFoundResult(),
                (ErrorKind.BackendError, _) => new StatusCodeResult(StatusCodes.Status502BadGateway),
                (ErrorKind.Timeout, _) => new StatusCodeResult(StatusCodes.Status504GatewayTimeout),
                (ErrorKind.ClientClosedRequest, _) => new StatusCodeResult(499),
                (ErrorKind.TechnicalError, _) => new StatusCodeResult(StatusCodes.Status500InternalServerError),
                (_, _) => throw new NotImplementedException("Forgot a case"),
            };
        }
    }
}
