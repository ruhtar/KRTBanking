using KRTBank.Domain.ResultPattern;
using Microsoft.AspNetCore.Http;

namespace KRTBank.Tests.Domain.Result;

public class ResultTests
{
     [Fact]
        public void Result_Ok_Should_Set_Success_True()
        {
            // Act
            var result = KRTBank.Domain.ResultPattern.Result.Ok("Success");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal(200, result.Code);
        }

        [Fact]
        public void Result_Fail_Should_Set_Success_False()
        {
            // Act
            var result = KRTBank.Domain.ResultPattern.Result.Fail("Error occurred", StatusCodes.Status400BadRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error occurred", result.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, result.Code);
        }

        [Fact]
        public void ResultT_Ok_Should_Set_Data_And_Success_True()
        {
            // Arrange
            const string data = "Hello World";

            // Act
            var result = Result<string>.Ok("Success", data);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal(200, result.Code);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ResultT_Fail_Should_Set_Data_And_Success_False()
        {
            // Arrange
            const int data = 123;

            // Act
            var result = Result<int>.Fail("Failed", StatusCodes.Status500InternalServerError, data);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed", result.Message);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.Code);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void ResultT_Fail_Should_Allow_Null_Data()
        {
            // Act
            var result = Result<string>.Fail("Error");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public void Result_Ok_Should_Allow_Null_Message_And_Default_Code()
        {
            // Act
            var result = KRTBank.Domain.ResultPattern.Result.Ok();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.Equal(StatusCodes.Status200OK, result.Code);
        }
}