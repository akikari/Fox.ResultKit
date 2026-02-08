# Fox.ResultKit

A lightweight, type-safe result handling library for .NET applications that eliminates the need for exception-based error handling in business logic.

## 📋 Overview

Fox.ResultKit provides a clean and functional approach to handle operation results in .NET applications. Instead of throwing exceptions for expected failures, it uses the `Result` and `Result<T>` types to explicitly model success and failure states.

Inspired by F# Result type and Scott Wlaschin's Railway Oriented Programming pattern, this library brings functional composition and type-safe error handling to C#.

## ✨ Features

- **Type-safe Result Handling** - Explicit success/failure modeling with `Result` and `Result<T>`
- **Railway Oriented Programming** - Functional composition with `Map`, `Bind`, `Ensure`, `Tap`, `Match`
- **Exception Safety** - `Try` and `TryAsync` for wrapping unsafe operations
- **Result Composition** - `Combine` multiple validation results
- **Zero Dependencies** - No third-party packages, minimal footprint
- **Nullable Reference Types** - Full null-safety support
- **Async-First Design** - All functional extensions have async variants
- **Multi-targeting** - Supports .NET 8, 9, and 10
- **XML Documentation** - Complete IntelliSense experience

## 🚀 Installation

```bash
dotnet add package Fox.ResultKit
```

## 📖 Basic Usage

### Creating Results

```csharp
using Fox.ResultKit;

// Non-generic Result
Result success = Result.Success();
Result failure = Result.Failure("Operation failed");

// Generic Result with value
Result<int> successValue = Result<int>.Success(42);
Result<int> failureValue = Result<int>.Failure("Invalid input");
```

### Pattern Matching

```csharp
var result = Result<int>.Success(10);

// Return value
string output = result.Match(
    onSuccess: value => $"Value: {value}",
    onFailure: error => $"Error: {error}"
);

// Void actions
result.Match(
    onSuccess: () => Console.WriteLine("Success!"),
    onFailure: error => Console.WriteLine($"Error: {error}")
);
```

### Railway Oriented Programming

```csharp
var result = await GetUserById(userId)
    .ToResult($"User {userId} not found")
    .Ensure(user => user.IsActive, "User is not active")
    .Map(user => new UserDto(user.Id, user.Email))
    .Tap(dto => logger.LogInformation("User: {Email}", dto.Email));

return result.Match<IActionResult>(
    onSuccess: dto => Ok(dto),
    onFailure: error => NotFound(error)
);
```

### Validation with Combine

```csharp
private Result ValidateEmail(string email) =>
    Result.Success()
        .Ensure(() => !string.IsNullOrWhiteSpace(email), "Email required")
        .Ensure(() => email.Contains("@"), "Invalid email");

private Result ValidatePassword(string password) =>
    Result.Success()
        .Ensure(() => password?.Length >= 8, "Min 8 characters");

// Combine validations
var validation = ResultCombineExtensions.Combine(
    ValidateEmail(email),
    ValidatePassword(password)
);
```

### Exception Handling

```csharp
// Sync
var result = ResultTryExtensions.Try(
    () => int.Parse("123"),
    "Parse failed"
);

// Async
var asyncResult = await ResultTryExtensions.TryAsync(
    async () => await httpClient.GetStringAsync(url),
    "Request failed"
);
```

## 🎯 When to Use

### ✅ Use Fox.ResultKit when:
- Modeling expected failures (validation, business rules, not found)
- Building clean domain logic without try-catch noise
- Explicit error handling in method signatures
- Composing operations functionally
- Eliminating null checks

### ❌ Don't use for:
- Truly exceptional situations (OOM, hardware failures)
- Performance-critical hot paths
- External APIs needing typed error codes

## 📦 Related Packages

- **[Fox.ResultKit.MediatR](../Fox.ResultKit.MediatR/README.md)** - MediatR integration for CQRS pipelines

## 📚 Documentation

For complete documentation and advanced examples, visit the [GitHub repository](https://github.com/akikari/Fox.ResultKit).

## 📄 License

MIT License - see [LICENSE](../../LICENSE.txt) for details.

## 🙏 Acknowledgments

- Inspired by [F# Result type](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-fresult-2.html)
- Based on [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) by Scott Wlaschin
- Influenced by [FluentResults](https://github.com/altmann/FluentResults) and [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
