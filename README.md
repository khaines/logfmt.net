# Logfmt.net

[![NuGet](https://img.shields.io/nuget/v/logfmt.net.svg)](https://www.nuget.org/packages/logfmt.net)
[![.NET](https://github.com/khaines/logfmt.net/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khaines/logfmt.net/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**logfmt.net** is a simple, lightweight, and high-performance structured logging library for .NET applications, focusing on the [logfmt](https://brandur.org/logfmt) format.

## Features

- **High Performance**: Optimized for low allocations and high throughput.
- **Modern .NET Support**: Targets .NET 8.0 and .NET 10.0.
- **Standard Integrations**:
  - Native support for `Microsoft.Extensions.Logging`.
  - Integration with `OpenTelemetry`.
- **Flexible Output**: Writes to Console (stdout) by default, or any `Stream`.
- **Structured Data**: First-class support for Key-Value pairs.

## Installation

Install the package via NuGet:

```bash
dotnet add package logfmt.net
```

## Usage

### Basic Usage

```csharp
using Logfmt;

var log = new Logger();
log.Info("Hello, World!");
// Output: ts=2026-03-21T12:00:00.0000000Z level=info msg="Hello, World!"

// With structured data using string pairs
log.Info("User logged in", "user_id", "123", "ip", "192.168.1.1");
// Output: ts=... level=info msg="User logged in" user_id=123 ip=192.168.1.1
```

### Default Fields with WithData

Use `WithData()` to create a logger that includes fields on every log entry:

```csharp
var log = new Logger().WithData("service", "api", "env", "production");
log.Info("Server started", "port", "8080");
// Output: ts=... level=info msg="Server started" port=8080 service=api env=production
```

### Severity Filtering

Control which log levels are emitted:

```csharp
var log = new Logger(SeverityLevel.Warn);
log.Debug("This won't be logged");
log.Warn("This will be logged");
```

### Custom Output Stream

Write logs to any stream:

```csharp
using var stream = File.OpenWrite("app.log");
var log = new Logger(stream, SeverityLevel.Info);
log.Info("Written to file");
```

### Microsoft.Extensions.Logging

Logfmt.net integrates seamlessly with the standard .NET logging abstractions.

```csharp
using Microsoft.Extensions.Logging;
using Logfmt.ExtensionLogging;

// Add to your ILoggingBuilder (e.g., in ASP.NET Core or Generic Host)
builder.Logging.ClearProviders();
builder.Logging.AddLogfmt();

// Inject and use ILogger
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogInformation("Processing request {RequestId}", 12345);
        // Output: ts=... level=info msg="Processing request 12345" RequestId=12345
    }
}
```

### OpenTelemetry Support

You can use logfmt as an exporter for OpenTelemetry logs.

```csharp
using OpenTelemetry.Logs;
using Logfmt.OpenTelemetryLogging;

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddLogfmtConsoleExporter();
});
```

## Building the Source

You can build the project using the .NET CLI:

```bash
dotnet build
dotnet test
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

Please feel free to submit a Pull Request or open an Issue.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
