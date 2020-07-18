using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class ClassViewModel
    {
        public string Description { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        public decimal[] Values { get; set; }
    }
}
