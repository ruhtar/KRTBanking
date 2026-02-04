using System.Text.Json.Serialization;

namespace KRTBank.Infrastructure.Secrets;

public class SnsSecret
{
    [JsonPropertyName("krtbank-account-events-arn")] // TOdo: DEIXAR ASSIM mSM?
    public string TopicArn { get; set; } = string.Empty;
}