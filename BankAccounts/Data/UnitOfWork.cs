using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Entities;
using BankAccounts.Repositories;

namespace BankAccounts.Data
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private bool disposed;
        private DefaultContext context;
        private IRepository<Class> classRepository;
        private IRepository<Account> accountRepository;
        private IRepository<File> fileRepository;

        public UnitOfWork(DefaultContext context)
        {
            this.context = context;
        }

        public DbContext Context => context;

        public IRepository<Class> ClassRepository
        {
            get
            {
                if (classRepository == null)
                {
                    classRepository = new ClassRepository(context);
                }
                return classRepository;
            }
        }

        public IRepository<Account> AccountRepository
        {
            get
            {
                if (accountRepository == null)
                {
                    accountRepository = new AccountRepository(context);
                }
                return accountRepository;
            }
        }

        public IRepository<File> FileRepository
        {
            get
            {
                if (fileRepository == null)
                {
                    fileRepository = new FileRepository(context);
                }
                return fileRepository;
            }
        }

        public Task SaveAsync()
        {
            return context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                context.Dispose();
            }

            disposed = true;
        }
    }
}
