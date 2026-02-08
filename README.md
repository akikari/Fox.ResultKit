# Fox.ResultKit

[![Build Status](https://github.com/akikari/Fox.ResultKit/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/akikari/Fox.ResultKit/actions/workflows/build-and-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Fox.ResultKit.svg)](https://www.nuget.org/packages/Fox.ResultKit/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Fox.ResultKit.svg)](https://www.nuget.org/packages/Fox.ResultKit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight, type-safe result handling library for .NET applications that eliminates the need for exception-based error handling in business logic.

## 📋 Overview

Fox.ResultKit provides a clean and functional approach to handle operation results in .NET applications. Instead of throwing exceptions for expected failures, it uses the `Result` and `Result<T>` types to explicitly model success and failure states.

## 💡 Why Fox.ResultKit?

### Lightweight Alternative to FluentResults
- **Zero dependencies** - No third-party packages, minimal footprint
- **Simple API** - Just `Result<T>` and extension methods, nothing more
- **Perfect for domain logic** - Clean separation of business rules and infrastructure

### Railway Oriented Programming for C#
- **Inspired by F# Result type** and Scott Wlaschin's Railway Oriented Programming pattern
- **Functional composition** - Chain operations with `Map`, `Bind`, `Ensure`, `Tap`, `Match`
- **Exception safety** - Wrap unsafe code with `Try`/`TryAsync`

### Modern .NET Design
- **Nullable reference types** - Full null-safety support
- **Multi-targeting** - Works on .NET 8, 9, and 10
- **XML documentation** - Complete IntelliSense experience
- **Async-first** - All functional extensions have async variants

## 🎯 When to Use

### ✅ Use Fox.ResultKit when:
- **Modeling expected failures** - Validation errors, business rule violations, "not found" scenarios
- **Building clean domain logic** - Avoid try-catch noise in business layer
- **Explicit error handling** - Method signatures should reveal possible failures
- **Composing operations** - Chain multiple operations with functional style
- **Eliminating null checks** - Replace nullable return values with explicit Result

### ❌ Don't use for:
- **Truly exceptional situations** - Out of memory, stack overflow, hardware failures
- **Simple CRUD operations** - If EF Core's built-in exception handling is sufficient
- **Performance-critical hot paths** - Result allocation has small overhead (though minimal)
- **External API integration** - If you need typed error codes/HTTP status mapping (consider FluentResults)

## 🔄 Comparison with Alternatives

| Feature | Fox.ResultKit | FluentResults | CSharpFunctionalExtensions | LanguageExt |
|---------|-----------|---------------|----------------------------|-------------|
| **Dependencies** | 0 | 2+ | 0 | 10+ |
| **Learning Curve** | ⭐ Easy | ⭐⭐ Moderate | ⭐⭐ Moderate | ⭐⭐⭐ Steep |
| **API Complexity** | Simple | Feature-rich | Moderate | Complex |
| **Functional Extensions** | ✅ Map, Bind, Match | ✅ Full | ✅ Full | ✅ Full + Monads |
| **Multiple Errors** | ❌ Single | ✅ List | ✅ List | ✅ NEL |
| **Typed Errors** | ❌ String only | ✅ Custom | ✅ Custom | ✅ Custom |
| **Async Support** | ✅ Full | ✅ Full | ✅ Full | ✅ Full |
| **Best For** | Clean domain logic | Rich validation | DDD projects | FP enthusiasts |

**Fox.ResultKit's niche**: If you want **Railway Oriented Programming without the complexity** of LanguageExt or the feature bloat of FluentResults, Fox.ResultKit is the sweet spot.

## ✨ Features

- **Type-safe result handling** - Explicit success/failure modeling
- **Generic support** - `Result<T>` for operations returning values
- **Exception handling utilities** - `Try`, `TryAsync` for safe execution
- **Result composition** - Combine multiple results
- **Lightweight** - Zero external dependencies
- **Modern .NET** - Supports .NET 8, 9, and 10
- **Nullable reference types** - Full nullable annotation support
- **XML documentation** - Complete IntelliSense support

## 🚀 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Fox.ResultKit
```

Or via Package Manager Console:

```powershell
Install-Package Fox.ResultKit
```

### Fox.ResultKit.MediatR Integration

For MediatR integration (CQRS pipeline behavior):

[![NuGet](https://img.shields.io/nuget/v/Fox.ResultKit.MediatR.svg)](https://www.nuget.org/packages/Fox.ResultKit.MediatR/)

```bash
dotnet add package Fox.ResultKit.MediatR
```

See [Fox.ResultKit.MediatR documentation](src/Fox.ResultKit.MediatR/README.md) for usage details.

## 📖 Usage

### Basic Result

```csharp
using Fox.ResultKit;

// Creating results
Result success = Result.Success();
Result failure = Result.Failure("Operation failed");

// Imperative style (property-based)
if (result.IsSuccess)
{
    Console.WriteLine("Success!");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}

// Functional style (recommended)
result.Match(
    onSuccess: () => Console.WriteLine("Success!"),
    onFailure: error => Console.WriteLine($"Error: {error}")
);
```

### Generic Result

```csharp
// Creating results with values
Result<int> success = Result<int>.Success(42);
Result<int> failure = Result<int>.Failure("Invalid input");

// Imperative style (property-based)
if (result.IsSuccess)
{
    Console.WriteLine($"Value: {result.Value}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}

// Functional style (recommended)
string output = result.Match(
    onSuccess: value => $"Value: {value}",
    onFailure: error => $"Error: {error}"
);

Console.WriteLine(output);
```

### Railway Oriented Programming Pipeline

```csharp
// Functional composition with Map, Bind, Ensure, Tap
var result = await repository.FindByIdAsync(userId)
    .ToResult($"User {userId} not found")
    .Ensure(user => user.IsActive, "User is not active")
    .Map(user => new UserDto(user.Id, user.Email, user.IsActive))
    .Tap(dto => logger.LogInformation("Retrieved user: {Email}", dto.Email));

// Pattern matching for HTTP responses
return result.Match<IActionResult>(
    onSuccess: dto => Ok(dto),
    onFailure: error => NotFound(error)
);
```

### Validation with Combine

```csharp
// Functional validation pipeline
private static Result ValidateEmail(string email) =>
    Result.Success()
        .Ensure(() => !string.IsNullOrWhiteSpace(email), "Email is required")
        .Ensure(() => email.Contains("@"), "Invalid email format");

private static Result ValidatePassword(string password) =>
    Result.Success()
        .Ensure(() => !string.IsNullOrWhiteSpace(password), "Password is required")
        .Ensure(() => password.Length >= 8, "Password must be at least 8 characters");

// Combine multiple validations
var validationResult = ResultCombineExtensions.Combine(
    ValidateEmail(email),
    ValidatePassword(password)
);

// Convert to Result<T> with value if all validations pass
var result = validationResult.ToResult(new CreateUserRequest(email, password));
```

### Exception Handling with Try

```csharp
// Wrap unsafe operations
var result = ResultTryExtensions.Try(() => 
    int.Parse("123"), 
    "Failed to parse number"
);

// Async version
var result = await ResultTryExtensions.TryAsync(async () => 
    await GetDataFromApiAsync(), 
    "Failed to fetch data"
);

// Or use in a pipeline with Bind
var parsedResult = inputString
    .ToResult("Input is null")
    .Bind(input => ResultTryExtensions.Try(
        () => int.Parse(input), 
        "Invalid number format"
    ));
```

### Converting to Result

```csharp
// From nullable
string? value = GetValue();
Result<string> result = value.ToResult("Value was null");

// From exception
try 
{
    PerformOperation();
}
catch (Exception ex)
{
    Result<Data> result = ResultExceptionExtensions.FromException<Data>(ex);
}
```

### Real-world Example: Service Layer

```csharp
public class UserService(IUserRepository repository, ILogger<UserService> logger)
{
    public async Task<Result<Guid>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        return await ValidateUserInput(email, password)
            .ToResult((Email: email, Password: password))
            .EnsureAsync(_ => CheckEmailNotExistsAsync(email, cancellationToken), "Email already exists")
            .BindAsync(credentials => CreateAndSaveUserAsync(credentials.Email, credentials.Password, cancellationToken))
            .TapAsync(user => Task.Run(() => logger.LogInformation("User created: {UserId} - {Email}", user.Id, user.Email)))
            .TapFailureAsync(error => Task.Run(() => logger.LogError("User creation failed: {Error}", error)))
            .MapAsync(user => Task.FromResult(user.Id));
    }
    
    public Result ValidateUserInput(string email, string password)
    {
        return ResultCombineExtensions.Combine(
            ValidateEmail(email),
            ValidatePassword(password));
    }
    
    private static Result ValidateEmail(string email)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(email), "Email is required")
            .Ensure(() => email.Contains('@'), "Invalid email format");
    }
    
    private static Result ValidatePassword(string password)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(password), "Password is required")
            .Ensure(() => password.Length >= 8, "Password must be at least 8 characters");
    }
    
    private async Task<bool> CheckEmailNotExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await repository.FindByEmailAsync(email, cancellationToken) == null;
    }
    
    private async Task<Result<User>> CreateAndSaveUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new User(email, password);
        await repository.AddAsync(user, cancellationToken);
        return Result<User>.Success(user);
    }
}

