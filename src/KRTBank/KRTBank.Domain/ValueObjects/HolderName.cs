using KRTBank.Domain.Exceptions;

namespace KRTBank.Domain.ValueObjects;

public sealed class HolderName
{
    public string Value { get; }

    public HolderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("HolderName is required.", 400);

        if (value.Length < 3)
            throw new DomainException("HolderName is too short.", 400);

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
        => obj is HolderName other && Value == other.Value;

    public override int GetHashCode()
        => Value.GetHashCode();
    
    public static bool operator ==(HolderName? left, HolderName? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(HolderName? left, HolderName? right)
        => !(left == right);
    
    public static implicit operator string(HolderName holderName) => holderName.Value;
}