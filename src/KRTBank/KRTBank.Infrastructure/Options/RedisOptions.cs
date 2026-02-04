namespace KRTBank.Infrastructure.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string Endpoint { get; set; } = string.Empty;
}