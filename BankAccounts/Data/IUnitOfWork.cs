using System.Threading.Tasks;
using BankAccounts.Entities;
using BankAccounts.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Data
{
    public interface IUnitOfWork
    {
        DbContext Context { get; }
        IRepository<Account> AccountRepository { get; }
        IRepository<File> FileRepository { get; }
        IRepository<Class> ClassRepository { get; }
        Task SaveAsync();
    }
}
