using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        [Required]
        [MaxLength(10)]
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
