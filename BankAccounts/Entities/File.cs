using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Entities
{
    public class File
    {
        public File()
        {
            Classes = new List<Class>();
        }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string BankName { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
    }
}
