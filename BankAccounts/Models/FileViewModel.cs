using System.Collections.Generic;

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
