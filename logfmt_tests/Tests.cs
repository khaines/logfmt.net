/*
MIT License

Copyright (c) 2019 Ken Haines

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
        public void CreateLoggerWithDefaultFieldsTest()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream).WithData(new KeyValuePair<string, string>("module","foo"));

            // write a log entry
            logger.Info("hello logs!");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("level=info msg=\"hello logs!\" module=foo", output);
        }
        
        [Fact]
        public void ExpectWarningWithInvalidKeyName()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry, but use a KVPair key containing a space. There should be a warning entry added to the output stream
            logger.Info("hello logs!",
                new KeyValuePair<string, string>("not valid key", "blue"));

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();
            // this should be the warning
            Assert.Contains(
                "level=warn msg=\"Error in processing log request. Key field cannot contain spaces and has been truncated.\" invalid_key=\"not valid key\"",
                output);
            output = reader.ReadLine();
            // line we input, but with the trucated key name
            Assert.Contains("level=info msg=\"hello logs!\" not=blue", output);
        }

        [Fact]
        public void ExpectNoExceptionOnClosedStream()
        {
            MemoryStream outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            outputStream.Close();
            outputStream.Dispose();
            try
            {
                // write a log entry. The stream is closed/disposed, but this shouldn't result in an unhandled exception to the caller
                logger.Info("hello logs!");
            }
            catch (Exception e)
            {
                Assert.Null(e);
            }
        }

        [Fact]
        public void LogDebugOutput()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Debug("hello logs!");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("level=debug msg=\"hello logs!\"", output);
        }


        [Fact]
        public void LogErrorOutput()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Error("hello logs!");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("level=error msg=\"hello logs!\"", output);
        }

        [Fact]
        public void LogInfoOutput()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Info("hello logs!");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("level=info msg=\"hello logs!\"", output);
        }


        [Fact]
        public void LogIWarnOutput()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Warn("hello logs!");

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("level=warn msg=\"hello logs!\"", output);
        }

        [Fact]
        public void LogOutputWithKVPairs()
        {
            var outputStream = new MemoryStream();
            var logger = new Logger(outputStream);

            // write a log entry
            logger.Info("hello logs!",
                new KeyValuePair<string, string>("color", "blue"),
                new KeyValuePair<string, string>("country", "United States"));

            outputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(outputStream);

            var output = reader.ReadLine();

            Assert.Contains("msg=\"hello logs!\"", output);
            Assert.Contains("color=blue", output);
            Assert.Contains("country=\"United States\"", output);
        }
    }
}