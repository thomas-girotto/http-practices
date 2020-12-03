using System;

namespace HttpPatterns.Models
{
    public enum ErrorKind
    {
        TechnicalError,
        Timeout,
        ClientClosedRequest,
        NotFound,
        BackendError
    }
}
