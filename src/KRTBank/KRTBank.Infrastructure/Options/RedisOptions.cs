namespace KRTBank.Infrastructure.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public required string Endpoint { get; set; } = string.Empty;
    public required int TtlInDays { get; set; }
}