// Controller using the service
[HttpPost]
[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(request);

    return (await userService.CreateUserAsync(request.Email, request.Password, cancellationToken))
        .Match<Guid, IActionResult>(
            onSuccess: userId => Ok(new { userId }),
            onFailure: error => BadRequest(new { error })
        );
}
```

## 🏗️ API Reference

### Result

| Member | Description |
|--------|-------------|
| `Result.Success()` | Creates a successful result |
| `Result.Failure(string error)` | Creates a failed result with error message |
| `IsSuccess` | Returns true if operation succeeded |
| `IsFailure` | Returns true if operation failed |
| `Error` | Error message (null if success) |
| `ThrowIfFailure()` | Throws exception if result is failure |

### Result&lt;T&gt;

| Member | Description |
|--------|-------------|
| `Result<T>.Success(T value)` | Creates a successful result with value |
| `Result<T>.Failure(string error)` | Creates a failed result with error message |
| `IsSuccess` | Returns true if operation succeeded |
| `IsFailure` | Returns true if operation failed |
| `Value` | The result value (throws if failure) |
| `Error` | Error message (null if success) |
| `ThrowIfFailure()` | Throws exception if result is failure |

### Railway Oriented Programming Extensions

#### Transformation

| Method | Description |
|--------|-------------|
| `Map<T, U>(this Result<T>, Func<T, U>)` | Transforms success value to new type |
| `MapAsync<T, U>(this Result<T>, Func<T, Task<U>>)` | Async transform |
| `MapAsync<T, U>(this Task<Result<T>>, Func<T, Task<U>>)` | Async result to async transform |

#### Chaining

| Method | Description |
|--------|-------------|
| `Bind<T, U>(this Result<T>, Func<T, Result<U>>)` | Chains operations that return Result |
| `BindAsync<T, U>(this Result<T>, Func<T, Task<Result<U>>>)` | Async bind |
| `BindAsync<T, U>(this Task<Result<T>>, Func<T, Task<Result<U>>>)` | Async result to async bind |

#### Validation

| Method | Description |
|--------|-------------|
| `Ensure<T>(this Result<T>, Func<T, bool>, string)` | Validates result value with predicate |
| `Ensure(this Result, Func<bool>, string)` | Stateless validation |
| `EnsureAsync<T>(this Result<T>, Func<T, Task<bool>>, string)` | Async validation |
| `EnsureAsync<T>(this Task<Result<T>>, Func<T, Task<bool>>, string)` | Async result to async validation |

#### Side Effects

| Method | Description |
|--------|-------------|
| `Tap<T>(this Result<T>, Action<T>)` | Executes action on success (logging, etc.) |
| `TapAsync<T>(this Result<T>, Func<T, Task>)` | Async tap on success |
| `TapAsync<T>(this Task<Result<T>>, Func<T, Task>)` | Async result to async tap |
| `TapFailure<T>(this Result<T>, Action<string>)` | Executes action on failure |
| `TapFailureAsync<T>(this Result<T>, Func<string, Task>)` | Async tap on failure |
| `TapFailureAsync<T>(this Task<Result<T>>, Func<string, Task>)` | Async result to async tap failure |

#### Pattern Matching

| Method | Description |
|--------|-------------|
| `Match<T, U>(this Result<T>, Func<T, U>, Func<string, U>)` | Handles both success and failure cases |
| `MatchAsync<T, U>(this Task<Result<T>>, Func<T, Task<U>>, Func<string, Task<U>>)` | Async pattern matching |

### Conversion Extensions

| Method | Description |
|--------|-------------|
| `ToResult<T>(this Result<T>)` | Converts Result&lt;T&gt; to Result (discards value) |
| `ToResult<T>(this Result, T value)` | Converts Result to Result&lt;T&gt; with value |
| `ToResult<T>(this T? value, string errorIfNull)` | Converts nullable to Result&lt;T&gt; |

### Combination Extensions

| Method | Description |
|--------|-------------|
| `Combine(params Result[])` | Combines multiple results, fails on first error |
| `Combine<T>(T value, params Result[])` | Combines results and returns value if all succeed |
| `Combine<T>(params Result<T>[])` | Combines generic results, returns last value |

### Exception Handling Extensions

| Method | Description |
|--------|-------------|
| `Try<T>(Func<T>, string error)` | Wraps function execution, catches exceptions |
| `TryAsync<T>(Func<Task<T>>, string error)` | Async version of Try |
| `FromException<T>(Exception ex)` | Converts exception to Result&lt;T&gt; |
| `FromException<T>(Exception ex, bool includeInner, bool includeStack)` | Detailed exception conversion |

## 🎯 Design Principles

1. **Explicit over implicit** - Make success and failure explicit in the type system
2. **Railway-oriented programming** - Enable fluent result composition
3. **Zero overhead** - No external dependencies, minimal allocations
4. **Developer-friendly** - Clear API, excellent IntelliSense support

## 🔧 Requirements

- .NET 8.0 or higher
- C# 11 or higher (for file-scoped namespaces and modern features)
- Nullable reference types enabled (recommended)

## 🤝 Contributing

**Fox.ResultKit is intentionally lightweight and feature-focused.** The goal is to remain a simple, zero-dependency library for Railway Oriented Programming.

### What We Welcome

- ✅ **Bug fixes** - Issues with existing functionality
- ✅ **Documentation improvements** - Clarifications, examples, typo fixes
- ✅ **Performance optimizations** - Without breaking API compatibility

### What We Generally Do Not Accept

- ❌ New dependencies or third-party packages
- ❌ Large feature additions that increase complexity
- ❌ Breaking API changes

If you want to propose a significant change, please open an issue first to discuss whether it aligns with the project's philosophy.

### Build Policy

The project enforces a **strict build policy** to ensure code quality:

- ❌ **No errors allowed** - Build must be error-free
- ❌ **No warnings allowed** - All compiler warnings must be resolved
- ❌ **No messages allowed** - Informational messages must be suppressed or addressed

All pull requests must pass this requirement.

### Code Style

- Follow the existing code style (see `.github/copilot-instructions.md`)
- Use file-scoped namespaces
- Enable nullable reference types
- Add XML documentation for public APIs
- Write unit tests for new features

### How to Contribute

1. Fork the repository
2. Create a feature branch from `main`
3. Follow the coding standards in `.github/copilot-instructions.md`
4. Ensure all tests pass
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 👤 Author

**Károly Akácz**

- GitHub: [@akikari](https://github.com/akikari)
- Repository: [Fox.ResultKit](https://github.com/akikari/Fox.ResultKit)

## 📊 Project Status

Current version: **1.0.0**

See [CHANGELOG.md](CHANGELOG.md) for version history.

## 📞 Support

For issues, questions, or feature requests, please open an issue in the [GitHub repository](https://github.com/akikari/Fox.ResultKit/issues).
