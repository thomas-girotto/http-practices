using HttpPatterns.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.FunctionalStyle
{
    public interface IUserHttpClient
    {
        Task<HttpResult<User>> GetUser(string email, CancellationToken cancellationToken);
        Task<HttpResult<List<Company>>> GetCompanies(int mainCompanyId, CancellationToken cancellationToken);
    }

    public class UserHttpClient : IUserHttpClient
    {
        private readonly HttpClient httpClient;

        public UserHttpClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResult<User>> GetUser(string email, CancellationToken cancellationToken)
        {
            return await HttpResult<User>.FromHttpCall(() => httpClient.GetAsync($"api/user/{email}"), cancellationToken);
        }

        public async Task<HttpResult<List<Company>>> GetCompanies(int mainCompanyId, CancellationToken cancellationToken)
        {
            return await HttpResult<List<Company>>.FromHttpCall(() => httpClient.GetAsync($"api/companies/{mainCompanyId}"), cancellationToken);
        }
    }
}
