using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Entities
{
    public class Class
    {
        public Class()
        {
            Accounts = new List<Account>();
        }

        public int Id { get; set; }
        public int FileId { get; set; }
        [Required]
        [MaxLength(1)]
        public string Number { get; set; }
        [Required]
        public string Description { get; set; }

        public virtual File File { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
