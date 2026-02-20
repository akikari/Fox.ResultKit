# Contributing to Fox.ResultKit

Thank you for your interest in contributing to Fox.ResultKit! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## How to Contribute

### Reporting Issues

If you find a bug or have a feature request:

1. Check if the issue already exists in the [GitHub Issues](https://github.com/akikari/Fox.ResultKit/issues)
2. If not, create a new issue with:
   - Clear, descriptive title
   - Detailed description of the problem or feature
   - Steps to reproduce (for bugs)
   - Expected vs actual behavior
   - Code samples if applicable
   - Environment details (.NET version, OS, etc.)

### Submitting Changes

1. **Fork the repository** and create a new branch from `main`
2. **Make your changes** following the coding guidelines below
3. **Write or update tests** for your changes
4. **Update documentation** if needed (README, XML comments)
5. **Ensure all tests pass** (`dotnet test`)
6. **Ensure build succeeds** (`dotnet build`)
7. **Submit a pull request** with:
   - Clear description of changes
   - Reference to related issues
   - Summary of testing performed

## Coding Guidelines

Fox.ResultKit follows strict coding standards. Please review the [Copilot Instructions](.github/copilot-instructions.md) for detailed guidelines.

### Key Standards

#### General
- **Language**: All code, comments, and documentation must be in English
- **Line Endings**: CRLF
- **Indentation**: 4 spaces (no tabs)
- **Namespaces**: File-scoped (`namespace MyNamespace;`)
- **Nullable**: Enabled
- **Language Version**: latest

#### Naming Conventions
- **Private Fields**: camelCase without underscore prefix (e.g., `value`, not `_value`)
- **Public Members**: PascalCase
- **Local Variables**: camelCase

#### Code Style
- Use expression-bodied members for simple properties and methods
- Use auto-properties where possible
- Prefer `var` only when type is obvious
- Maximum line length: 100 characters
- Add blank line after closing brace UNLESS next line is also `}`

#### Documentation
- **XML Comments**: Required for all public APIs
- **Language**: English
- **Decorators**: 98 characters width using `//======` (no space after prefix)
- **File Headers**: 3-line header (purpose + technical description + decorators)

Example:
```csharp
//==================================================================================================
// Represents the result of an operation that can be either success or failure.
// Sealed class implementation for Result pattern with value.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Represents the result of an operation that can contain a value on success or an error on failure.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
//==================================================================================================
public sealed class Result<T> : IResult
{
    private readonly T? value;

    //==============================================================================================
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    //==============================================================================================
    public bool IsSuccess { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the result value. Throws if the result is a failure.
    /// </summary>
    //==============================================================================================
    public T Value => IsSuccess ? value! : throw new InvalidOperationException("Cannot access Value on a failed result.");

    //==============================================================================================
    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    //==============================================================================================
    public static Result<T> Success(T value) => new(true, value, null);
}
```

## Testing Requirements

- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Test Naming**: `MethodName_should_expected_behavior`
- **Coverage**: Aim for 100% coverage of new code
- **Test Structure**:
  - Arrange: Setup test data
  - Act: Execute the method under test
  - Assert: Verify expected behavior

Example:
```csharp
[Fact]
public void Success_should_create_success_result()
{
    // Arrange & Act
    var result = Result<int>.Success(42);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(42);
}

[Fact]
public void Map_should_transform_success_value()
{
    // Arrange
    var result = Result<int>.Success(42);

    // Act
    var mapped = result.Map(x => x.ToString());

    // Assert
    mapped.IsSuccess.Should().BeTrue();
    mapped.Value.Should().Be("42");
}
```

## Architecture Principles

Fox.ResultKit follows functional programming principles and SOLID design:

- **Single Responsibility**: Each type has one clear purpose (Result, Result<T>, ErrorsResult)
- **Open/Closed**: Open for extension via extension methods, closed for modification
- **Liskov Substitution**: IResult interface enables polymorphic operations
- **Interface Segregation**: Minimal, focused interfaces (IResult)
- **Dependency Inversion**: No dependencies on external frameworks

### Design Guidelines

- **Zero Dependencies**: No third-party packages in core library
- **Immutable Types**: Result types are immutable after creation
- **Explicit Over Implicit**: Method signatures reveal possible failures
- **Functional Composition**: Railway Oriented Programming with Map, Bind, Ensure, Tap
- **Type Safety**: Leverage C# type system to prevent errors at compile time

## Project Structure

```
Fox.ResultKit/
├── src/
│   ├── Fox.ResultKit/             # Core package
│   │   ├── Result.cs              # Non-generic Result type
│   │   ├── ResultT.cs             # Generic Result<T> type
│   │   ├── ErrorsResult.cs        # Multiple error collection
│   │   ├── IResult.cs             # Common interface
│   │   ├── ResultError.cs         # Convention-based error codes
│   │   └── Extensions/            # Railway Oriented Programming extensions
│   └── Fox.ResultKit.MediatR/     # MediatR integration
│       ├── ResultPipelineBehavior.cs  # Pipeline behavior
│       └── Extensions/            # DI extensions
├── tests/
│   ├── Fox.ResultKit.Tests/       # Core tests (169 tests)
│   └── Fox.ResultKit.MediatR.Tests/  # MediatR tests (16 tests)
└── samples/
    └── Fox.ResultKit.WebApi.Demo/ # ASP.NET Core demo (Classic + CQRS)
```

## Pull Request Process

1. **Update tests**: Ensure your changes are covered by tests
2. **Update documentation**: Keep README and XML comments up to date
3. **Follow coding standards**: Use provided `.editorconfig` and copilot instructions
4. **Keep commits clean**: 
   - Use clear, descriptive commit messages
   - Squash commits if needed before merging
5. **Update CHANGELOG.md**: Add entry under `[Unreleased]` section
6. **Ensure CI passes**: All tests must pass and build must succeed

### Commit Message Format

Use clear, imperative commit messages:

```
Add support for async conditional handlers

- Implement AddConditionalHandlerAsync method
- Add unit tests for async conditions
- Update documentation
```

## Feature Requests

When proposing new features, please consider:

1. **Scope**: Does this fit the focused nature of Fox.ChainKit?
2. **Complexity**: Does this add unnecessary complexity?
3. **Dependencies**: Does this require new external dependencies?
4. **Breaking Changes**: Will this break existing code?
5. **Use Cases**: What real-world scenarios does this address?

Fox.ResultKit aims to be lightweight and focused. Features should align with Railway Oriented Programming and the Result pattern.

## Development Setup

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022+ or Rider (recommended)
- Git

### Getting Started

1. Clone the repository:
```bash
git clone https://github.com/akikari/Fox.ResultKit.git
cd Fox.ResultKit
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

4. Run tests:
```bash
dotnet test
```

5. Run the sample application:
```bash
dotnet run --project samples/Fox.ResultKit.WebApi.Demo/Fox.ResultKit.WebApi.Demo.csproj
```

## Questions?

If you have questions about contributing, feel free to:
- Open a [GitHub Discussion](https://github.com/akikari/Fox.ResultKit/discussions)
- Create an issue labeled `question`
- Reach out to the maintainers

## License

By contributing to Fox.ResultKit, you agree that your contributions will be licensed under the MIT License.

Thank you for contributing to Fox.ResultKit! 🎉
