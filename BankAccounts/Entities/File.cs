using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAccounts.Entities
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BankName { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
    }
}
