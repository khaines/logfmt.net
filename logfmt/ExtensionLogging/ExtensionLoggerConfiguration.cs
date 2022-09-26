// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The configuration object used by the logger.
    /// </summary>
    public class ExtensionLoggerConfiguration
    {
        /// <summary>
        /// Gets or sets logging level by category.
        /// </summary>
        public Dictionary<string, LogLevel> LogLevel { get; set; }
    }
}
