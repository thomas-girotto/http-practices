using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.FunctionalStyle
{
    public class HttpResult<TSuccess> where TSuccess : notnull
    {
        public TSuccess? Response { get; init; }
        public ErrorKind? Error { get; init; }

        private HttpResult(TSuccess response)
        {
            Response = response;
        }

        private HttpResult(ErrorKind error)
        {
            Error = error;
        }

        public static HttpResult<TSuccess> FromSuccess(TSuccess success)
        {
            return new HttpResult<TSuccess>(success);
        }

        public static HttpResult<TSuccess> FromError(ErrorKind error)
        {
            return new HttpResult<TSuccess>(error);
        }

        public static async Task<HttpResult<TSuccess>> FromHttpCall(Func<Task<HttpResponseMessage>> httpCall, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return HttpResult<TSuccess>.FromError(ErrorKind.ClientClosedRequest);
                }

                var httpResponseMessage = await httpCall();

                if (cancellationToken.IsCancellationRequested)
                {
                    return HttpResult<TSuccess>.FromError(ErrorKind.ClientClosedRequest);
                }

                if (httpResponseMessage == null)
                {
                    return new HttpResult<TSuccess>(ErrorKind.TechnicalError);
                }
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<TSuccess>();
                    if (result == null)
                    {
                        return new HttpResult<TSuccess>(ErrorKind.BackendNotFound);
                    }
                    return new HttpResult<TSuccess>(result);
                }
                else
                {
                    return httpResponseMessage.StatusCode switch
                    {
                        HttpStatusCode.NotFound => new HttpResult<TSuccess>(ErrorKind.BackendNotFound),
                        _ => new HttpResult<TSuccess>(ErrorKind.BackendOtherNonSuccessStatusCode),
                    };
                }
            }
            catch (OperationCanceledException ex)
            {
                return ex.CancellationToken.IsCancellationRequested
                    ? new HttpResult<TSuccess>(ErrorKind.ClientClosedRequest)
                    : new HttpResult<TSuccess>(ErrorKind.Timeout);
            }
            catch (Exception)
            {
                return new HttpResult<TSuccess>(ErrorKind.TechnicalError);
            }
        }

        public async Task<HttpResult<TOther>> MatchAsync<TOther>(Func<TSuccess, Task<HttpResult<TOther>>> asyncFunc) where TOther: notnull
        {
            if (Error != null)
            {
                return HttpResult<TOther>.FromError(Error.Value);
            }
            return await asyncFunc(Response!);
        }

        public HttpResult<TOther> Match<TOther>(Func<TSuccess, TOther> action) where TOther : notnull
        {
            if (Error != null)
            {
                return HttpResult<TOther>.FromError(Error.Value);
            }
            return HttpResult<TOther>.FromSuccess(action(Response!));
        }

        public IActionResult ToActionResult()
        {
            return (Error, Response) switch
            {
                (ErrorKind.BackendNotFound, _) => new NotFoundResult(),
                (ErrorKind.Timeout, _) => new StatusCodeResult(StatusCodes.Status504GatewayTimeout),
                (ErrorKind.ClientClosedRequest, _) => new StatusCodeResult(499),
                (ErrorKind.TechnicalError, _) => new StatusCodeResult(StatusCodes.Status500InternalServerError),
                (ErrorKind.BackendOtherNonSuccessStatusCode, _) => new StatusCodeResult(StatusCodes.Status502BadGateway),
                var (_, result) when result is not null => new OkObjectResult(result),
                (_, _) => throw new NotImplementedException("Forgot a case"),
            };
        }
    }

    public enum ErrorKind
    {
        TechnicalError,
        Timeout,
        ClientClosedRequest,
        BackendNotFound,
        BackendOtherNonSuccessStatusCode
    }
}
