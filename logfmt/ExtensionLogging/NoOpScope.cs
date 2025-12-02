// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

/// <summary>
/// This is a placeholder scope, which has no functionality.
/// Used to satisfy some interface requirements of Microsoft's logging extention libraries.
/// </summary>
internal sealed class NoOpScope : IDisposable
{
    private NoOpScope()
    {
    }

    /// <summary>
    /// Gets the single instance of the <see cref="Logfmt.ExtensionLogging.NoOpScope" />.
    /// </summary>
    /// <returns>The singleton instance.</returns>
    public static NoOpScope Instance { get; } = new NoOpScope();

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}