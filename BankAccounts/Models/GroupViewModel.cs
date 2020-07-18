using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class GroupViewModel
    {
        public string Number { get; set; }
        public List<AccountViewModel> Accounts { get; set; }
        public decimal[] Values { get; set; }
    }
}
