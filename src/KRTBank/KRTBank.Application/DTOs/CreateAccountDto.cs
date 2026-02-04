namespace KRTBank.Application.DTOs;

public sealed record CreateAccountDto(
    string HolderName,
    string Cpf
);