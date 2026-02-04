using KRTBank.Domain.Entities;

namespace KRTBank.Domain.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account);
    Task<Account?> GetByIdAsync(Guid id);
    Task UpdateAsync(Account account);
    Task DeleteAsync(Guid id);
}