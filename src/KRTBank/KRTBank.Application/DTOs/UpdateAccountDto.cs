namespace KRTBank.Application.DTOs;

public sealed record UpdateAccountDto(
    string HolderName,
    bool IsActive
);