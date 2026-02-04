using KRTBank.Application.DTOs;

namespace KRTBank.Application.Interfaces;

public interface IAccountService
{
    Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}