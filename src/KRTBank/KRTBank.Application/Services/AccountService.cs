using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Entities;
using KRTBank.Domain.Enums;
using KRTBank.Domain.Exceptions;
using KRTBank.Domain.Interfaces;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IEventPublisher _publisher;

    public AccountService(IAccountRepository repository, IEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // var cached = await _cache.GetAsync(id);
        // // if (cached != null)
        // return cached;
        var account = await _repository.GetByIdAsync(id, cancellationToken);
        if (account is null)
        {
            return null;
        }
        
        // await _cache.SetAsync(dto);

        return new AccountDto(account.Id, account.HolderName, account.Cpf, account.Status);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
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

        await _publisher.PublishAsync(new
        {
            Type = "AccountDeleted",
            AccountId = account.Id,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
    }
}