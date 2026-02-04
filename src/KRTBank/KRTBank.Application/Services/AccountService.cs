using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;

namespace KRTBank.Application.Services;

public class AccountService : IAccountService
{
    public Task<Guid> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}