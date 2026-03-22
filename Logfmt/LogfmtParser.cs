// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt;

/// <summary>
/// Parses logfmt-formatted strings into key-value pairs.
/// Follows the grammar defined by <see href="https://pkg.go.dev/github.com/kr/logfmt">kr/logfmt</see>.
/// </summary>
public static class LogfmtParser
{
    /// <summary>
    /// Parses a logfmt-formatted line into a list of key-value pairs.
    /// </summary>
    /// <param name="line">The logfmt-formatted string to parse.</param>
    /// <returns>A list of key-value pairs extracted from the line.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="line"/> is null.</exception>
    public static IReadOnlyList<KeyValuePair<string, string>> Parse(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var result = new List<KeyValuePair<string, string>>();
        int pos = 0;
        int len = line.Length;

        while (pos < len)
        {
            // Skip garbage (whitespace)
            while (pos < len && line[pos] == ' ')
            {
                pos++;
            }

            if (pos >= len)
            {
                break;
            }

            // Parse key
            int keyStart = pos;
            while (pos < len && line[pos] != '=' && line[pos] != ' ')
            {
                pos++;
            }

            if (pos == keyStart)
            {
                // No key found, skip character
                pos++;
                continue;
            }

            string key = line[keyStart..pos];

            // Check for = sign
            if (pos >= len || line[pos] != '=')
            {
                // Bare key with no value
                result.Add(new KeyValuePair<string, string>(key, string.Empty));
                continue;
            }

            // Skip '='
            pos++;

            // Parse value
            string value;
            if (pos < len && line[pos] == '"')
            {
                // Quoted value
                value = ParseQuotedValue(line, ref pos);
            }
            else
            {
                // Unquoted value
                int valueStart = pos;
                while (pos < len && line[pos] != ' ')
                {
                    pos++;
                }

                value = line[valueStart..pos];
            }

            result.Add(new KeyValuePair<string, string>(key, value));
        }

        return result;
    }

    private static string ParseQuotedValue(string line, ref int pos)
    {
        // Skip opening quote
        pos++;

        var buffer = new System.Text.StringBuilder();
        int len = line.Length;

        while (pos < len && line[pos] != '"')
        {
            if (line[pos] == '\\' && pos + 1 < len)
            {
                char next = line[pos + 1];
                switch (next)
                {
                    case '"':
                        buffer.Append('"');
                        pos += 2;
                        break;
                    case '\\':
                        buffer.Append('\\');
                        pos += 2;
                        break;
                    case 'n':
                        buffer.Append('\n');
                        pos += 2;
                        break;
                    case 'r':
                        buffer.Append('\r');
                        pos += 2;
                        break;
                    case 't':
                        buffer.Append('\t');
                        pos += 2;
                        break;
                    default:
                        buffer.Append(line[pos]);
                        pos++;
                        break;
                }
            }
            else
            {
                buffer.Append(line[pos]);
                pos++;
            }
        }

        // Skip closing quote
        if (pos < len && line[pos] == '"')
        {
            pos++;
        }

        return buffer.ToString();
    }
}
