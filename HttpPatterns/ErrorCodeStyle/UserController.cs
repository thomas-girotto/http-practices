using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HttpPatterns.ErrorCodeStyle
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("api/user/{email}/companies")]
        public async Task<IActionResult> GetCompanies(string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (companies, error) = await userService.GetCompanies(email, cancellationToken);
            return HttpCallHelper.ToActionResult(companies, error);
        }
    }
}
