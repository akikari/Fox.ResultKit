# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/akikari/Fox.ResultKit/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/akikari/Fox.ResultKit/releases/tag/v1.0.0
