// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt;

/// <summary>
/// Used to label log entries with a given severity.
/// </summary>
public enum SeverityLevel
{
    /// <summary>
    /// Trace severity.
    /// </summary>
    Trace,

    /// <summary>
    /// Debug severity.
    /// </summary>
    Debug,

    /// <summary>
    /// Info severity.
    /// </summary>
    Info,

    /// <summary>
    /// Warn severity.
    /// </summary>
    Warn,

    /// <summary>
    /// Error severity.
    /// </summary>
    Error,

    /// <summary>
    /// Fatal severity.
    /// </summary>
    Fatal,

    /// <summary>
    /// Off severity. This is only used in configuration to disable output.
    /// </summary>
    Off,
}