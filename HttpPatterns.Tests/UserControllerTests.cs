using FluentAssertions;
using HttpPatterns.FunctionalStyle;
using HttpPatterns.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpPatterns.Tests
{
    public class UserControllerTests
    {
        private static TimeSpan httpClientTimeout = TimeSpan.FromSeconds(1);

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
        public async Task Should_return_200_When_both_backend_calls_succeeds()
        {
            // Arrange
            var (sut, fakeHandler) = Setup();
            fakeHandler.WillReturnOk(new User { CanTrade = true, Email = "", MainCompanyBdrId = 1 }) ;
            var companies = new List<Company> { new Company { CompanyBdrId = 1, Name = "SG" } };
            fakeHandler.WillReturnOk(companies);

            // Act
            var result = await sut.GetCompanies("prenom.nom@sgcib.com");

            // Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(companies);
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
        public async Task Should_return_499ClientClosedRequest_When_the_client_abort_request_before_http_call()
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
