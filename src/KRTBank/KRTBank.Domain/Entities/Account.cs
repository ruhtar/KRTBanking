using KRTBank.Domain.Enums;
using KRTBank.Domain.Exceptions;
using KRTBank.Domain.ValueObjects;

namespace KRTBank.Domain.Entities;

public sealed class Account
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
    
    // Rehidratar
    private Account(Guid id, string holderName, Cpf cpf, AccountStatus status)
    {
        Id = id;
        HolderName = holderName;
        Cpf = cpf;
        Status = status;
    }

    public void ChangeHolderName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Nome do titular é obrigatório.");

        HolderName = newName;
    }
    
    public static Account ToDomain(string id, string holderName, string cpf, int status)
    {
        return new Account(
            Guid.Parse(id),
            holderName,
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