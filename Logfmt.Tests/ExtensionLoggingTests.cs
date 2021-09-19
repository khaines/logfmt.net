// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System.IO;
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
  }
}