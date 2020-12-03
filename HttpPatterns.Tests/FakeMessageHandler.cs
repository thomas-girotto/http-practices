using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.Tests
{
    public class FakeMessageHandler : HttpMessageHandler
    {
        private Queue<Func<CancellationToken, Task<HttpResponseMessage>>> _actions = new Queue<Func<CancellationToken, Task<HttpResponseMessage>>>();

        public FakeMessageHandler WillReturnOk<T>(T payload)
        {
            _actions.Enqueue((_) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            }));
            return this;
        }

        public FakeMessageHandler WillTimeout(TimeSpan timeAfterWhichWeShouldTimeout)
        {
            _actions.Enqueue(async (ct) =>
            {
                // Just wait one second more than the timeout set to the HttpClient instance
                await Task.Delay(timeAfterWhichWeShouldTimeout.Add(TimeSpan.FromSeconds(1)), ct);
                throw new Exception("Should have timeout before this exception");
            });
            return this;
        }

        public FakeMessageHandler WillWaitForTheCancellationTokenToBeCancelled()
        {
            _actions.Enqueue(async (ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                throw new Exception("");
            });
            return this;
        }

        public FakeMessageHandler WillReturnNull()
        {
            _actions.Enqueue((_) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            return this;
        }

        public FakeMessageHandler WillReturnStatusCode(HttpStatusCode statusCode)
        {
            _actions.Enqueue((_) => Task.FromResult(new HttpResponseMessage(statusCode)));
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_actions.TryDequeue(out var buildResponseFunc))
            {
                return buildResponseFunc(cancellationToken);
            }
            
            throw new InvalidOperationException("An http call was made and you didn't provide any scenario to respond to it");
        }
    }
}
