using KRTBank.Domain.Exceptions;
using KRTBank.Domain.ValueObjects;

namespace KRTBank.Tests.Domain.ValueObjects;

public class CpfTests
{
    [Fact]
    public void Constructor_Should_Create_Cpf_When_Valid()
    {
        // Arrange
        const string cpf = "52998224725";

        // Act
        var result = new Cpf(cpf);

        // Assert
        Assert.Equal("52998224725", result.NormalizedValue);
        Assert.Equal("529.982.247-25", result.FormattedValue);
    }

    [Fact]
    public void Constructor_Should_Normalize_Cpf_With_Mask()
    {
        // Arrange
        const string cpf = "529.982.247-25";

        // Act
        var result = new Cpf(cpf);

        // Assert
        Assert.Equal("52998224725", result.NormalizedValue);
        Assert.Equal("529.982.247-25", result.FormattedValue);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Cpf_Is_Null()
    {
        Assert.Throws<DomainException>(() => new Cpf(null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Cpf_Is_Empty()
    {
        Assert.Throws<DomainException>(() => new Cpf(""));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Cpf_Is_Whitespace()
    {
        Assert.Throws<DomainException>(() => new Cpf("   "));
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    [InlineData("52998224724")]
    public void Constructor_Should_Throw_When_Cpf_Is_Invalid(string cpf)
    {
        var ex = Assert.Throws<DomainException>(() => new Cpf(cpf));
        Assert.Equal("Invalid CPF.", ex.Message);
    }

    [Fact]
    public void Equals_And_Equality_Operator_Should_Return_True_For_Same_Cpf_Value()
    {
        // Arrange
        var cpf1 = new Cpf("52998224725");
        var cpf2 = new Cpf("529.982.247-25");

        // Act
        var equalsResult = cpf1.Equals(cpf2);
        var operatorResult = cpf1 == cpf2;

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
    }

    [Fact]
    public void Equals_And_Equality_Operator_Should_Return_False_For_Different_Cpf()
    {
        // Arrange
        var cpf1 = new Cpf("52998224725");
        var cpf2 = new Cpf("11144477735");

        // Act
        var equalsResult = cpf1.Equals(cpf2);
        var operatorResult = cpf1 == cpf2;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
    }

    [Fact]
    public void GetHashCode_Should_Be_Same_For_Equal_Objects()
    {
        // Arrange
        var cpf1 = new Cpf("52998224725");
        var cpf2 = new Cpf("529.982.247-25");

        // Act & Assert
        Assert.Equal(cpf1.GetHashCode(), cpf2.GetHashCode());
    }

    [Fact]
    public void Implicit_Operator_Should_Return_NormalizedValue()
    {
        // Arrange
        var cpf = new Cpf("529.982.247-25");

        // Act
        string value = cpf;

        // Assert
        Assert.Equal("52998224725", value);
    }

    [Fact]
    public void ToString_Should_Return_NormalizedValue()
    {
        // Arrange
        var cpf = new Cpf("52998224725");

        // Act
        var text = cpf.ToString();

        // Assert
        Assert.Equal("52998224725", text);
    }
}
