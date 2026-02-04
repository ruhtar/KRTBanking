using KRTBank.Domain.Enums;
using KRTBank.Domain.ValueObjects;

namespace KRTBank.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string HolderName { get; private set; }
    public Cpf Cpf { get; private set; }
    public AccountStatus Status { get; private set; }

    public Account(string holderName, string cpf)
    {
        Id = Guid.NewGuid();
        HolderName = holderName; // TODO: value object?
        Cpf = new Cpf(cpf);
        Status = AccountStatus.Active;
    }

    public void Deactivate() => Status = AccountStatus.Inactive;
    
    private Account()
    {
        
    }
}