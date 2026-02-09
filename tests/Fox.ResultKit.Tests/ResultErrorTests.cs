//==================================================================================================
// ResultError unit tests - Testing the ResultError utility class.
// Test suite for convention-based error code parsing and formatting.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// ResultError utility unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultErrorTests
{
    [Fact]
    public void Create_should_format_code_and_message()
    {
        var error = ResultError.Create("USER_NOT_FOUND", "User does not exist");

        error.Should().Be("USER_NOT_FOUND: User does not exist");
    }

    [Fact]
    public void Create_should_throw_when_code_is_null()
    {
        var act = () => ResultError.Create(null!, "message");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_throw_when_code_is_empty()
    {
        var act = () => ResultError.Create("", "message");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_throw_when_code_is_whitespace()
    {
        var act = () => ResultError.Create("   ", "message");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_throw_when_message_is_null()
    {
        var act = () => ResultError.Create("CODE", null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_throw_when_message_is_empty()
    {
        var act = () => ResultError.Create("CODE", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_throw_when_message_is_whitespace()
    {
        var act = () => ResultError.Create("CODE", "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_should_extract_code_and_message()
    {
        var (code, message) = ResultError.Parse("USER_NOT_FOUND: User does not exist");

        code.Should().Be("USER_NOT_FOUND");
        message.Should().Be("User does not exist");
    }

    [Fact]
    public void Parse_should_handle_plain_message_without_code()
    {
        var (code, message) = ResultError.Parse("Simple error message");

        code.Should().Be("");
        message.Should().Be("Simple error message");
    }

    [Fact]
    public void Parse_should_trim_whitespace_around_separator()
    {
        var (code, message) = ResultError.Parse("ERROR_CODE   :   Error message");

        code.Should().Be("ERROR_CODE");
        message.Should().Be("Error message");
    }

    [Fact]
    public void Parse_should_handle_message_with_multiple_colons()
    {
        var (code, message) = ResultError.Parse("ERROR_CODE: Message with: multiple: colons");

        code.Should().Be("ERROR_CODE");
        message.Should().Be("Message with: multiple: colons");
    }

    [Fact]
    public void Parse_should_throw_when_error_is_null()
    {
        var act = () => ResultError.Parse(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_should_throw_when_error_is_empty()
    {
        var act = () => ResultError.Parse("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_should_throw_when_error_is_whitespace()
    {
        var act = () => ResultError.Parse("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_and_Parse_should_roundtrip()
    {
        var originalCode = "USER_EMAIL_EXISTS";
        var originalMessage = "Email already exists";

        var error = ResultError.Create(originalCode, originalMessage);
        var (code, message) = ResultError.Parse(error);

        code.Should().Be(originalCode);
        message.Should().Be(originalMessage);
    }

    [Fact]
    public void Parse_should_handle_numeric_error_codes()
    {
        var (code, message) = ResultError.Parse("404: Not found");

        code.Should().Be("404");
        message.Should().Be("Not found");
    }

    [Fact]
    public void Parse_should_handle_hierarchical_error_codes()
    {
        var (code, message) = ResultError.Parse("VALIDATION.EMAIL.FORMAT: Invalid email format");

        code.Should().Be("VALIDATION.EMAIL.FORMAT");
        message.Should().Be("Invalid email format");
    }
}
