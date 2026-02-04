using KRTBank.Domain.Enums;

namespace KRTBank.Application.DTOs;

public sealed record AccountDto(
    Guid Id,
    string HolderName,
    string Cpf,
    AccountStatus Status
);