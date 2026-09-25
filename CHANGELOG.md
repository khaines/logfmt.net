# Changelog

## 1.1.1 / 2026-09-25

- [SECURITY] Encoder: Control characters (C0/C1, including TAB and NEL U+0085) and the Unicode line separators U+2028/U+2029 are now quoted and escaped instead of emitted raw, closing a log-forgery vector and a terminal-escape-injection vector. The parser decodes `\uXXXX` escapes back to the original character. #90
- [BUGFIX] Logger: `Log(severity, msg, object[])` now contains a throwing value `ToString()` as a `[VALUE ERROR]` placeholder, matching the Extensions.Logging adapter. #89
- [BUGFIX] OpenTelemetryLogging: `ConsoleLogExporter` reads exception messages via a throw-safe helper so a single malformed record no longer fails the whole batch export. #89

## 1.1.0 / 2026-07-18

- [CHANGE] Encoding: Adopt kr/logfmt `=`/`"` conformance — values containing `=` are now quoted (e.g. `k="a=b"`), and the parser treats unquoted `=` and `"` as delimiters. #65 #82
- [CHANGE] Extensions.Logging: Category matching is now case-insensitive. #71
- [BUGFIX] Extensions.Logging: Runtime log-level lowering via `IOptionsMonitor` now takes effect; the core logger no longer double-gates, and `LogLevel.None`/out-of-range levels are never emitted. #80
- [BUGFIX] Extensions.Logging: `Log` no longer throws on hostile state — null keys, a throwing `ToString()`/formatter/`Exception.Message`, and throwing state enumerators are contained with `[VALUE ERROR]`/`[FORMATTER ERROR]`/`[STATE ERROR]` placeholders. #69 #80
- [BUGFIX] Logger: Fix a dispose-while-logging race in the core `Logger`. #66

## 1.0.0 / 2026-03-22

- Initial stable release.
