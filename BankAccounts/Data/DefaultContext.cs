using System.Linq;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Entities;

namespace BankAccounts.Data
{
    public class DefaultContext : DbContext
    {
        public DbSet<File> Files { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Account> Accounts { get; set; }

        public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("numeric(28,13)");
            }
        }
    }
}
