using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Enums;
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
}