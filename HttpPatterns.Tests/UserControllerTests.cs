using FluentAssertions;
using HttpPatterns.FunctionalStyle;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpPatterns.Tests
{
    public class UserControllerTests
    {
        private static TimeSpan httpClientTimeout = TimeSpan.FromMilliseconds(10);

        private (UserController sut, FakeMessageHandler fakeHttpHandler) Setup()
        {
            var fakeHandler = new FakeMessageHandler();
            var innerHttpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://just_a_valid_uri") };
            innerHttpClient.Timeout = httpClientTimeout;
            var httpClient = new UserHttpClient(innerHttpClient);
            
            var service = new UserService(httpClient);
            var controller = new UserController(service);

            return (controller, fakeHandler);
        }

        [Fact]
        public async Task Should_return_404_When_backend_returns_404NotFound()
        {
            // Arrange
            var (sut, fakeHandler) = Setup();
            fakeHandler.WillReturnStatusCode(HttpStatusCode.NotFound);

            // Act
            var result = await sut.GetCompanies("prenom.nom@sgcib.com");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Should_return_502BadGateway_When_backend_returns_500InternalServerError()
        {
            // Arrange
            var (sut, fakeHandler) = Setup();
            fakeHandler.WillReturnStatusCode(HttpStatusCode.InternalServerError);

            // Act
            var result = await sut.GetCompanies("prenom.nom@sgcib.com");

            // Assert
            result
                .Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode
                .Should().Be(StatusCodes.Status502BadGateway);
        }

        [Fact]
        public async Task Should_return_504GatewayTimeout_When_backend_http_calls_timeout()
        {
            // Arrange
            var (sut, fakeHandler) = Setup();
            fakeHandler.WillTimeout(httpClientTimeout);

            // Act
            var result = await sut.GetCompanies("prenom.nom@sgcib.com");

            // Assert
            result
                .Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode
                .Should().Be(StatusCodes.Status504GatewayTimeout);
        }

        [Fact]
        public async Task Should_return_499ClientClosedRequest_When_the_client_timeout_before_server_responds()
        {
            // Arrange
            var (sut, fakeHandler) = Setup();
            fakeHandler.WillWaitForTheCancellationTokenToBeCancelled();
            var requestCts = new CancellationTokenSource();
            requestCts.Cancel();

            // Act
            var result = await sut.GetCompanies("prenom.nom@sgcib.com", requestCts.Token);

            // Assert
            result
                .Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode
                .Should().Be(499);
        }
    }
}
