using KRTBank.Domain.Exceptions;

namespace KRTBank.Domain.ValueObjects;

public class HolderName
{
    public string Value { get; }

    public HolderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Nome do titular é obrigatório.");

        if (value.Length < 3)
            throw new DomainException("Nome do titular é muito curto.");

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
        => obj is HolderName other && Value == other.Value;

    public override int GetHashCode()
        => Value.GetHashCode();
    
    public static implicit operator string(HolderName holderName) => holderName.Value;
}