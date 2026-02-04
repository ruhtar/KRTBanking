using KRTBank.Application.DTOs;
using KRTBank.Domain.ResultPattern;

namespace KRTBank.Application.Interfaces;

public interface IAccountService
{
    Task<Result<AccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AccountDto>> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
