/*
MIT License

Copyright (c) 2021 Ken Haines

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
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace Logfmt
{
  public class Logger
  {
    private const string Date = "ts";
    private const string Message = "msg";
    private const string Level = "level";
    private const string Fieldformat = "{0}={1}";

    //will match spaces and other invalid characters that should not be in the key field
    private readonly Regex KeyNameFilter = new Regex("([^a-z0-9_])+", RegexOptions.IgnoreCase & RegexOptions.Compiled);

    private readonly TextWriter _output;
    private readonly Stream _outputStream;
    private List<KeyValuePair<string, string>> _includedData;

    public Logger() : this(Console.OpenStandardOutput())
    {
    }

    public Logger(Stream stream)
    {
      _outputStream = stream;
      _output = new StreamWriter(_outputStream);
      _includedData = new List<KeyValuePair<string, string>>();
    }

    public Logger WithData(params KeyValuePair<string, string>[] kvpairs)
    {
      var newLogger = new Logger(_outputStream);
      newLogger._includedData = _includedData;
      newLogger._includedData.AddRange(kvpairs);

      return newLogger;
    }

    public Logger WithData(params string[] kvpairs)
    {
      this.CheckParamArrayLength(kvpairs);
      List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
      for (var i = 0; i < kvpairs.Length; i += 2)
      {
        pairs.Add(new KeyValuePair<string, string>(kvpairs[i], kvpairs[i + 1]));
      }
      return this.WithData(pairs.ToArray());
    }

    public void Log(SeverityLevel severity, params KeyValuePair<string, string>[] kvpairs)
    {
      var buffer = new StringBuilder();

      // Date in ISO8601 format
      buffer.Append(string.Format(Fieldformat, Date, DateTime.UtcNow.ToString("o")));
      buffer.Append(" ");

      // severity level
      buffer.Append(string.Format(Fieldformat, Level, severity.ToLower()));

      // parameter pairs
      foreach (var pair in kvpairs.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
      {
        buffer.Append(" ");
        // data pair
        buffer.Append(string.Format(Fieldformat, PrepareKeyField(pair.Key), PrepareValueField(pair.Value)));

      }

      // default data to be included
      foreach (var pair in _includedData.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
      {
        buffer.Append(" ");
        // data pair
        buffer.Append(string.Format(Fieldformat, PrepareKeyField(pair.Key), PrepareValueField(pair.Value)));
      }

      if (_outputStream.CanWrite)
      {
        _output.WriteLine(buffer.ToString());
        _output.Flush();
      }
    }

    public void Log(SeverityLevel severity, string msg, params string[] kvpairs)
    {
      this.CheckParamArrayLength(kvpairs);

      List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

      pairs.Add(new KeyValuePair<string, string>(Message, msg));
      for (var i = 0; i < kvpairs.Length; i += 2)
      {
        pairs.Add(new KeyValuePair<string, string>(kvpairs[i], kvpairs[i + 1]));
      }

      this.Log(severity, pairs.ToArray());
    }

    private void CheckParamArrayLength<T>(T[] kvpairs)
    {
      if (kvpairs.Length % 2 != 0)
      {
        throw new ArgumentException("kvpairs must be an array with an even number of elements");
      }
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
      return KeyNameFilter.Replace(key, "_");
    }
  }
}