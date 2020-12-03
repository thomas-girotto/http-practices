using HttpPatterns.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.ErrorCodeStyle
{
    public interface IUserHttpClient
    {
        Task<(User? user, ErrorKind? error)> GetUser(string email, CancellationToken cancellationToken);
        Task<(List<Company>? companies, ErrorKind? error)> GetCompanies(int mainCompanyId, CancellationToken cancellationToken);
    }

    public class UserHttpClient : IUserHttpClient
    {
        private readonly HttpClient httpClient;

        public UserHttpClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<(User? user, ErrorKind? error)> GetUser(string email, CancellationToken cancellationToken)
        {
            return await HttpCallHelper.FromHttpCall<User>(() => httpClient.GetAsync($"api/user/{email}"), cancellationToken);
        }

        public async Task<(List<Company>? companies, ErrorKind? error)> GetCompanies(int mainCompanyId, CancellationToken cancellationToken)
        {
            return await HttpCallHelper.FromHttpCall<List<Company>>(() => httpClient.GetAsync($"api/companies/{mainCompanyId}"), cancellationToken);
        }
    }
}
