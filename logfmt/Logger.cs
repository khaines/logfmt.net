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
using System.Text;

namespace logfmt
{
    public class Logger
    {
        private const string Date = "ts";
        private const string Message = "msg";
        private const string Level = "level";
        private const string InfoLevel = "info";
        private const string DebugLevel = "debug";
        private const string WarnLevel = "warn";
        private const string ErrorLevel = "error";
        private const string Fieldformat = "{0}={1}";

        private readonly TextWriter _output;

        public Logger()
        {
            _output = Console.Out;
        }

        public Logger(Stream stream)
        {
            _output = new StreamWriter(stream);
        }

        public void Info(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            Log(msg, InfoLevel, kvpairs);
        }

        public void Debug(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            Log(msg, DebugLevel, kvpairs);
        }


        public void Warn(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            Log(msg, WarnLevel, kvpairs);
        }


        public void Error(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            Log(msg, ErrorLevel, kvpairs);
        }

        private void Log(string msg, string severity, params KeyValuePair<string, string>[] kvpairs)
        {
            var buffer = new StringBuilder();

            // Date in ISO8601 format
            buffer.Append(string.Format(Fieldformat, Date, DateTime.UtcNow.ToString("o")));
            buffer.Append(" ");

            // severity level
            buffer.Append(string.Format(Fieldformat, Level, severity));
            buffer.Append(" ");

            // message
            buffer.Append(string.Format(Fieldformat, Message, PrepareValueField(msg)));


            // parameter pairs
            foreach (var pair in kvpairs)
            {
                buffer.Append(" ");
                // message
                buffer.Append(string.Format(Fieldformat, PrepareKeyField(pair.Key), PrepareValueField(pair.Value)));
            }

            _output.WriteLine(buffer.ToString());
            _output.Flush();
        }


        private string PrepareValueField(string value)
        {
            if (value.Contains(" "))
            {
                value = value.Replace("\"", "\\\"");
                value = "\"" + value + "\"";
            }

            return value;
        }

        private string PrepareKeyField(string key)
        {
            if (key.Contains(" "))
            {
                Warn("Error in processing log request. Key field cannot contain spaces and has been truncated.",
                    new KeyValuePair<string, string>("invalid_key", key));
                var space = key.IndexOf(" ");
                key = key.Substring(0, space);
            }

            return key;
        }
    }
}