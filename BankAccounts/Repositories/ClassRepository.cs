using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankAccounts.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Repositories
{
    public class ClassRepository : IRepository<Class>
    {
        private DbContext context;

        public ClassRepository(DbContext context)
        {
            this.context = context;
        }

        public void Create(Class entity)
        {
            context.Set<Class>().Add(entity);
        }

        public void Delete(object id)
        {
            var entity = context.Set<Class>().Find(id);
            context.Set<Class>().Remove(entity);
        }

        public async Task<IEnumerable<Class>> GetAllAsync()
        {
            return await context.Set<Class>().ToListAsync();
        }

        public async Task<Class> GetByIdAsync(object id)
        {
            return await context.Set<Class>().FindAsync(id);
        }

        public void Update(Class entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
