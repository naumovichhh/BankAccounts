using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAccounts.Entities
{
    public class Class
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }

        public virtual File File { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
