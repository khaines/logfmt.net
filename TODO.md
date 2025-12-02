# Project Improvement Plan

## 1. Modernization (High Priority)

- [x] **Enable Nullable Reference Types (NRTs)**
  - Add `<Nullable>enable</Nullable>` to `logfmt.csproj`.
  - Fix resulting warnings to ensure null safety.
- [x] **Convert to File-Scoped Namespaces**
  - Update all `.cs` files to use `namespace Logfmt;` syntax (reduces nesting).
- [x] **Use Global Usings**
  - Enable `<ImplicitUsings>enable</ImplicitUsings>` or create a `GlobalUsings.cs` to remove repetitive `using` statements.

## 2. Code Quality & Style (Medium Priority)

- [x] **Fix StyleCop Warnings (XML Documentation)**
  - Add `/// <summary>` documentation to public methods in `Logger.cs` and OpenTelemetry classes.
  - Resolve `SA1600` warnings.
- [x] **Fix Ordering & Layout Rules**
  - Resolve `SA1200` (using directive placement) and `SA1507` (multiple blank lines) warnings.
- [x] **Standardize "Magic Strings"**
  - Refactor hardcoded strings like `"ts"`, `"level"`, `"msg"` into `private const string` fields in `Logger.cs`.

## 3. Performance Optimization (Long Term)

- [x] **Reduce Allocations in `PrepareValueField`**
  - Refactor string concatenation and `Replace` calls to use `StringBuilder` or `Span<char>` to reduce GC pressure.
- [x] **Add Benchmarks**
  - Create a `Logfmt.Benchmarks` project using BenchmarkDotNet to measure throughput and allocation.

## 4. CI/CD & Tooling (Housekeeping)

- [x] **Migrate to GitHub Actions**
  - Replace local `Makefile` workflows with a `.github/workflows/dotnet.yml` for automated build/test/pack.
- [x] **Centralize Package Management**
  - Implement `Directory.Packages.props` to manage dependency versions in one place.
