using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Entities;
using KRTBank.Domain.Enums;
using KRTBank.Domain.ResultPattern;
using KRTBank.Domain.ValueObjects;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cache;

    public AccountService(IAccountRepository accountRepository, ICacheService cache)
    {
        _accountRepository = accountRepository;
        _cache = cache;
    }

    public async Task<Result<AccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cache = await _cache.GetAsync<AccountDto>(id.ToString());
        var cacheExists = cache != null;
        if (cacheExists)
            return Result<AccountDto>.Ok(data: cache);

        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null)
            return Result<AccountDto>.Fail($"Account with id {id} was not found.", 404);

        var dto = new AccountDto(account.Id, account.HolderName, account.Cpf, account.Status);
        await _cache.SetAsync(dto.Id.ToString(), dto);

        return Result<AccountDto>.Ok("Account found.", dto);
    }

    public async Task<Result<AccountDto>> CreateAsync(CreateAccountDto dto,
        CancellationToken cancellationToken = default)
    {
        var existingAccount = await _accountRepository.GetByCpfAsync(new Cpf(dto.Cpf), cancellationToken);
        if (existingAccount is not null)
            return Result<AccountDto>.Fail("An account for this CPF already exists.", 422);

        var account = new Account(dto.HolderName, dto.Cpf);

        await _accountRepository.AddAsync(account, cancellationToken);

        var resultDto = new AccountDto(account.Id, account.HolderName, account.Cpf, account.Status);

        return Result<AccountDto>.Ok("Account created successfully.", resultDto, 201);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null)
            return Result.Fail($"Account with id {id} was not found.", 404);

        account.ChangeHolderName(dto.HolderName);

        if (dto.IsActive is true)
            account.Activate();
        else if (dto.IsActive is false)
            account.Deactivate();
  
        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _cache.RemoveAsync(id.ToString());


        return Result.Ok("Account updated successfully.");
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null)
            return Result.Fail($"Account with id {id} was not found.", 404);

        await _accountRepository.DeleteAsync(id, cancellationToken);
        await _cache.RemoveAsync(id.ToString());

        return Result.Ok("Account deleted successfully.");
    }
}