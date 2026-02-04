using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Entities;
using KRTBank.Domain.Enums;
using KRTBank.Domain.Exceptions;
using KRTBank.Domain.Interfaces;
using KRTBank.Domain.ValueObjects;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IEventPublisher _publisher;
    private readonly ICacheService _cache;
        
    public AccountService(IAccountRepository repository, IEventPublisher publisher, ICacheService cache)
    {
        _repository = repository;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<AccountDto>(id.ToString());
        if (cached != null)
            return cached;
        
        var account = await _repository.GetByIdAsync(id, cancellationToken);
        if (account is null)
        {
            return null;
        }

        var dto = new AccountDto(account.Id, account.HolderName, account.Cpf, account.Status);
        
        await _cache.SetAsync(dto.Id.ToString(), dto);

        return dto;
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var alreadyExistAccount = await _repository.GetByCpfAsync(new Cpf(dto.Cpf), cancellationToken);

        if (alreadyExistAccount is not null)
        {
            throw new DomainException("An account for this CPF already exists.", 422);
        }
        
        var account = new Account(dto.HolderName, dto.Cpf);

        await _repository.AddAsync(account, cancellationToken);
        
        await _publisher.PublishAsync(new
        {
            Type = "AccountCreated",
            AccountId = account.Id,
            account.HolderName,
            account.Cpf,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);

        return new AccountDto(account.Id, account.HolderName, account.Cpf, account.Status);
    }

    public async Task UpdateAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(id, cancellationToken)
                      ?? throw new DomainException($"Account with id {id} was not found.", 400);

        var oldName = account.HolderName;

        var newName = dto.HolderName;

        account.ChangeHolderName(newName);

        if (dto.IsActive) account.Activate();
        else account.Deactivate();

        await _repository.UpdateAsync(account, cancellationToken);
        
        await _cache.RemoveAsync(id.ToString());

        await _publisher.PublishAsync(new
        {
            Type = "AccountUpdated",
            AccountId = account.Id,
            OldName = oldName,
            NewName = newName,
            IsActive = account.Status == AccountStatus.Active,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(id, cancellationToken)
                      ?? throw new DomainException($"Account with id {id} was not found.", 400);

        await _repository.DeleteAsync(id, cancellationToken);
        
        await _cache.RemoveAsync(id.ToString());

        await _publisher.PublishAsync(new
        {
            Type = "AccountDeleted",
            AccountId = account.Id,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
    }
}