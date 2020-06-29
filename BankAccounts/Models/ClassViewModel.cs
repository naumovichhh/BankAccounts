using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAccounts.Models
{
    public class ClassViewModel
    {
        public string Description { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        public decimal[] Values { get; set; }
    }
}
