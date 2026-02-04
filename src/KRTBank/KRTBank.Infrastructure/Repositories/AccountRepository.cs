using Amazon.DynamoDBv2.DataModel;
using KRTBank.Domain.Entities;
using KRTBank.Domain.Interfaces;
using KRTBank.Infrastructure.Models;

namespace KRTBank.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IDynamoDBContext _context;

    public AccountRepository(IDynamoDBContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        var model = ToDbModel(account);
        await _context.SaveAsync(model, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.LoadAsync<AccountDbModel>(id.ToString(), cancellationToken);
        return model is null ? null : ToDomain(model);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        var model = ToDbModel(account);
        await _context.SaveAsync(model, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.LoadAsync<AccountDbModel>(id.ToString(), cancellationToken);

        if (model is null)
            return;

        var account = new Account(model.HolderName, model.Cpf);
        
        account.Deactivate(); // soft delete
        
        await _context.SaveAsync(ToDbModel(account), cancellationToken);
    }
    
    private static AccountDbModel ToDbModel(Account account)
    {
        return new AccountDbModel
        {
            Id = account.Id.ToString(),
            HolderName = account.HolderName,
            Cpf = account.Cpf.Value,
            Status = (int)account.Status
        };
    }

    private static Account ToDomain(AccountDbModel model)
    {
        return new Account(model.HolderName, model.Cpf);
    }
}