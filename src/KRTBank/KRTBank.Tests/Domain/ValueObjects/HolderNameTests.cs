using KRTBank.Domain.Exceptions;
using KRTBank.Domain.ValueObjects;

namespace KRTBank.Tests.Domain.ValueObjects;

public class HolderNameTests
{
    [Fact]
    public void Constructor_Should_Create_HolderName_When_Valid()
    {
        // Arrange
        const string name = "Arthur Amorim";

        // Act
        var holderName = new HolderName(name);

        // Assert
        Assert.Equal(name, holderName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_Throw_When_Null_Or_Empty(string value)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new HolderName(value));

        // Assert
        Assert.Equal("HolderName is required.", ex.Message);
        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Ab")]
    public void Constructor_Should_Throw_When_Too_Short(string value)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new HolderName(value));

        // Assert
        Assert.Equal("HolderName is too short.", ex.Message);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void Constructor_Should_Trim_Value()
    {
        // Arrange
        const string value = "  Arthur  ";

        // Act
        var holderName = new HolderName(value);

        // Assert
        Assert.Equal("Arthur", holderName.Value);
    }

    [Fact]
    public void Equals_Should_Return_True_For_Same_Value()
    {
        var h1 = new HolderName("Arthur");
        var h2 = new HolderName("Arthur");

        Assert.True(h1.Equals(h2));
        Assert.True(h1 == h2);
        Assert.Equal(h1.GetHashCode(), h2.GetHashCode());
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Value()
    {
        var h1 = new HolderName("Arthur");
        var h2 = new HolderName("Carlos");

        Assert.False(h1.Equals(h2));
        Assert.False(h1 == h2);
    }

    [Fact]
    public void Implicit_Operator_Should_Return_String_Value()
    {
        // Arrange
        var holderName = new HolderName("Arthur");

        // Act
        string value = holderName;

        // Assert
        Assert.Equal("Arthur", value);
    }

    [Fact]
    public void ToString_Should_Return_Value()
    {
        var holderName = new HolderName("Arthur");

        Assert.Equal("Arthur", holderName.ToString());
    }
}
