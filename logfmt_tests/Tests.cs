using System;
using System.Collections.Generic;
using System.IO;
using logfmt;
using Xunit;

namespace logfmt_tests
{
    public class Tests
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

        [Fact]
        public void LogOutputWithKVPairs()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Info("hello logs!",
                new KeyValuePair<string, string>("color", "blue"),new KeyValuePair<string,string>("country","United States"));

            outputStream.Seek(0,SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("msg=\"hello logs!\"", output);
            Assert.Contains("color=blue", output);
            Assert.Contains("country=\"United States\"", output);
        }

        [Fact]
        public void ExpectWarningWithInvalidKeyName()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry, but use a KVPair key containing a space. There should be a warning entry added to the output stream
            logger.Info("hello logs!",
                new KeyValuePair<string, string>("not valid key", "blue"));

            outputStream.Seek(0,SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();
            // this should be the warning
            Assert.Contains(
                "level=warn msg=\"Error in processing log request. Key field cannot contain spaces and has been truncated.\" invalid_key=\"not valid key\"", output);
            output = reader.ReadLine();
            // line we input, but with the trucated key name
            Assert.Contains("level=info msg=\"hello logs!\" not=blue",output);

        }
    }
}