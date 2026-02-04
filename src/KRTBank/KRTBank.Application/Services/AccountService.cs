using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Entities;
using KRTBank.Domain.Interfaces;

namespace KRTBank.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    
    //TODO: FAZER CACHE E PUBLISHer

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = new Account(dto.HolderName, dto.Cpf);
        
        await _repository.AddAsync(account);
        
        return account.Id;
    }


    public async Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // var cached = await _cache.GetAsync(id);
        // if (cached != null) return cached;

        var account = await _repository.GetByIdAsync(id);

        if (account is null)
        {
            return null;
        }
        
        // await _cache.SetAsync(dto);

        return new AccountDto(account.Id, account.HolderName, account.Cpf, account.Status);
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