using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAccounts.Models
{
    public class FileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BankName { get; set; }
        public List<ClassViewModel> Classes { get; set; }
        public decimal[] Values { get; set; }
    }
}
