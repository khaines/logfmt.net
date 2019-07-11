using System;
using System.IO;
using logfmt;
using Xunit;

namespace logfmt_tests
{
    public class UnitTest1
    {
        [Fact]
        public void LogOutput()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Info("hello logs!");

            outputStream.Seek(0,SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("msg=\"hello logs!\"", output);

        }
    }
}