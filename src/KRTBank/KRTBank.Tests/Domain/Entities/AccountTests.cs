using KRTBank.Domain.Entities;
using KRTBank.Domain.Enums;
using KRTBank.Domain.Exceptions;

namespace KRTBank.Tests.Domain.Entities;

public class AccountTests
{
    [Fact]
    public void Constructor_Should_Create_Account_With_Active_Status()
    {
        // Arrange
        const string holderName = "Arthur";
        const string cpf = "85025957095";

        // Act
        var account = new Account(holderName, cpf);

        // Assert
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.Equal(holderName, account.HolderName.Value);
        Assert.Equal(cpf, account.Cpf.NormalizedValue);
    }
    
    [Fact]
    public void Constructor_Should_Throw_Exception_When_Cpf_Is_Invalid()
    {
        // Arrange
        const string holderName = "Arthur";
        const string cpf = "123";

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            new Account(holderName, cpf)
        );
    }

    [Fact]
    public void ChangeHolderName_Should_Update_Name()
    {
        // Arrange
        var account = new Account("Arthur", "85025957095");

        // Act
        account.ChangeHolderName("Novo Nome");

        // Assert
        Assert.Equal("Novo Nome", account.HolderName.Value);
    }

    [Fact]
    public void Deactivate_Should_Set_Status_Inactive()
    {
        // Arrange
        var account = new Account("Arthur", "85025957095");

        // Act
        account.Deactivate();

        // Assert
        Assert.Equal(AccountStatus.Inactive, account.Status);
    }

    [Fact]
    public void Activate_Should_Set_Status_Active()
    {
        // Arrange
        var account = new Account("Arthur", "85025957095");
        account.Deactivate();

        // Act
        account.Activate();

        // Assert
        Assert.Equal(AccountStatus.Active, account.Status);
    }
    
    [Fact]
    public void Should_Map_All_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        const string holderName = "Arthur";
        const string cpf = "85025957095";
        const int status = (int)AccountStatus.Inactive;

        // Act
        var account = new Account(id, holderName, cpf, status);

        // Assert
        Assert.Equal(Guid.Parse(id), account.Id);
        Assert.Equal(AccountStatus.Inactive, account.Status);
        Assert.Equal(holderName, account.HolderName.Value);
        Assert.Equal(cpf, account.Cpf.NormalizedValue);
    }

    [Fact]
    public void Constructor_Should_Generate_New_Guid()
    {
        // Arrange & Act
        var account1 = new Account("Arthur", "85025957095");
        var account2 = new Account("Arthur", "85025957095");

        // Assert
        Assert.NotEqual(account1.Id, account2.Id);
    }
}