namespace KRTBank.Infrastructure.Options;

public class SnsOptions
{
    public const string SectionName = "SNS";

    public string SecretName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}