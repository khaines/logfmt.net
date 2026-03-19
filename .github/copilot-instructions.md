# Copilot Instructions for logfmt.net

## Build, Test, and Lint

```bash
dotnet build           # Build all projects
dotnet test            # Run all tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run a single test class
dotnet test --filter "FullyQualifiedName~Logfmt.Tests.LogfmtTests"

# Run tests for a specific framework
dotnet test Logfmt.Tests/ --framework net8.0

# Run benchmarks
cd Logfmt.Benchmarks && dotnet run -c Release
```

There is no separate lint command. **StyleCop.Analyzers** runs during build with warnings elevated to errors, so `dotnet build` is the lint step.

## Architecture

This is a .NET library that formats structured log output in [logfmt](https://brandur.org/logfmt) format (`key=value` pairs). It has three layers:

- **Core** (`Logfmt/Logger.cs`) — A `Logger` class that writes logfmt-formatted lines to a `Stream` (defaults to stdout). Uses `StringBuilder` for low-allocation formatting. Each call flushes immediately. `Logger.WithData()` returns a new `Logger` instance with accumulated default key-value pairs (builder pattern).
- **Microsoft.Extensions.Logging integration** (`Logfmt/ExtensionLogging/`) — An `ILoggerProvider`/`ILogger` adapter. `ExtensionLoggerProvider` caches loggers per category in a `ConcurrentDictionary`. Scopes are no-ops (`NoOpScope`).
- **OpenTelemetry integration** (`Logfmt/OpenTelemetryLogging/`) — A `BaseExporter<LogRecord>` that extracts attributes, trace context, and exception info from OpenTelemetry log records and writes them through the core `Logger`.

## Conventions

- **Targets:** net8.0 and net10.0 (library and tests). Benchmarks target net8.0 only. SDK version pinned to 10.0.100 in `global.json`.
- **Central package management:** All package versions are in `Directory.Packages.props`. Use `VersionOverride` in .csproj files only when necessary.
- **C# style:** File-scoped namespaces, nullable reference types enabled, implicit usings enabled. `SA1101` (prefix `this.`) is disabled.
- **Testing:** xUnit with built-in `Assert`. Tests capture output by writing to a `MemoryStream` and reading it back. Test classes are named `{Feature}Tests` with `[Fact]` methods.
- **Namespace structure:** Core types are in `Logfmt`, integrations use `Logfmt.ExtensionLogging` and `Logfmt.OpenTelemetryLogging`.
