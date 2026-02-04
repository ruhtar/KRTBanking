using Amazon.DynamoDBv2.DataModel;

namespace KRTBank.Infrastructure.Models;

[DynamoDBTable("accounts")]
public class AccountDbModel
{
    [DynamoDBHashKey]
    public string Id { get; set; } = default!;

    public string HolderName { get; set; } = default!;
    public string Cpf { get; set; } = default!;
    public int Status { get; set; }
}