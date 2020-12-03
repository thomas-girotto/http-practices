using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpPatterns.Models
{
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
