using HttpPatterns.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPatterns.ErrorCodeStyle
{
    public interface IUserService
    {
        Task<(List<Company>? companies, ErrorKind? error)> GetCompanies(string email, CancellationToken cancellationToken);
    }

    public class UserService : IUserService
    {
        private readonly IUserHttpClient httpClient;

        public UserService(IUserHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<(List<Company>? companies, ErrorKind? error)> GetCompanies(string email, CancellationToken cancellationToken)
        {
            var (user, error) = await httpClient.GetUser(email, cancellationToken);
            if (error != null)
            {
                return (null, error);
            }
            return await httpClient.GetCompanies(user!.MainCompanyBdrId, cancellationToken);
        }
    }
}
