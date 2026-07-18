# Changelog

## 1.1.0 / 2026-07-18

- [CHANGE] Encoding: Adopt kr/logfmt `=`/`"` conformance — values containing `=` are now quoted (e.g. `k="a=b"`), and the parser treats unquoted `=` and `"` as delimiters. #65 #82
- [CHANGE] Extensions.Logging: Category matching is now case-insensitive. #71
- [BUGFIX] Extensions.Logging: Runtime log-level lowering via `IOptionsMonitor` now takes effect; the core logger no longer double-gates, and `LogLevel.None`/out-of-range levels are never emitted. #80
- [BUGFIX] Extensions.Logging: `Log` no longer throws on hostile state — null keys, a throwing `ToString()`/formatter/`Exception.Message`, and throwing state enumerators are contained with `[VALUE ERROR]`/`[FORMATTER ERROR]`/`[STATE ERROR]` placeholders. #69 #80
- [BUGFIX] Logger: Fix a dispose-while-logging race in the core `Logger`. #66

## 1.0.0 / 2026-03-22

- Initial stable release.
