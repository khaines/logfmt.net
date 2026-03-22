# ADR 0001: Simplicity and Performance Over Features

**Status:** Accepted  
**Date:** 2026-03-22  
**Authors:** Ken Haines

## Context

logfmt.net is a structured logging library for .NET that formats output in the [logfmt](https://brandur.org/logfmt) format. As the library approaches v1.0, we need to establish clear boundaries around what the library will and will not do.

The .NET logging ecosystem includes full-featured frameworks like Serilog (enrichment pipelines, 100+ sinks, message templates, async batching) and NLog (XML configuration, log routing, targets, wrappers). These are excellent tools but serve a different purpose than logfmt.net.

We surveyed logfmt implementations across six libraries in four languages:
- **Go:** [go-logfmt/logfmt](https://github.com/go-logfmt/logfmt), [kr/logfmt](https://pkg.go.dev/github.com/kr/logfmt), [jsternberg/zap-logfmt](https://github.com/jsternberg/zap-logfmt)
- **Node.js:** [csquared/node-logfmt](https://github.com/csquared/node-logfmt)
- **Python:** [josheppinette/python-logfmter](https://github.com/josheppinette/python-logfmter)
- **Ruby:** [cyberdelia/logfmt-ruby](https://github.com/cyberdelia/logfmt-ruby)

**None of these libraries are feature-rich.** They are all intentionally slim — focused on encoding (and sometimes decoding) logfmt, with minimal opinions about log routing, storage, or processing.

## Decision

**logfmt.net will prioritize simplicity and performance over feature richness.** The library's purpose is to format structured log output in logfmt format quickly and with minimal allocations. It is not a logging framework.

### What we will do

- Provide a fast, low-allocation logfmt encoder
- Provide a logfmt parser/decoder for symmetry
- Integrate with `Microsoft.Extensions.Logging` as a provider
- Integrate with OpenTelemetry as a log exporter
- Support the `WithData()` builder pattern for log context (the logfmt-native equivalent of scopes)
- Maintain thread safety for concurrent logging
- Target modern .NET (currently net8.0 and net10.0)

### What we will NOT do

| Feature | Rationale |
|---------|-----------|
| **Multiple sink/provider composition** | logfmt is an output format, not a routing framework. .NET's `ILoggerFactory` already supports composing multiple providers — users can add logfmt alongside console, file, or any other provider. |
| **Log enrichment/middleware pipeline** | Over-engineering for a format library. `WithData()` provides the logfmt-native pattern for adding context to log entries, as described in Brandur Leach's [original logfmt article](https://brandur.org/logfmt). |
| **Async/buffered logging** | Synchronous flush is simpler, more predictable, and easier to debug. The ~110ns per-call overhead makes async unnecessary for most workloads. Users who need buffering can wrap the output stream or use OpenTelemetry's `BatchLogRecordExportProcessor`. |
| **JSON output mode** | Out of scope. This is a logfmt library. Use Serilog or `System.Text.Json` for JSON logging. |
| **File rolling/rotation** | Infrastructure concern. Write to stdout and let the platform (Docker, systemd, log aggregator) handle rotation. This follows the [twelve-factor app](https://12factor.net/logs) methodology. |
| **Scope tracking** | `NoOpScope` is intentional. Scopes add allocation overhead on every `BeginScope` call and logfmt has no standard representation for nested contexts. The `WithData()` builder pattern is the correct logfmt-native equivalent. Brandur's later [canonical log lines](https://brandur.org/canonical-log-lines) article recommends a single rich log line per request rather than nested scopes. |
| **Sampling/rate limiting** | Better handled at the infrastructure level (OpenTelemetry SDK sampling, log aggregator ingestion rules). Adding sampling to the library would couple output formatting with delivery policy. |
| **Configuration from appsettings.json** | Not directly implemented, but supported through `IOptionsMonitor<ExtensionLoggerConfiguration>` which integrates with .NET's standard configuration system when properly wired up via dependency injection. |
| **Semantic/template logging (`{PropertyName}`)** | The `Microsoft.Extensions.Logging` integration already handles message template rendering via the framework's `formatter` delegate. The core logger uses explicit key-value pairs which are more aligned with the logfmt philosophy of eliminating guesswork in log line design. |

## Consequences

- **Users who need advanced features** should use Serilog or NLog and can add logfmt.net alongside them.
- **The API surface stays small** — fewer methods to learn, fewer breaking changes to manage, and easier to maintain.
- **Performance remains the primary differentiator** — zero-allocation filtered calls, ~110ns/264B per log entry, thread-safe with minimal contention.
- **The library remains a single NuGet package** rather than a constellation of sink/enricher packages.
