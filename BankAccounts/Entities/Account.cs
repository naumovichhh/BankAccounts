using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAccounts.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string Number { get; set; }
        public decimal InActive { get; set; }
        public decimal InPassive { get; set; }
        public decimal Debet { get; set; }
        public decimal Credit { get; set; }
        public decimal OutActive { get; set; }
        public decimal OutPassive { get; set; }

        public virtual Class Class { get; set; }
    }
}
