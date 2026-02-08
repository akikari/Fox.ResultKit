# Fox.ResultKit WebApi Demo

Demonstration of **Fox.ResultKit** and **Fox.ResultKit.MediatR** integration in ASP.NET Core.

This project showcases **two architectural approaches** to error handling:

1. **Classic Service Layer** (`/api/classic/*`) - Traditional service injection with Fox.ResultKit
2. **CQRS with MediatR** (`/api/cqrs/*`) - Command/Query separation with MediatR pipeline behaviors

## 🎯 What This Demo Shows

### ✅ Fox.ResultKit Features

- **Basic Result handling** - Success/Failure states
- **Generic Result&lt;T&gt;** - Type-safe value returns
- **Functional composition** - `Map`, `Bind`, `Ensure`, `Tap`, `Match` extensions
- **Result combination** - `Combine` multiple validations
- **Exception handling** - `Try`/`TryAsync` wrappers

### ✅ MediatR Integration

- **Pipeline behaviors** - Automatic exception → Result.Failure conversion
- **CQRS pattern** - Commands (writes) vs Queries (reads)
- **Clean handlers** - No try-catch boilerplate in handlers

## 🚀 Running the Demo

```bash
cd samples/Fox.ResultKit.WebApi.Demo
dotnet run
```

Navigate to: `https://localhost:7xxx/swagger`

## 📖 API Endpoints

### Classic Service Layer (`/api/classic/users`)

| Method | Endpoint | Description | Fox.ResultKit Features |
|--------|----------|-------------|-------------------|
| POST | `/api/classic/users` | Create user | Basic Result, Combine validation |
| GET | `/api/classic/users/{id}` | Get user DTO | Map, Ensure, Tap pipeline |
| PATCH | `/api/classic/users/{id}/status` | Update status | Bind chain |
| GET | `/api/classic/users/{id}/summary` | Get summary | Full pipeline + Match |

### CQRS with MediatR (`/api/cqrs/users`)

| Method | Endpoint | Description | MediatR Feature |
|--------|----------|-------------|-----------------|
| POST | `/api/cqrs/users` | Create user (command) | CreateUserCommand |
| GET | `/api/cqrs/users/{id}` | Get user (query) | GetUserQuery + pipeline |
| GET | `/api/cqrs/users` | List users (query) | ListUsersQuery + Map |

## 🔍 Code Examples

### 1. Basic Result Usage (Classic)

**Controller:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var result = await userService.CreateUserAsync(request.Email, request.Password);

    if (result.IsSuccess)
        return Ok(new { userId = result.Value });

    return BadRequest(new { error = result.Error });
}
```

**Service:**
```csharp
public async Task<Result<Guid>> CreateUserAsync(string email, string password)
{
    var validationResult = ValidateUserInput(email, password);
    if (validationResult.IsFailure)
        return Result<Guid>.Failure(validationResult.Error!);

    var user = new User(email, password);
    await repository.AddAsync(user);

    return Result<Guid>.Success(user.Id);
}
```

### 2. Functional Pipeline (Classic)

**GET `/api/classic/users/{id}` - Map, Ensure, Tap:**

```csharp
public async Task<Result<UserDto>> GetUserDtoAsync(Guid userId)
{
    var user = await repository.FindByIdAsync(userId);

    return Result<User>.Success(user!)
        .Ensure(u => u != null, $"User {userId} not found")
        .Ensure(u => u!.IsActive, "User is not active")
        .Map(u => new UserDto(u!.Id, u.Email, u.IsActive, u.CreatedAt))
        .Tap(dto => logger.LogInformation("Retrieved: {Email}", dto.Email));
}
```

**GET `/api/classic/users/{id}/summary` - Full Pipeline with Match:**

```csharp
public async Task<string> GetUserSummaryAsync(Guid userId)
{
    return await FindUserResult(userId)
        .EnsureAsync(user => Task.FromResult(user.IsActive), "User is inactive")
        .MapAsync(user => Task.FromResult($"{user.Email} (Active since {user.CreatedAt:yyyy-MM-dd})"))
        .TapAsync(summary => Task.Run(() => logger.LogInformation("Summary: {Summary}", summary)))
        .MatchAsync(
            onSuccess: summary => Task.FromResult(summary),
            onFailure: error => Task.FromResult($"Error: {error}")
        );
}
```

### 3. CQRS with MediatR

**Controller:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var command = new CreateUserCommand(request.Email, request.Password);
    var result = await mediator.Send(command);

    return result.Match(
        onSuccess: userId => CreatedAtAction(nameof(GetUser), new { id = userId }, new { userId }),
        onFailure: error => BadRequest(new { error })
    );
}
```

