using Amazon.DynamoDBv2.DataModel;
using KRTBank.Domain.Entities;

namespace KRTBank.Infrastructure.Models;

[DynamoDBTable("accounts")]
public class AccountDbModel
{
    [DynamoDBHashKey]
    public string Id { get; set; } = default!;
    public string HolderName { get; set; } = default!;
    
    [DynamoDBProperty]
    [DynamoDBGlobalSecondaryIndexHashKey("CpfIndex")] 
    public string Cpf { get; set; } = default!;
    public int Status { get; set; }
    
    public static AccountDbModel ToDbModel(Account account)
    {
        return new AccountDbModel
        {
            Id = account.Id.ToString(),
            HolderName = account.HolderName,
            Cpf = account.Cpf.NormalizedValue,
            Status = (int)account.Status
        };
    }
}