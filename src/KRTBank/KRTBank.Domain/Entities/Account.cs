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
        Activate();
    }
    
    public Account(string id, string holderName, string cpf, int status)
    {
        Id = Guid.Parse(id);
        HolderName = new HolderName(holderName);
        Cpf = new Cpf(cpf);
        Status = (AccountStatus)status;
    }

    public void ChangeHolderName(string newName)
    {
        HolderName = new HolderName(newName);
    }
    
    public void Activate() => Status = AccountStatus.Active;
    public void Deactivate() => Status = AccountStatus.Inactive;
}