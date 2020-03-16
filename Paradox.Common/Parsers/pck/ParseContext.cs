// a class that helps with writing hand-rolled parsers, which we use for regex and ebnf parsing

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Paradox.Common.Parsers.pck
{
    #region ExpectingException
    /// <summary>
    /// An exception encountered during parsing where the stream contains one thing, but another is expected
    /// </summary>
    [Serializable]
    public sealed class ExpectingException : Exception
    {
        /// <summary>
        /// Initialize the exception with the specified message.
        /// </summary>
        /// <param name="message">The message</param>
        public ExpectingException(string message) : base(message) { }
        /// <summary>
        /// The list of expected strings.
        /// </summary>
        public string[] Expecting { get; internal set; }
        /// <summary>
        /// The position when the error was realized.
        /// </summary>
        public long Position { get; internal set; }
        /// <summary>
        /// The line of the error
        /// </summary>
        public int Line { get; internal set; }
        /// <summary>
        /// The column of the error
        /// </summary>
        public int Column { get; internal set; }

    }
    #endregion ExpectingException
    /// <summary>
    /// see https://www.codeproject.com/Articles/5162847/ParseContext-2-0-Easier-Hand-Rolled-Parsers
    /// </summary>
    public partial class ParseContext : IDisposable
    {
        public bool TryReadWhiteSpace()
        {
            this.EnsureStarted();
            if (-1 == this.Current || !char.IsWhiteSpace((char)this.Current))
                return false;
            this.CaptureCurrent();
            while (-1 != this.Advance() && char.IsWhiteSpace((char)this.Current))
                this.CaptureCurrent();
            return true;
        }

        public bool TrySkipWhiteSpace()
        {
            this.EnsureStarted();
            if (-1 == this.Current || !char.IsWhiteSpace((char)this.Current))
                return false;
            while (-1 != this.Advance() && char.IsWhiteSpace((char)this.Current)) ;
            return true;
        }

        public bool TryReadUntil(int character, bool readCharacter = true)
        {
            this.EnsureStarted();
            if (0 > character) character = -1;
            this.CaptureCurrent();
            if (this.Current == character)
            {
                return true;
            }
            while (-1 != this.Advance() && this.Current != character)
                this.CaptureCurrent();
            //
            if (this.Current != character) return false;
            if (!readCharacter) return true;
            this.CaptureCurrent();
            this.Advance();
            return true;
        }

        public bool TrySkipUntil(int character, bool skipCharacter = true)
        {
            this.EnsureStarted();
            if (0 > character) character = -1;
            if (this.Current == character)
                return true;
            while (-1 != this.Advance() && this.Current != character) ;
            if (this.Current != character) return false;
            if (skipCharacter)
                this.Advance();
            return true;
        }
        public bool TryReadUntil(int character, int escapeChar, bool readCharacter = true)
        {
            this.EnsureStarted();
            if (0 > character) character = -1;
            if (-1 == this.Current) return false;
            if (this.Current == character)
            {
                if (readCharacter)
                {
                    this.CaptureCurrent();
                    this.Advance();
                }
                return true;
            }

            do
            {
                if (escapeChar == this.Current)
                {
                    this.CaptureCurrent();
                    if (-1 == this.Advance())
                        return false;
                    this.CaptureCurrent();
                }
                else
                {
                    if (character == this.Current)
                    {
                        if (!readCharacter) return true;
                        this.CaptureCurrent();
                        this.Advance();
                        return true;
                    }
                    else
                        this.CaptureCurrent();
                }
            }
            while (-1 != this.Advance());

            return false;
        }
        public bool TrySkipUntil(int character, int escapeChar, bool skipCharacter = true)
        {
            this.EnsureStarted();
            if (0 > character) character = -1;
            if (this.Current == character)
                return true;
            while (-1 != this.Advance() && this.Current != character)
            {
                if (character != escapeChar) continue;
                if (-1 == this.Advance())
                    break;
            }

            if (this.Current != character) return false;
            if (skipCharacter)
                this.Advance();
            return true;
        }
        private static bool _ContainsChar(IEnumerable<char> chars, char ch)
        {
            return chars.Any(cmp => cmp == ch);
        }

        public bool TryReadUntil(bool readCharacter = true, params char[] anyOf)
        {
            this.EnsureStarted();
            if (null == anyOf)
                anyOf = Array.Empty<char>();
            this.CaptureCurrent();
            if (-1 != this.Current && _ContainsChar(anyOf, (char)this.Current))
            {
                if (!readCharacter) return true;
                this.CaptureCurrent();
                this.Advance();
                return true;
            }
            while (-1 != this.Advance() && !_ContainsChar(anyOf, (char)this.Current))
                this.CaptureCurrent();
            if (-1 == this.Current || !_ContainsChar(anyOf, (char) this.Current)) return false;
            if (!readCharacter) return true;
            this.CaptureCurrent();
            this.Advance();
            return true;
        }
        public bool TrySkipUntil(bool skipCharacter = true, params char[] anyOf)
        {
            this.EnsureStarted();
            if (null == anyOf)
                anyOf = Array.Empty<char>();
            if (-1 != this.Current && _ContainsChar(anyOf, (char)this.Current))
            {
                if (skipCharacter)
                    this.Advance();
                return true;
            }
            while (-1 != this.Advance() && !_ContainsChar(anyOf, (char)this.Current)) ;
            if (-1 != this.Current && _ContainsChar(anyOf, (char)this.Current))
            {
                if (skipCharacter)
                    this.Advance();
                return true;
            }
            return false;
        }
        public bool TryReadUntil(string text)
        {
            this.EnsureStarted();
            if (string.IsNullOrEmpty(text))
                return false;
            while (-1 != this.Current && this.TryReadUntil(text[0], false))
            {
                var found = true;
                for (var i = 1; i < text.Length; ++i)
                {
                    if (this.Advance() != text[i])
                    {
                        found = false;
                        break;
                    }
                    this.CaptureCurrent();
                }
                if (found)
                {
                    this.Advance();
                    return true;
                }
            }

            return false;
        }
        public bool TrySkipUntil(string text)
        {
            this.EnsureStarted();
            if (string.IsNullOrEmpty(text))
                return false;
            while (-1 != this.Current && this.TrySkipUntil(text[0], false))
            {
                var found = true;
                for (var i = 1; i < text.Length; ++i)
                {
                    if (this.Advance() != text[i])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    this.Advance();
                    return true;
                }
            }
            return false;
        }


        public bool TryReadDigits()
        {
            this.EnsureStarted();
            if (-1 == this.Current || !char.IsDigit((char)this.Current))
                return false;
            this.CaptureCurrent();
            while (-1 != this.Advance() && char.IsDigit((char)this.Current))
                this.CaptureCurrent();
            return true;
        }
        public bool TrySkipDigits()
        {
            this.EnsureStarted();
            if (-1 == this.Current || !char.IsDigit((char)this.Current))
                return false;
            while (-1 != this.Advance() && char.IsDigit((char)this.Current)) ;
            return true;
        }

        public bool TryReadJsonString()
        {
            this.EnsureStarted();
            if ('\"' != this.Current)
                return false;
            this.CaptureCurrent();
            while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current && this.Current != '\"')
            {
                this.CaptureCurrent();
                if ('\\' == this.Current)
                {
                    if (-1 == this.Advance() || '\r' == this.Current || '\n' == this.Current)
                        return false;
                    this.CaptureCurrent();

                }
            }
            if (this.Current == '\"')
            {
                this.CaptureCurrent();
                this.Advance(); // move past the string
                return true;
            }
            return false;
        }

        private static bool _IsHexChar(char hex)
        {
            return (
                (':' > hex && '/' < hex) ||
                ('G' > hex && '@' < hex) ||
                ('g' > hex && '`' < hex)
            );
        }

        private static byte _FromHexChar(char hex)
        {
            if (':' > hex && '/' < hex)
                return (byte)(hex - '0');
            if ('G' > hex && '@' < hex)
                return (byte)(hex - '7'); // 'A'-10
            if ('g' > hex && '`' < hex)
                return (byte)(hex - 'W'); // 'a'-10
            throw new ArgumentException("The value was not hex.", "hex");
        }
        public bool TrySkipJsonString()
        {
            this.EnsureStarted();
            if ('\"' != this.Current)
                return false;
            while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current && this.Current != '\"')
                if ('\\' == this.Current)
                    if (-1 == this.Advance() || '\r' == this.Current || '\n' == this.Current)
                        return false;

            if (this.Current == '\"')
            {
                this.Advance(); // move past the string
                return true;
            }
            return false;
        }
        public bool TryParseJsonString(out string result)
        {
            result = null;
            var sb = new StringBuilder();
            this.EnsureStarted();
            if ('\"' != this.Current)
                return false;
            this.CaptureCurrent();
            while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current && this.Current != '\"')
            {
                this.CaptureCurrent();
                if ('\\' == this.Current)
                {
                    if (-1 == this.Advance() || '\r' == this.Current || '\n' == this.Current)
                        return false;
                    this.CaptureCurrent();
                    switch (this.Current)
                    {
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '/':
                            sb.Append('/');
                            break;
                        case '\"':
                            sb.Append('\"');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'u':
                            this.CaptureCurrent();
                            if (-1 == this.Advance())
                                return false;
                            var ch = 0;
                            if (!_IsHexChar((char)this.Current))
                                return false;
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);

                            this.CaptureCurrent();
                            if (-1 == this.Advance())
                                return false;
                            if (!_IsHexChar((char)this.Current))
                                return false;
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);

                            this.CaptureCurrent();
                            if (-1 == this.Advance())
                                return false;
                            if (!_IsHexChar((char)this.Current))
                                return false;
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);

                            this.CaptureCurrent();
                            if (-1 == this.Advance())
                                return false;
                            if (!_IsHexChar((char)this.Current))
                                return false;
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);

                            sb.Append((char)ch);
                            break;
                        default:
                            return false;
                    }
                }
                else
                    sb.Append((char)this.Current);

            }
            if (this.Current == '\"')
            {
                this.CaptureCurrent();
                this.Advance(); // move past the string
                result = sb.ToString();
                return true;
            }
            return false;
        }
        public string ParseJsonString()
        {
            var sb = new StringBuilder();
            this.EnsureStarted();
            this.Expecting('\"');
            while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current && this.Current != '\"')
            {
                if ('\\' == this.Current)
                {
                    this.Advance();
                    switch (this.Current)
                    {
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'v':
                            sb.Append('\v');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '\"':
                            sb.Append('\"');
                            break;
                        case 'u':
                            var ch = 0;
                            this.Advance();
                            this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f');
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);
                            this.Advance();
                            this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f');
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);
                            this.Advance();
                            this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f');
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);
                            this.Advance();
                            this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f');
                            ch <<= 4;
                            ch |= _FromHexChar((char)this.Current);
                            sb.Append((char)ch);
                            break;
                        default:
                            this.Expecting('b', 'n', 'r', 't', '\\', '/', '\"', 'u');
                            break;
                    }
                }
                else
                    sb.Append((char)this.Current);
            }
            this.Expecting('\"');
            this.Advance();
            return sb.ToString();
        }
        public bool TryReadJsonValue()
        {
            this.TryReadWhiteSpace();
            if ('t' == this.Current)
            {
                this.CaptureCurrent();
                if (this.Advance() != 'r')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'u')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'e')
                    return false;
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('f' == this.Current)
            {
                this.CaptureCurrent();
                if (this.Advance() != 'a')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'l')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 's')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'e')
                    return false;
                this.CaptureCurrent();
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('n' == this.Current)
            {
                this.CaptureCurrent();
                if (this.Advance() != 'u')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'l')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'l')
                    return false;
                this.CaptureCurrent();
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('-' == this.Current || '.' == this.Current || char.IsDigit((char)this.Current))
                return this.TryReadReal();
            if ('\"' == this.Current)
                return this.TryReadJsonString();

            if ('[' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                if (this.TryReadJsonValue())
                {
                    this.TryReadWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.CaptureCurrent();
                        this.Advance();
                        if (!this.TryReadJsonValue()) return false;
                        this.TryReadWhiteSpace();
                    }
                }
                this.TryReadWhiteSpace();
                if (']' != this.Current)
                    return false;
                this.CaptureCurrent();
                this.Advance();
                return true;
            }
            if ('{' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                this.TryReadWhiteSpace();
                if (this.TryReadJsonString())
                {
                    this.TryReadWhiteSpace();
                    if (':' != this.Current) return false;
                    this.CaptureCurrent();
                    this.Advance();
                    if (!this.TryReadJsonValue())
                        return false;

                    this.TryReadWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.CaptureCurrent();
                        this.Advance();
                        this.TryReadWhiteSpace();
                        if (!this.TryReadJsonString())
                            return false;
                        this.TryReadWhiteSpace();
                        if (':' != this.Current) return false;
                        this.CaptureCurrent();
                        this.Advance();
                        if (!this.TryReadJsonValue())
                            return false;
                        this.TryReadWhiteSpace();
                    }
                }
                this.TryReadWhiteSpace();
                if ('}' != this.Current)
                    return false;
                this.CaptureCurrent();
                this.Advance();
                return true;
            }
            return false;
        }
        public bool TrySkipJsonValue()
        {
            this.TrySkipWhiteSpace();
            if ('t' == this.Current)
            {
                if (this.Advance() != 'r')
                    return false;
                if (this.Advance() != 'u')
                    return false;
                if (this.Advance() != 'e')
                    return false;
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('f' == this.Current)
            {
                if (this.Advance() != 'a')
                    return false;
                if (this.Advance() != 'l')
                    return false;
                if (this.Advance() != 's')
                    return false;
                if (this.Advance() != 'e')
                    return false;
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('n' == this.Current)
            {
                if (this.Advance() != 'u')
                    return false;
                if (this.Advance() != 'l')
                    return false;
                if (this.Advance() != 'l')
                    return false;
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('-' == this.Current || '.' == this.Current || char.IsDigit((char)this.Current))
                return this.TrySkipReal();
            if ('\"' == this.Current)
                return this.TrySkipJsonString();

            if ('[' == this.Current)
            {
                this.Advance();
                if (this.TrySkipJsonValue())
                {
                    this.TrySkipWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.Advance();
                        if (!this.TrySkipJsonValue()) return false;
                        this.TrySkipWhiteSpace();
                    }
                }
                this.TrySkipWhiteSpace();
                if (']' != this.Current)
                    return false;
                this.Advance();
                return true;
            }
            if ('{' == this.Current)
            {
                this.Advance();
                this.TrySkipWhiteSpace();
                if (this.TrySkipJsonString())
                {
                    this.TrySkipWhiteSpace();
                    if (':' != this.Current) return false;
                    this.Advance();
                    if (!this.TrySkipJsonValue())
                        return false;
                    this.TrySkipWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.Advance();
                        this.TrySkipWhiteSpace();
                        if (!this.TrySkipJsonString())
                            return false;
                        this.TrySkipWhiteSpace();
                        if (':' != this.Current) return false;
                        this.Advance();
                        if (!this.TrySkipJsonValue())
                            return false;
                        this.TrySkipWhiteSpace();
                    }
                }
                this.TrySkipWhiteSpace();
                if ('}' != this.Current)
                    return false;
                this.Advance();
                return true;
            }
            return false;
        }
        public bool TryParseJsonValue(out object result)
        {
            result = null;
            this.TryReadWhiteSpace();
            if ('t' == this.Current)
            {
                this.CaptureCurrent();
                if (this.Advance() != 'r')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'u')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'e')
                    return false;
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                result = true;
                return true;
            }
            if ('f' == this.Current)
            {
                this.CaptureCurrent();
                if (this.Advance() != 'a')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'l')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 's')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'e')
                    return false;
                this.CaptureCurrent();
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                result = false;
                return true;
            }
            if ('n' == this.Current)
            {
                this.CaptureCurrent();
                if (this.Advance() != 'u')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'l')
                    return false;
                this.CaptureCurrent();
                if (this.Advance() != 'l')
                    return false;
                this.CaptureCurrent();
                if (-1 != this.Advance() && char.IsLetterOrDigit((char)this.Current))
                    return false;
                return true;
            }
            if ('-' == this.Current || '.' == this.Current || char.IsDigit((char)this.Current))
            {
                double r;
                if (this.TryParseReal(out r))
                {
                    result = r;
                    return true;
                }
                return false;
            }
            if ('\"' == this.Current)
            {
                string s;
                if (this.TryParseJsonString(out s))
                {
                    result = s;
                    return true;
                }
                return false;
            }
            if ('[' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                var l = new List<object>();
                object v;
                if (this.TryParseJsonValue(out v))
                {
                    l.Add(v);
                    this.TryReadWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.CaptureCurrent();
                        this.Advance();
                        if (!this.TryParseJsonValue(out v)) return false;
                        l.Add(v);
                        this.TryReadWhiteSpace();
                    }
                }
                this.TryReadWhiteSpace();
                if (']' != this.Current)
                    return false;
                this.CaptureCurrent();
                this.Advance();
                result = l;
                return true;
            }
            if ('{' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                this.TryReadWhiteSpace();
                string n;
                object v;
                var d = new Dictionary<string, object>();
                if (this.TryParseJsonString(out n))
                {
                    this.TryReadWhiteSpace();
                    if (':' != this.Current) return false;
                    this.CaptureCurrent();
                    this.Advance();
                    if (!this.TryParseJsonValue(out v))
                        return false;
                    d.Add(n, v);
                    this.TryReadWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.CaptureCurrent();
                        this.Advance();
                        this.TryReadWhiteSpace();
                        if (!this.TryParseJsonString(out n))
                            return false;
                        this.TryReadWhiteSpace();
                        if (':' != this.Current) return false;
                        this.CaptureCurrent();
                        this.Advance();
                        if (!this.TryParseJsonValue(out v))
                            return false;
                        d.Add(n, v);
                        this.TryReadWhiteSpace();
                    }
                }
                this.TryReadWhiteSpace();
                if ('}' != this.Current)
                    return false;
                this.CaptureCurrent();
                this.Advance();
                result = d;
                return true;
            }
            return false;
        }

        public object ParseJsonValue()
        {
            this.TrySkipWhiteSpace();
            if ('t' == this.Current)
            {
                this.Advance(); this.Expecting('r');
                this.Advance(); this.Expecting('u');
                this.Advance(); this.Expecting('e');
                this.Advance();
                return true;
            }
            if ('f' == this.Current)
            {
                this.Advance(); this.Expecting('a');
                this.Advance(); this.Expecting('l');
                this.Advance(); this.Expecting('s');
                this.Advance(); this.Expecting('e');
                this.Advance();
                return true;
            }
            if ('n' == this.Current)
            {
                this.Advance(); this.Expecting('u');
                this.Advance(); this.Expecting('l');
                this.Advance(); this.Expecting('l');
                this.Advance();
                return null;
            }
            if ('-' == this.Current || '.' == this.Current || char.IsDigit((char)this.Current))
                return this.ParseReal();
            if ('\"' == this.Current)
                return this.ParseJsonString();
            if ('[' == this.Current)
            {
                this.Advance();
                this.TrySkipWhiteSpace();
                var l = new List<object>();
                if (']' != this.Current)
                {
                    l.Add(this.ParseJsonValue());
                    this.TrySkipWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.Advance();
                        l.Add(this.ParseJsonValue());
                        this.TrySkipWhiteSpace();
                    }
                }
                this.TrySkipWhiteSpace();
                this.Expecting(']');
                this.Advance();
                return l;
            }
            if ('{' == this.Current)
            {
                this.Advance();
                this.TrySkipWhiteSpace();
                var d = new Dictionary<string, object>();
                if ('}' != this.Current)
                {

                    var n = this.ParseJsonString();
                    this.TrySkipWhiteSpace();
                    this.Expecting(':');
                    this.Advance();
                    var v = this.ParseJsonValue();
                    d.Add(n, v);
                    this.TrySkipWhiteSpace();
                    while (',' == this.Current)
                    {
                        this.Advance();
                        this.TrySkipWhiteSpace();
                        n = this.ParseJsonString();
                        this.TrySkipWhiteSpace();
                        this.Expecting(':');
                        this.Advance();
                        v = this.ParseJsonValue();
                        d.Add(n, v);
                        this.TrySkipWhiteSpace();
                    }
                }
                this.TrySkipWhiteSpace();
                if ('}' != this.Current)
                    return false;
                this.Advance();
                return d;
            }
            return false;
        }


        public bool TryReadInteger()
        {
            this.EnsureStarted();
            var neg = false;
            if ('-' == this.Current)
            {
                neg = true;
                this.CaptureCurrent();
                this.Advance();
            }
            else if ('0' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                if (-1 == this.Current) return true;
                return !char.IsDigit((char)this.Current);
            }
            if (-1 == this.Current || (neg && '0' == this.Current) || !char.IsDigit((char)this.Current))
                return false;
            if (!this.TryReadDigits())
                return false;
            return true;
        }

        public bool TrySkipInteger()
        {
            this.EnsureStarted();
            var neg = false;
            if ('-' == this.Current)
            {
                neg = true;
                this.Advance();
            }
            else if ('0' == this.Current)
            {
                this.Advance();
                if (-1 == this.Current) return true;
                return !char.IsDigit((char)this.Current);
            }
            if (-1 == this.Current || (neg && '0' == this.Current) || !char.IsDigit((char)this.Current))
                return false;
            if (!this.TrySkipDigits())
                return false;
            return true;
        }
        // must be object because we don't know the int type. To be lexically valid we must use BigInteger when necessary
        public bool TryParseInteger(out object result)
        {
            result = null;
            this.EnsureStarted();
            if (-1 == this.Current) return false;
            var neg = false;
            if ('-' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                neg = true;
            }
            var l = this.CaptureBuffer.Length;
            if (this.TryReadDigits())
            {
                var num = this.CaptureBuffer.ToString(l, this.CaptureBuffer.Length - l);
                if (neg)
                    num = '-' + num;
                int r;
                if (int.TryParse(num, out r))
                {
                    result = r;
                    return true;
                }
                long ll;
                if (long.TryParse(num, out ll))
                {
                    result = ll;
                    return true;
                }

            }
            return false;
        }
        public object ParseInteger()
        {
            this.EnsureStarted();
            this.Expecting('-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var neg = ('-' == this.Current);
            if (neg)
            {
                this.Advance();
                this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            }
            long i = 0;
            if (!neg)
            {
                i += ((char)this.Current) - '0';
                while (-1 != this.Advance() && char.IsDigit((char)this.Current))
                {
                    i *= 10;
                    i += ((char)this.Current) - '0';
                }

            }
            else
            {
                i -= ((char)this.Current) - '0';
                while (-1 != this.Advance() && char.IsDigit((char)this.Current))
                {
                    i *= 10;
                    i -= ((char)this.Current) - '0';
                }
            }
            if (i <= int.MaxValue && i >= int.MinValue)
                return (int)i;
            else if (i <= long.MaxValue && i >= long.MinValue)
                return i;
            return i;
        }
        public bool TryReadReal()
        {
            this.EnsureStarted();
            var readAny = false;
            if ('-' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
            }
            if (char.IsDigit((char)this.Current))
            {
                if (!this.TryReadDigits())
                    return false;
                readAny = true;
            }
            if ('.' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                if (!this.TryReadDigits())
                    return false;
                readAny = true;
            }
            if ('E' == this.Current || 'e' == this.Current)
            {
                this.CaptureCurrent();
                this.Advance();
                if ('-' == this.Current || '+' == this.Current)
                {
                    this.CaptureCurrent();
                    this.Advance();
                }
                return this.TryReadDigits();
            }

            return readAny;
        }
        public bool TrySkipReal()
        {
            var readAny = false;
            this.EnsureStarted();
            if ('-' == this.Current)
                this.Advance();
            if (char.IsDigit((char)this.Current))
            {
                if (!this.TrySkipDigits())
                    return false;
                readAny = true;
            }
            if ('.' == this.Current)
            {
                this.Advance();
                if (!this.TrySkipDigits())
                    return false;
                readAny = true;
            }
            if ('E' == this.Current || 'e' == this.Current)
            {
                this.Advance();
                if ('-' == this.Current || '+' == this.Current)
                    this.Advance();
                return this.TrySkipDigits();
            }

            return readAny;
        }
        public bool TryParseReal(out double result)
        {
            result = default(double);
            var l = this.CaptureBuffer.Length;
            if (!this.TryReadReal())
                return false;
            return double.TryParse(this.CaptureBuffer.ToString(l, this.CaptureBuffer.Length - l), out result);
        }

        public double ParseReal()
        {
            this.EnsureStarted();
            var sb = new StringBuilder();
            this.Expecting('-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            var neg = ('-' == this.Current);
            if (neg)
            {
                sb.Append((char)this.Current);
                this.Advance();
                this.Expecting('.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            }
            while (-1 != this.Current && char.IsDigit((char)this.Current))
            {
                sb.Append((char)this.Current);
                this.Advance();
            }
            if ('.' == this.Current)
            {
                sb.Append((char)this.Current);
                this.Advance();
                this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                sb.Append((char)this.Current);
                while (-1 != this.Advance() && char.IsDigit((char)this.Current))
                {
                    sb.Append((char)this.Current);
                }
            }
            if ('E' == this.Current || 'e' == this.Current)
            {
                sb.Append((char)this.Current);
                this.Advance();
                this.Expecting('+', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                switch (this.Current)
                {
                    case '+':
                    case '-':
                        sb.Append((char)this.Current);
                        this.Advance();
                        break;
                }
                this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                sb.Append((char)this.Current);
                while (-1 != this.Advance())
                {
                    this.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                    sb.Append((char)this.Current);
                }
            }
            return double.Parse(sb.ToString());

        }
        public bool TryReadCLineComment()
        {
            this.EnsureStarted();
            if ('/' != this.Current)
                return false;
            this.CaptureCurrent();
            if ('/' != this.Advance())
                return false;
            this.CaptureCurrent();
            while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current)
                this.CaptureCurrent();
            return true;
        }
        public bool TrySkipCLineComment()
        {
            this.EnsureStarted();
            if ('/' != this.Current)
                return false;
            if ('/' != this.Advance())
                return false;
            while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current) ;
            return true;
        }
        public bool TryReadCBlockComment()
        {
            this.EnsureStarted();
            if ('/' != this.Current)
                return false;
            this.CaptureCurrent();
            if ('*' != this.Advance())
                return false;
            this.CaptureCurrent();
            if (-1 == this.Advance())
                return false;
            return this.TryReadUntil("*/");
        }
        public bool TrySkipCBlockComment()
        {
            this.EnsureStarted();
            if ('/' != this.Current)
                return false;
            if ('*' != this.Advance())
                return false;
            if (-1 == this.Advance())
                return false;
            return this.TrySkipUntil("*/");
        }
        public bool TryReadCComment()
        {
            this.EnsureStarted();
            if ('/' != this.Current)
                return false;
            this.CaptureCurrent();
            if ('*' == this.Advance())
            {
                this.CaptureCurrent();
                if (-1 == this.Advance())
                    return false;
                return this.TryReadUntil("*/");
            }
            if ('/' == this.Current)
            {
                this.CaptureCurrent();
                while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current)
                    this.CaptureCurrent();
                return true;
            }
            return false;
        }
        public bool TrySkipCComment()
        {
            this.EnsureStarted();
            if ('/' != this.Current)
                return false;
            if ('*' == this.Advance())
            {
                if (-1 == this.Advance())
                    return false;
                return this.TrySkipUntil("*/");
            }
            if ('/' == this.Current)
            {
                while (-1 != this.Advance() && '\r' != this.Current && '\n' != this.Current) ;
                return true;
            }
            return false;
        }
        public bool TryReadCCommentsAndWhitespace()
        {
            var result = false;
            while (-1 != this.Current)
            {
                if (!this.TryReadWhiteSpace() && !this.TryReadCComment())
                    break;
                result = true;
            }
            if (this.TryReadWhiteSpace())
                result = true;
            return result;
        }
        public bool TrySkipCCommentsAndWhiteSpace()
        {
            var result = false;
            while (-1 != this.Current)
            {
                if (!this.TrySkipWhiteSpace() && !this.TrySkipCComment())
                    break;
                result = true;
            }
            if (this.TrySkipWhiteSpace())
                result = true;
            return result;
        }

        private ParseContext(IEnumerable<char> inner) { this._inner = inner.GetEnumerator(); }

        private ParseContext(TextReader inner) { this._inner = new _TextReaderEnumerator(inner); }

        private Queue<char> _input = new Queue<char>();

        private IEnumerator<char> _inner = null;
        public StringBuilder CaptureBuffer { get; } = new StringBuilder();
        public long Position { get; private set; } = -2;
        public int Column { get; private set; } = 1;
        public int Line { get; private set; } = 1;

        public int Current { get; private set; } = -2;
        public int TabWidth { get; set; } = 8;

        private bool _EnsureInput()
        {
            if (0 == this._input.Count)
            {
                if (!this._inner.MoveNext())
                    return false;
                this._input.Enqueue(this._inner.Current);
                return true;
            }
            return true;
        }
        public void EnsureStarted()
        {
            this._CheckDisposed();
            if (-2 == this.Current)
                this.Advance();
        }
        public int Peek(int lookAhead = 1)
        {
            this._CheckDisposed();
            if (-2 == this.Current) throw new InvalidOperationException("The parse context has not been started.");
            if (0 > lookAhead)
                lookAhead = 0;
            if (!this.EnsureLookAhead(0 != lookAhead ? lookAhead : 1))
                return -1;
            var i = 0;
            foreach (var result in this._input)
            {
                if (i == lookAhead)
                    return result;
                ++i;
            }
            return -1;

        }
        public bool EnsureLookAhead(int lookAhead = 1)
        {
            this._CheckDisposed();
            if (1 > lookAhead) lookAhead = 1;
            while (this._input.Count < lookAhead && this._inner.MoveNext())
                this._input.Enqueue(this._inner.Current);
            return this._input.Count >= lookAhead;
        }
        public int Advance()
        {
            this._CheckDisposed();
            if (0 != this._input.Count)
                this._input.Dequeue();
            if (this._EnsureInput())
            {
                if (-2 == this.Current)
                {
                    this.Position = -1;
                    this.Column = 0;
                }
                this.Current = this._input.Peek();
                ++this.Column;
                ++this.Position;
                if ('\n' == this.Current)
                {
                    ++this.Line;
                    this.Column = 0;
                }
                else if ('\r' == this.Current)
                {
                    this.Column = 0;
                }
                else if ('\t' == this.Current && 0 < this.TabWidth)
                {
                    this.Column = ((this.Column / this.TabWidth) + 1) * this.TabWidth;
                }
                // handle other whitespace as necessary here...
                return this.Current;
            }
            if (-1 != this.Current)
            { // last read moves us past the end. subsequent reads don't move anything
                ++this.Position;
                ++this.Column;
            }
            this.Current = -1;
            return -1;
        }
        public void Dispose()
        {
            if (null != this._inner)
            {
                this.Current = -3;
                this._inner.Dispose();
                this._inner = null;
            }
        }
        public void ClearCapture()
        {
            this._CheckDisposed();
            this.CaptureBuffer.Clear();
        }
        public void CaptureCurrent()
        {
            this._CheckDisposed();
            if (-2 == this.Current) throw new InvalidOperationException("The parse context has not been started.");
            if (-1 != this.Current)
                this.CaptureBuffer.Append((char)this.Current);
        }
        public string GetCapture(int startIndex, int count = 0)
        {
            this._CheckDisposed();
            if (0 == count)
                count = this.CaptureBuffer.Length - startIndex;
            return this.CaptureBuffer.ToString(startIndex, count);
        }
        public string GetCapture(int startIndex = 0)
        {
            this._CheckDisposed();
            return this.CaptureBuffer.ToString(startIndex, this.CaptureBuffer.Length - startIndex);
        }
        public void SetLocation(int line, int column, long position)
        {
            switch (this.Current)
            {
                case -3:
                    throw new ObjectDisposedException(this.GetType().Name);
                case -2:
                    throw new InvalidOperationException("The cursor is before the start of the stream.");
                case -1:
                    throw new InvalidOperationException("The cursor is after the end of the stream.");
            }
            this.Position = position;
            this.Line = line;
            this.Column = column;
        }
        [DebuggerHidden()]
        public void ThrowExpectingRanges(int[] expecting)
        {
            ExpectingException ex = null;
            ex = new ExpectingException(this._GetExpectingMessageRanges(expecting));
            ex.Position = this.Position;
            ex.Line = this.Line;
            ex.Column = this.Column;
            ex.Expecting = null;
            throw ex;

        }

        private void _CheckDisposed()
        {
            if (-3 == this.Current) throw new ObjectDisposedException(this.GetType().Name);
        }

        private string _GetExpectingMessageRanges(int[] expecting)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            for (var i = 0; i < expecting.Length; i++)
            {
                var first = expecting[i];
                ++i;
                var last = expecting[i];
                if (first == last)
                {
                    if (-1 == first)
                        sb.Append("(end of stream)");
                    else
                        sb.Append((char)first);
                }
                else
                {
                    sb.Append((char)first);
                    sb.Append('-');
                    sb.Append((char)last);
                }
            }
            sb.Append(']');
            var at = string.Concat(" at line ", this.Line, ", column ", this.Column, ", position ", this.Position);
            if (-1 == this.Current)
            {
                if (0 == expecting.Length)
                    return string.Concat("Unexpected end of input", at, ".");
                return string.Concat("Unexpected end of input. Expecting ", sb.ToString(), at, ".");
            }
            if (0 == expecting.Length)
                return string.Concat("Unexpected character \"", (char)this.Current, "\" in input", at, ".");
            return string.Concat("Unexpected character \"", (char)this.Current, "\" in input. Expecting ", sb.ToString(), at, ".");

        }

        private string _GetExpectingMessage(int[] expecting)
        {
            StringBuilder sb = null;
            switch (expecting.Length)
            {
                case 0:
                    break;
                case 1:
                    sb = new StringBuilder();
                    if (-1 == expecting[0])
                        sb.Append("end of input");
                    else
                    {
                        sb.Append("\"");
                        sb.Append((char)expecting[0]);
                        sb.Append("\"");
                    }
                    break;
                case 2:
                    sb = new StringBuilder();
                    if (-1 == expecting[0])
                        sb.Append("end of input");
                    else
                    {
                        sb.Append("\"");
                        sb.Append((char)expecting[0]);
                        sb.Append("\"");
                    }
                    sb.Append(" or ");
                    if (-1 == expecting[1])
                        sb.Append("end of input");
                    else
                    {
                        sb.Append("\"");
                        sb.Append((char)expecting[1]);
                        sb.Append("\"");
                    }
                    break;
                default: // length > 2
                    sb = new StringBuilder();
                    if (-1 == expecting[0])
                        sb.Append("end of input");
                    else
                    {
                        sb.Append("\"");
                        sb.Append((char)expecting[0]);
                        sb.Append("\"");
                    }
                    var l = expecting.Length - 1;
                    var i = 1;
                    for (; i < l; ++i)
                    {
                        sb.Append(", ");
                        if (-1 == expecting[i])
                            sb.Append("end of input");
                        else
                        {
                            sb.Append("\"");
                            sb.Append((char)expecting[i]);
                            sb.Append("\"");
                        }
                    }
                    sb.Append(", or ");
                    if (-1 == expecting[i])
                        sb.Append("end of input");
                    else
                    {
                        sb.Append("\"");
                        sb.Append((char)expecting[i]);
                        sb.Append("\"");
                    }
                    break;
            }
            var at = string.Concat(" at line ", this.Line, ", column ", this.Column, ", position ", this.Position);
            if (-1 == this.Current)
            {
                if (0 == expecting.Length)
                    return string.Concat("Unexpected end of input", at, ".");
                return string.Concat("Unexpected end of input. Expecting ", sb.ToString(), at, ".");
            }
            if (0 == expecting.Length)
                return string.Concat("Unexpected character \"", (char)this.Current, "\" in input", at, ".");
            return string.Concat("Unexpected character \"", (char)this.Current, "\" in input. Expecting ", sb.ToString(), at, ".");

        }
        [DebuggerHidden()]
        public void Expecting(params int[] expecting)
        {
            ExpectingException ex = null;
            switch (expecting.Length)
            {
                case 0:
                    if (-1 == this.Current)
                        ex = new ExpectingException(this._GetExpectingMessage(expecting));
                    break;
                case 1:
                    if (expecting[0] != this.Current)
                        ex = new ExpectingException(this._GetExpectingMessage(expecting));
                    break;
                default:
                    if (0 > Array.IndexOf(expecting, this.Current))
                        ex = new ExpectingException(this._GetExpectingMessage(expecting));
                    break;
            }
            if (null != ex)
            {
                ex.Position = this.Position;
                ex.Line = this.Line;
                ex.Column = this.Column;
                ex.Expecting = new string[expecting.Length];
                for (var i = 0; i < ex.Expecting.Length; i++)
                    ex.Expecting[i] = Convert.ToString(expecting[i]);
                throw ex;
            }
        }
        public static ParseContext Create(IEnumerable<char> @string) { return new ParseContext(@string); }
        public static ParseContext CreateFrom(TextReader reader) { return new ParseContext(reader); }
        public static ParseContext CreateFrom(string filename) { return new ParseContext(File.OpenText(filename)); }

        public static ParseContext CreateFromUrl(string url)
        {
            var wreq = WebRequest.Create(url);
            var wresp = wreq.GetResponse();
            return CreateFrom(new StreamReader(wresp.GetResponseStream()));
        }

        private class _TextReaderEnumerator : IEnumerator<char>
        {
            private int _current = -2;

            private TextReader _inner;
            internal _TextReaderEnumerator(TextReader inner) { this._inner = inner; }
            public char Current
            {
                get
                {
                    switch (this._current)
                    {
                        case -1:
                            throw new InvalidOperationException("The enumerator is past the end of the stream.");
                        case -2:
                            throw new InvalidOperationException("The enumerator has not been started.");
                    }
                    return unchecked((char)this._current);
                }
            }
            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this._current = -3;
                if (null != this._inner)
                {
                    this._inner.Dispose();
                    this._inner = null;
                }
            }
            public bool MoveNext()
            {
                switch (this._current)
                {
                    case -1:
                        return false;
                    case -3:
                        throw new ObjectDisposedException(this.GetType().Name);
                }
                this._current = this._inner.Read();
                if (-1 == this._current)
                    return false;
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

    }

}
