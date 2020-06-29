using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        }
    }
}