**Handler (no try-catch needed!):**
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<Guid>.Failure("Email is required");

        // Business logic (exceptions auto-handled by ResultPipelineBehavior!)
        var user = new User(request.Email, request.Password);
        await repository.AddAsync(user, ct);

        return Result<Guid>.Success(user.Id);
    }
}
```

### 4. Functional Pipeline in MediatR Handler

**GetUserQuery Handler:**
```csharp
public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken ct)
{
    var user = await repository.FindByIdAsync(request.UserId, ct);

    return Result<User>.Success(user!)
        .Ensure(u => u != null, $"User {request.UserId} not found")
        .Ensure(u => u!.IsActive, "User is not active")
        .MapAsync(u => Task.FromResult(new UserDto(u!.Id, u.Email, u.IsActive, u.CreatedAt)))
        .TapAsync(dto => Task.Run(() => logger.LogInformation("Retrieved: {Email}", dto.Email)))
        .Result;
}
```

## 🧪 Testing with Swagger

### Example 1: Create User (Classic)

**Request:**
```http
POST /api/classic/users
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Success Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Failure Response:**
```json
{
  "error": "Invalid email format"
}
```

### Example 2: Get User (CQRS)

**Request:**
```http
GET /api/cqrs/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Success Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john@example.com",
  "isActive": true,
  "createdAt": "2026-02-06T14:30:00Z"
}
```

**Failure Response (404):**
```json
{
  "error": "User 3fa85f64-5717-4562-b3fc-2c963f66afa6 not found"
}
```

## 📚 Architecture Overview

### Classic Service Layer Flow

```
Controller → Service → Repository
   ↓           ↓
 Result    Result<T>
```

**Pros:**
- Simple, straightforward
- Easy to understand
- Good for small/medium apps

### CQRS with MediatR Flow

```
Controller → MediatR → Handler → Repository
                ↓         ↓
         Pipeline     Result<T>
         Behavior
```

**Pros:**
- Scalable architecture
- Clear separation (commands vs queries)
- Pipeline behaviors (logging, validation, etc.)
- Testable handlers

## 🎓 Learning Path

1. **Start with Classic endpoints** (`/api/classic/*`)
   - Understand basic Result usage
   - Learn validation with Combine
   - Explore functional pipelines (Map, Bind, Ensure, Tap, Match)

2. **Move to CQRS endpoints** (`/api/cqrs/*`)
   - See how MediatR simplifies handlers
   - Understand pipeline behaviors
   - Compare with Classic approach

3. **Read the code**
   - `Domain/Services/UserService.cs` - Classic service with pipeline examples
   - `Features/Users/*/` - CQRS handlers with MediatR
   - `Controllers/` - Both approaches side-by-side

## 🔧 Project Structure

```
Fox.ResultKit.WebApi.Demo/
├── Domain/
│   ├── Models/
│   │   └── User.cs                   # Domain entity
│   ├── Repositories/
│   │   ├── IUserRepository.cs        # Repository interface
│   │   └── InMemoryUserRepository.cs # In-memory implementation
│   └── Services/
│       └── UserService.cs            # Classic service with ResultKit pipelines
├── Features/                         # CQRS vertical slices
│   └── Users/
│       ├── CreateUser/               # Command
│       ├── GetUser/                  # Query with pipeline
│       └── ListUsers/                # Query with Map
├── Controllers/
│   ├── ClassicUsersController.cs     # Service-based endpoints
│   └── CqrsUsersController.cs        # MediatR-based endpoints
├── DTOs/
│   └── UserDtos.cs                   # Request/Response DTOs
└── Program.cs                        # DI configuration
```

## 💡 Key Takeaways

### ResultKit Strengths

✅ **Explicit error handling** - No hidden exceptions  
✅ **Functional composition** - Chain operations elegantly  
✅ **Type-safe** - Compiler ensures Result handling  
✅ **Lightweight** - Zero external dependencies  

### MediatR Integration Benefits

✅ **Automatic exception handling** - Pipeline behavior catches exceptions  
✅ **Clean handlers** - No try-catch boilerplate  
✅ **CQRS pattern** - Scalable architecture  
✅ **Testable** - Handlers are simple to unit test  

## 📝 License

Copyright (c) 2026 Károly Akácz. All rights reserved.

This demo is part of the ResultKit project for internal use.

---

**Tip:** Compare the same operation (e.g., Create User) in both Classic and CQRS controllers to see the architectural differences!
