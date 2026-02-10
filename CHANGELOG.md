# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

_No unreleased changes yet._

## [1.2.0] - 2026-02-10

### Added

#### Fox.ResultKit (Core Library)
- `IResult` interface - Common contract for Result and Result<T> enabling polymorphic operations
- `ErrorsResult` record struct for collecting multiple validation errors
- `ErrorsResult.Collect(params IResult[])` - Aggregates errors from mixed Result and Result<T> types
- `ErrorsResult.ToResult()` - Converts ErrorsResult to single Result with combined error message
- `Bind(this Result, Func<Result>)` extension - Chains non-generic Result operations for fail-fast validation chains
- Validation-first pattern: Collect all validation errors (better UX) before domain pipeline (fail-fast)
- Support for mixed Result and Result<T> types in error collection scenarios

#### Fox.ResultKit.MediatR
- Updated dependency to Fox.ResultKit 1.2.0
- Documentation updated with ErrorsResult validation examples in MediatR handlers

#### Documentation
- Added comprehensive ErrorsResult documentation to README
- Validation-first pattern best practices and examples
- Updated comparison table: Multiple Errors ✅ ErrorsResult
- ErrorsResult examples in all package READMEs (Core, MediatR)

#### Samples
- New `/api/classic/users/with-validation` endpoint demonstrating ErrorsResult in classic controller
- New `/api/cqrs/users/with-validation` endpoint demonstrating ErrorsResult in CQRS controller
- `CreateUserWithValidationCommand` handler showing validation-first pattern
- `UserService.ValidateUserInputWithErrors()` method for input validation aggregation
- Updated `.http` file with validation examples and expected error responses

### Changed
- Version bumped to 1.2.0 for both Fox.ResultKit and Fox.ResultKit.MediatR packages

## [1.1.0] - 2026-02-09

### Added

#### Fox.ResultKit (Core Library)
- `ResultError` utility class for convention-based error code parsing and formatting
- `ResultError.Create(code, message)` - Creates formatted error string with code prefix
- `ResultError.Parse(error)` - Extracts error code and message from formatted string
- Error code convention: `"[ERROR_CODE] Message"` format for structured error handling

#### Documentation
- Added ResultError convention documentation to README
- Error code convention examples and best practices
- Package README updated with ResultError usage examples

#### Samples
- Updated demo application to use ResultError convention throughout
- Error code examples: `USER_NOT_FOUND`, `USER_EMAIL_EXISTS`, `USER_INACTIVE`, validation error codes
- Controller pattern matching with ResultError.Parse for typed error responses

### Changed
- Version bumped to 1.1.0 for Fox.ResultKit package

## [1.0.0] - 2026-02-07

### Added

#### Fox.ResultKit (Core Library)
- `Result` and `Result<T>` sealed classes for type-safe operation result handling
- Railway Oriented Programming extension methods:
  - `Map<T, U>` - Transform success values
  - `Bind<T, U>` - Chain operations that return Result
  - `Ensure<T>` - Validate results with predicates
  - `Tap<T>` - Execute side-effects on success
  - `TapFailure<T>` - Execute side-effects on failure
  - `Match<T, U>` - Pattern matching for both success and failure cases
- Async variants for all functional extensions (`MapAsync`, `BindAsync`, `EnsureAsync`, `TapAsync`, `TapFailureAsync`, `MatchAsync`)
- Conversion extensions (`ToResult`) for nullable types and Result type conversions
- `Combine` methods for aggregating multiple validation results
- Exception handling utilities (`Try`, `TryAsync`, `FromException`)
- Full XML documentation for IntelliSense support
- Multi-targeting support for .NET 8, 9, and 10
- Zero external dependencies

#### Fox.ResultKit.MediatR
- `ResultPipelineBehavior<TRequest, TResponse>` for automatic Result handling in MediatR pipelines
- Automatic exception-to-Result conversion for Result-typed responses
- `AddResultKitMediatR()` extension method for dependency injection configuration
- Seamless integration with CQRS patterns via MediatR

#### Documentation
- Comprehensive README with usage examples and API reference
- Railway Oriented Programming examples for service layer
- CQRS pattern examples with MediatR integration
- Comparison with alternative libraries (FluentResults, CSharpFunctionalExtensions, LanguageExt)

#### Samples
- `Fox.ResultKit.WebApi.Demo` - ASP.NET Core Web API demonstrating:
  - Classic service layer approach with Fox.ResultKit
  - CQRS pattern with MediatR and Fox.ResultKit
  - Controller-level error handling with pattern matching
  - Functional validation pipelines

[Unreleased]: https://github.com/akikari/Fox.ResultKit/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/akikari/Fox.ResultKit/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/akikari/Fox.ResultKit/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/akikari/Fox.ResultKit/releases/tag/v1.0.0
