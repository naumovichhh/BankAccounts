using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankAccounts.Data;
using BankAccounts.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Repositories
{
    public class FileRepository : IRepository<File>
    {
        private DbContext context;

        public FileRepository(DbContext context)
        {
            this.context = context;
        }

        public void Create(File entity)
        {
            context.Set<File>().Add(entity);
        }

        public void Delete(object id)
        {
            var entity = context.Set<File>().Find(id);
            context.Set<File>().Remove(entity);
        }

        public async Task<IEnumerable<File>> GetAllAsync()
        {
            return await context.Set<File>().ToListAsync();
        }

        public async Task<File> GetByIdAsync(object id)
        {
            return await context.Set<File>().FindAsync(id);
        }

        public void Update(File entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
