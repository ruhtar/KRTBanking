using Amazon.DynamoDBv2.DataModel;
using KRTBank.Domain.Entities;

namespace KRTBank.Infrastructure.Models;

[DynamoDBTable("accounts")]
public class AccountDbModel
{
    [DynamoDBHashKey]
    public required string Id { get; set; }
    public required string HolderName { get; set; } 
    
    [DynamoDBProperty]
    [DynamoDBGlobalSecondaryIndexHashKey("CpfIndex")] 
    public required string Cpf { get; set; } 
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