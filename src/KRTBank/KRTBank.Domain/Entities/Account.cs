using KRTBank.Domain.Enums;
using KRTBank.Domain.Exceptions;
using KRTBank.Domain.ValueObjects;

namespace KRTBank.Domain.Entities;

public sealed class Account
{
    public Guid Id { get; private set; }
    public HolderName HolderName { get; private set; }
    public Cpf Cpf { get; private set; }
    public AccountStatus Status { get; private set; }

    public Account(string holderName, string cpf)
    {
        Id = Guid.NewGuid();
        HolderName = new HolderName(holderName); 
        Cpf = new Cpf(cpf);
        Status = AccountStatus.Active;
    }
    
    // Rehidratar
    private Account(Guid id, HolderName holderName, Cpf cpf, AccountStatus status)
    {
        Id = id;
        HolderName = holderName;
        Cpf = cpf;
        Status = status;
    }

    public void ChangeHolderName(string newName)
    {
        HolderName = new HolderName(newName);
    }
    
    public static Account ToDomain(string id, string holderName, string cpf, int status)
    {
        return new Account(
            Guid.Parse(id),
            new HolderName(holderName),
            new Cpf(cpf),
            (AccountStatus)status
        );
    }
    
    public void Activate() => Status = AccountStatus.Active;
    public void Deactivate() => Status = AccountStatus.Inactive;
    
    private Account()
    {
        
    }
}