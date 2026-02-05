using System.Text;
using KRTBank.Domain.Exceptions;

namespace KRTBank.Domain.ValueObjects;

public sealed class Cpf
{
    public string NormalizedValue { get; }
    public string FormattedValue { get; }

    public Cpf(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("CPF is required.", 400);

        var normalized = Normalize(value);

        if (!IsValid(normalized))
            throw new DomainException("Invalid CPF.", 400);

        NormalizedValue = normalized;
        FormattedValue = Format(normalized);
    }

    private static string Normalize(string cpf)
    {
        var digits = new StringBuilder();

        foreach (var c in cpf)
        {
            if (char.IsDigit(c))
                digits.Append(c);
        }

        return digits.ToString();
    }

    private static string Format(string cpf)
    {
        var part1 = cpf.Substring(0, 3);
        var part2 = cpf.Substring(3, 3);
        var part3 = cpf.Substring(6, 3);
        var part4 = cpf.Substring(9, 2);

        return $"{part1}.{part2}.{part3}-{part4}";
    }

    private static bool IsValid(string cpf)
    {
        if (cpf.Length != 11) return false;
        if (cpf.All(c => c == cpf[0])) return false;

        var numbers = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);

        var remainder = (sum * 10) % 11;
        if (remainder == 10) remainder = 0;
        if (remainder != numbers[9]) return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);

        remainder = (sum * 10) % 11;
        if (remainder == 10) remainder = 0;

        return remainder == numbers[10];
    }

    public override string ToString() => NormalizedValue;

    public override bool Equals(object? obj) => Equals(obj as Cpf);

    public bool Equals(Cpf? other)
        => other is not null && NormalizedValue == other.NormalizedValue;

    public static bool operator ==(Cpf? left, Cpf? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Cpf? left, Cpf? right)
        => !(left == right);

    public override int GetHashCode() => NormalizedValue.GetHashCode();

    public static implicit operator string(Cpf cpf) => cpf.NormalizedValue;
}
