using System.Text;
using KRTBank.Domain.Exceptions;

namespace KRTBank.Domain.ValueObjects;

public sealed class Cpf : IEquatable<Cpf>
{
    public string Value { get; }

    public Cpf(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("CPF é obrigatório.");

        var normalized = Normalize(value);

        if (!IsValid(normalized))
            throw new DomainException("CPF inválido.");

        Value = normalized;
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

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as Cpf);

    public bool Equals(Cpf? other)
        => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(Cpf cpf) => cpf.Value;
}