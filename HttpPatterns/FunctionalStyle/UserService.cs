using HttpPatterns.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.FunctionalStyle
{
    public interface IUserService
    {
        Task<HttpResult<List<Company>>> GetCompanies(string email, CancellationToken cancellationToken);
    }

    public class UserService : IUserService
    {
        private readonly IUserHttpClient httpClient;

        public UserService(IUserHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResult<List<Company>>> GetCompanies(string email, CancellationToken cancellationToken)
        {
            return await (await httpClient.GetUser(email, cancellationToken))
                .Match(user => user.MainCompanyBdrId)
                .MatchAsync(companyId => httpClient.GetCompanies(companyId, cancellationToken));
        }
    }
}
