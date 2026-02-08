# ResultKit.MediatR

MediatR integration for ResultKit - Railway Oriented Programming pipeline behaviors for CQRS commands and queries.

## 📋 Overview

ResultKit.MediatR provides seamless integration between [MediatR](https://github.com/jbogard/MediatR) and [ResultKit](https://github.com/akikari/ResultKit), enabling automatic exception handling and result-based error handling in your CQRS command/query handlers.

## ✨ Features

- **Automatic Exception Handling** - Exceptions in handlers are automatically converted to `Result.Failure`
- **Type-safe Pipeline Behavior** - Works with both `Result` and `Result<T>` responses
- **Zero Configuration** - Single extension method to enable
- **Fluent Integration** - Seamlessly integrates with existing MediatR setup

## 🚀 Installation

This is a **private NuGet package**. Configure your NuGet source first:

```bash
dotnet nuget add source https://your-private-feed-url --name PrivateFeed
```

Then install both packages:

```bash
dotnet add package ResultKit
dotnet add package ResultKit.MediatR
```

## 📖 Usage

### 1. Register MediatR + ResultKit Pipeline

```csharp
using ResultKit.MediatR;

var builder = WebApplication.CreateBuilder(args);

// Register MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssemblyContaining<Program>());

// Add ResultKit pipeline behavior
builder.Services.AddResultKitMediatR();
```

### 2. Write Handlers that Return Result

```csharp
using MediatR;
using ResultKit;

// Command
public record CreateUserCommand(string Email, string Password) : IRequest<Result<Guid>>;

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<Guid>.Failure("Email is required");

        if (!request.Email.Contains("@"))
            return Result<Guid>.Failure("Invalid email format");

        // Business logic (exceptions auto-handled by pipeline)
        var user = new User(request.Email, request.Password);
        await _repository.AddAsync(user, cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
```

### 3. Use in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return Ok(new { userId = result.Value });

        return BadRequest(new { error = result.Error });
    }
}
```

## 🎯 How It Works

### Without ResultKit.MediatR

```csharp
public async Task<Result<User>> Handle(GetUserQuery request, CancellationToken ct)
{
    try
    {
        var user = await _repository.FindByIdAsync(request.Id, ct);
        return user != null 
            ? Result<User>.Success(user)
            : Result<User>.Failure("User not found");
    }
    catch (Exception ex)
    {
        return Result<User>.Failure($"Database error: {ex.Message}");
    }
}
```

### With ResultKit.MediatR (Cleaner!)

```csharp
public async Task<Result<User>> Handle(GetUserQuery request, CancellationToken ct)
{
    // Exceptions automatically caught and converted to Result.Failure
    var user = await _repository.FindByIdAsync(request.Id, ct);
    
    return user != null 
        ? Result<User>.Success(user)
        : Result<User>.Failure("User not found");
}
```

The pipeline behavior automatically catches exceptions and converts them to `Result.Failure(ex.Message)`.

## 📚 Advanced Usage

### Custom Error Handling

If you need custom exception handling, you can still catch specific exceptions:

```csharp
public async Task<Result<User>> Handle(GetUserQuery request, CancellationToken ct)
{
    try
    {
        var user = await _repository.FindByIdAsync(request.Id, ct);
        return Result<User>.Success(user);
    }
    catch (UserNotFoundException ex)
    {
        return Result<User>.Failure($"User {request.Id} not found");
    }
    // Other exceptions still handled by pipeline
}
```

### Combining with ResultKit Extensions

```csharp
public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken ct)
{
    return await _repository.FindByIdAsync(request.Id, ct)
        .ToResult($"User {request.Id} not found")
        .MapAsync(user => new UserDto(user.Name, user.Email))
        .TapAsync(dto => _logger.LogInformation("Retrieved user: {Name}", dto.Name));
}
```

## 🏗️ API Reference

### ServiceCollectionExtensions

| Method | Description |
|--------|-------------|
| `AddResultKitMediatR(this IServiceCollection)` | Registers the ResultKit pipeline behavior |

### ResultPipelineBehavior&lt;TRequest, TResponse&gt;

Automatically registered pipeline behavior that:
- Catches unhandled exceptions in handlers
- Converts them to `Result.Failure(exception.Message)`
- Only applies to handlers returning `Result` or `Result<T>`

## 🔧 Requirements

- .NET 8.0 or higher
- MediatR 12.0 or higher
- ResultKit 1.0 or higher

## 📝 License

Copyright (c) 2026 Károly Akácz. All rights reserved.

This is proprietary software for internal use only.

## 👤 Author

**Károly Akácz**

- GitHub: [@akikari](https://github.com/akikari)
- Repository: [ResultKit.MediatR](https://github.com/akikari/ResultKit.MediatR)

## 🔗 Related Packages

- [ResultKit](https://github.com/akikari/ResultKit) - Core result handling library
- [MediatR](https://github.com/jbogard/MediatR) - Simple mediator implementation in .NET

---

**Note:** This is a private NuGet package. Ensure you have proper access credentials configured.
