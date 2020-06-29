using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankAccounts.Data;
using BankAccounts.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Repositories
{
    public class AccountRepository : IRepository<Account>
    {
        private DbContext context;

        public AccountRepository(DbContext context)
        {
            this.context = context;
        }
        public void Create(Account entity)
        {
            context.Set<Account>().Add(entity);
        }

        public void Delete(object id)
        {
            var entity = context.Set<Account>().Find(id);
            context.Set<Account>().Remove(entity);
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await context.Set<Account>().ToListAsync();
        }

        public async Task<Account> GetByIdAsync(object id)
        {
            return await context.Set<Account>().FindAsync(id);
        }

        public void Update(Account entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
