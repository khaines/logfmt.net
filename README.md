# Logfmt.net

[![NuGet](https://img.shields.io/nuget/v/logfmt.net.svg)](https://www.nuget.org/packages/logfmt.net)
[![.NET](https://github.com/khaines/logfmt.net/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khaines/logfmt.net/actions/workflows/dotnet.yml)

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
using logfmt;

var log = new Logger();
log.Info("Hello, World!");
// Output: level=info msg="Hello, World!" ts=2023-10-27T10:00:00.0000000Z

// With structured data
log.Info("User logged in", 
    new KeyValuePair<string, string>("user_id", "123"), 
    new KeyValuePair<string, string>("ip", "192.168.1.1"));
// Output: level=info msg="User logged in" ts=... user_id=123 ip=192.168.1.1
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
        // Output: level=info msg="Processing request 12345" ts=... RequestId=12345
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
