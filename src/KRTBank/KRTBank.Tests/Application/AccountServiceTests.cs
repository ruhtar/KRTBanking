using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Entities;
using KRTBank.Domain.Enums;
using KRTBank.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Moq;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _cacheMock = new Mock<ICacheService>();
        _service = new AccountService(_accountRepositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_From_Cache_When_Cache_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cachedDto = new AccountDto(id, "Arthur", "12345678901", AccountStatus.Active);

        _cacheMock
            .Setup(c => c.GetAsync<AccountDto>(id.ToString()))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(cachedDto, result.Data);

        _accountRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _cacheMock.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<AccountDto>()),
            Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_Should_Get_From_Repository_And_Set_Cache_When_Not_In_Cache()
    {
        // Arrange
        var account = new Account("Arthur Amorim", "85025957095");

        _cacheMock
            .Setup(c => c.GetAsync<AccountDto>(account.Id.ToString()))
            .ReturnsAsync((AccountDto?)null);

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _service.GetByIdAsync(account.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(account.Id, result.Data!.Id);
        Assert.Equal(account.HolderName, result.Data.HolderName);
        Assert.Equal(account.Cpf, result.Data.Cpf);

        _cacheMock.Verify(c =>
                c.SetAsync(account.Id.ToString(), result.Data),
            Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_Should_Return_404_When_Account_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _cacheMock
            .Setup(c => c.GetAsync<AccountDto>(id.ToString()))
            .ReturnsAsync((AccountDto?)null);

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(StatusCodes.Status404NotFound, result.Code);

        _cacheMock.Verify(c =>
                c.SetAsync(It.IsAny<string>(), It.IsAny<AccountDto>()),
            Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_Should_Fail_When_Cpf_Already_Exists()
    {
        // Arrange
        var dto = new CreateAccountDto(HolderName: "Arthur Amorim", Cpf:"85025957095");

        var existingAccount = new Account(dto.HolderName, dto.Cpf);

        _accountRepositoryMock
            .Setup(r => r.GetByCpfAsync(dto.Cpf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.Code);

        _accountRepositoryMock.Verify(r =>
                r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_Should_Create_Account_When_Data_Is_Valid()
    {
        // Arrange
        var dto = new CreateAccountDto(HolderName: "Arthur Amorim", Cpf: "85025957095");

        _accountRepositoryMock
            .Setup(r => r.GetByCpfAsync(dto.Cpf, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status201Created, result.Code);
        Assert.Equal(dto.HolderName, result.Data!.HolderName);
        Assert.Equal(dto.Cpf, result.Data.Cpf);

        _accountRepositoryMock.Verify(r =>
                r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_Should_Return_404_When_Account_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateAccountDto(HolderName: "Novo Nome", IsActive: true);

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, result.Code);

        _accountRepositoryMock.Verify(r =>
                r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _cacheMock.Verify(c =>
                c.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_HolderName_And_Remove_Cache()
    {
        // Arrange
        var account = new Account("Arthur Amorim", "85025957095");
        var dto = new UpdateAccountDto(HolderName: "Novo Nome", IsActive: null);

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _service.UpdateAsync(account.Id, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Novo Nome", account.HolderName.ToString());
        Assert.True(account.Status == AccountStatus.Active);

        _accountRepositoryMock.Verify(r =>
                r.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);

        _cacheMock.Verify(c =>
                c.RemoveAsync(account.Id.ToString()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Activate_Account_When_IsActive_Is_True_And_Remove_Cache()
    {
        // Arrange
        var account = new Account("Arthur Amorim", "85025957095");
        account.Deactivate(); // garante que começa inativa

        var dto = new UpdateAccountDto(HolderName: "Arthur Amorim", IsActive: true);

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _service.UpdateAsync(account.Id, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(account.Status == AccountStatus.Active); 

        _accountRepositoryMock.Verify(r =>
                r.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);

        _cacheMock.Verify(c =>
                c.RemoveAsync(account.Id.ToString()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Deactivate_Account_When_IsActive_Is_False_And_Remove_Cache()
    {
        // Arrange
        var account = new Account("Arthur Amorim", "85025957095");
        account.Activate(); // garante que começa ativa

        var dto = new UpdateAccountDto(HolderName: "Arthur Amorim", IsActive: false);

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _service.UpdateAsync(account.Id, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(account.Status == AccountStatus.Inactive);

        _accountRepositoryMock.Verify(r =>
                r.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);

        _cacheMock.Verify(c =>
                c.RemoveAsync(account.Id.ToString()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_404_When_Account_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, result.Code);
        Assert.Equal($"Account with id {id} was not found.", result.Message);

        _accountRepositoryMock.Verify(r =>
                r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _cacheMock.Verify(c =>
                c.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Account_And_Remove_Cache()
    {
        // Arrange
        var account = new Account("Arthur Amorim", "85025957095");

        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _service.DeleteAsync(account.Id);

        // Assert
        Assert.True(result.IsSuccess);

        _accountRepositoryMock.Verify(r =>
                r.DeleteAsync(account.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _cacheMock.Verify(c =>
                c.RemoveAsync(account.Id.ToString()),
            Times.Once);
    }
}