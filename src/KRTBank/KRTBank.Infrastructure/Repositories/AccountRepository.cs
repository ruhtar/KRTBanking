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
        var model = AccountDbModel.ToDbModel(account);
        await _context.SaveAsync(model, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.LoadAsync<AccountDbModel>(id.ToString(), cancellationToken);
        return model is null ? null : Account.ToDomain(model.Id, model.HolderName, model.Cpf, model.Status);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        var model = AccountDbModel.ToDbModel(account);
        await _context.SaveAsync(model, cancellationToken);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _context.DeleteAsync<AccountDbModel>(id.ToString(), cancellationToken);
    }

}