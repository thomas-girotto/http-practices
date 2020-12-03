using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet("api/user/{email}")]
        public User Get(string email)
        {
            return new User
            {
                Email = email,
                MainCompanyBdrId = 1,
                CanTrade = true,
            };
        }

        [HttpGet("api/user/{userId}/companies")]
        public List<Company> GetUserCompanies(int userId)
        {
            return new List<Company>
            {
                new Company { CompanyBdrId = 1, Name = "Company1" },
                new Company { CompanyBdrId = 2, Name = "Company2" },
            };
        }
    }

    public class User
    {
        public string? Email { get; set; }
        public int MainCompanyBdrId { get; set; }
        public bool CanTrade { get; set; }
    }

    public class Company
    {
        public int CompanyBdrId { get; set; }
        public string? Name { get; set; }
    }
}
