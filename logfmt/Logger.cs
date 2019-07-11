using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace logfmt
{
    public class Logger
    {
        private const string DATE = "ts";
        private const string MESSAGE = "msg";
        private const string LEVEL = "level";
        private const string INFO = "info";
        private const string DEBUG = "debug";
        private const string WARN = "warn";
        private const string ERROR = "error";
        private const string FIELDFORMAT = "{0}={1}";

        private TextWriter _output;

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
            this.Log(msg, INFO, kvpairs);
        }

        public void Debug(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            this.Log(msg, DEBUG, kvpairs);
        }


        public void Warn(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            this.Log(msg, WARN, kvpairs);
        }


        public void Error(string msg, params KeyValuePair<string, string>[] kvpairs)
        {
            this.Log(msg, ERROR, kvpairs);
        }

        private void Log(string msg, string severity, params KeyValuePair<string, string>[] kvpairs)
        {
            var buffer = new StringBuilder();

            // Date in ISO8601 format
            buffer.Append(String.Format(FIELDFORMAT, DATE, DateTime.UtcNow.ToString("o")));
            buffer.Append(" ");

            // severity level
            buffer.Append(String.Format(FIELDFORMAT, LEVEL, severity));
            buffer.Append(" ");

            // message
            buffer.Append(String.Format(FIELDFORMAT, MESSAGE, this.PrepareValueField(msg)));



            // paramter pairs
            foreach (KeyValuePair<string, string> pair in kvpairs)
            {
                buffer.Append(" ");
                // message
                buffer.Append(String.Format(FIELDFORMAT, this.PrepareKeyField(pair.Key), this.PrepareValueField(pair.Value)));
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
                this.Warn("Error in processing log request. Key field cannot contain spaces and has been truncated.",
                    new[] {new KeyValuePair<string, string>("invalid_key", key)});
                var space = key.IndexOf(" ");
                key = key.Substring(0, space);
            }

            return key;
        }
    }
}