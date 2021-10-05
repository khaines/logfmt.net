// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
    using System.IO;
    using System.Collections.Generic;
    using Logfmt.ExtensionLogging;
    using Microsoft.Extensions.Logging;
    using Xunit;

    /// <summary>
    /// Tests covering the functionality found in the Logfmt.ExtensionLogging namespace.
    /// </summary>
    public class ExtensionLoggingTests
    {
        /// <summary>
        /// Tests basic output of the Extensionlogger instance via the ILogger interface.
        /// </summary>
        [Fact]
        public void TestILoggerBasicOutput()
        {
            var outputStream = new MemoryStream();
            ILogger logger = new ExtensionLogger(new Logger(outputStream));

            logger.LogInformation(new EventId(1, "test"), null, "test message");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);
            var output = reader.ReadLine();

            Assert.EndsWith("level=info _OriginalFormat_=\"test message\" msg=\"test message\" event_id=1 event_name=1", output);
        }

        /// <summary>
        /// Tests output of the extension logger, ensuring that a debug message isn't emitted
        /// the level is set to INFO
        [Fact]
        public void TestILoggerFilteredOutput()
        {
            var outputStream = new MemoryStream();
            ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info));


            logger.LogDebug(new EventId(1, "test"), null, "test message", "foo", "bar");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);
            var output = reader.ReadLine();
            Assert.True(output == null);
        }

        /// <summary>
        /// Tests to ensure provided state properties are logged
        /// </summary>
        [Fact]
        public void TestILoggerStatePropOutput()
        {
            var outputStream = new MemoryStream();
            ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info));
            var state = new Dictionary<string, object>();
            state["foo"] = "bar";
            state["msg"] = "test message";
            logger.Log(LogLevel.Warning, new EventId(1, "test"), state, null, null);

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);
            var output = reader.ReadLine();
            Assert.EndsWith("level=warn foo=bar msg=\"test message\" event_id=1 event_name=1", output);
        }
    }
}