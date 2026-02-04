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
    
    public async Task AddAsync(Account account)
    {
        var model = ToDbModel(account);
        await _context.SaveAsync(model);
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        var model = await _context.LoadAsync<AccountDbModel>(id.ToString());
        return model is null ? null : ToDomain(model);
    }

    public async Task UpdateAsync(Account account)
    {
        var model = ToDbModel(account);
        await _context.SaveAsync(model);
    }

    public async Task DeleteAsync(Guid id)
    {
        var model = await _context.LoadAsync<AccountDbModel>(id.ToString());

        if (model is null)
            return;

        var account = new Account(model.HolderName, model.Cpf);
        
        account.Deactivate(); // soft delete
        
        await _context.SaveAsync(ToDbModel(account));
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