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