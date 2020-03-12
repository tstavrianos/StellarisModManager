// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Stellaris.Data.Parsers
{
    /// <summary>
    /// An optimized representation of a substring.
    /// </summary>
    public readonly struct StringSegment : IEquatable<StringSegment>, IEquatable<string>
    {
        /// <summary>
        /// A <see cref="StringSegment"/> for <see cref="string.Empty"/>.
        /// </summary>
        public static readonly StringSegment Empty = string.Empty;

        /// <summary>
        /// Initializes an instance of the <see cref="StringSegment"/> struct.
        /// </summary>
        /// <param name="buffer">
        /// The original <see cref="string"/>. The <see cref="StringSegment"/> includes the whole <see cref="string"/>.
        /// </param>
        public StringSegment(string buffer)
        {
            this.Buffer = buffer;
            this.Offset = 0;
            this.Length = buffer?.Length ?? 0;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="StringSegment"/> struct.
        /// </summary>
        /// <param name="buffer">The original <see cref="string"/> used as buffer.</param>
        /// <param name="offset">The offset of the segment within the <paramref name="buffer"/>.</param>
        /// <param name="length">The length of the segment.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="length"/> is less than zero, or <paramref name="offset"/> +
        /// <paramref name="length"/> is greater than the number of characters in <paramref name="buffer"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSegment(string buffer, int offset, int length)
        {
            // Validate arguments, check is minimal instructions with reduced branching for inlinable fast-path
            // Negative values discovered though conversion to high values when converted to unsigned
            // Failure should be rare and location determination and message is delegated to failure functions
            if(buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if((uint)offset > (uint)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if((uint)length > (uint)(buffer.Length - offset))
                throw new ArgumentOutOfRangeException(nameof(length));

            this.Buffer = buffer;
            this.Offset = offset;
            this.Length = length;
        }

        /// <summary>
        /// Gets the <see cref="string"/> buffer for this <see cref="StringSegment"/>.
        /// </summary>
        public string Buffer { get; }

        /// <summary>
        /// Gets the offset within the buffer for this <see cref="StringSegment"/>.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the length of this <see cref="StringSegment"/>.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the value of this segment as a <see cref="string"/>.
        /// </summary>
        public string Value => this.HasValue ? this.Buffer.Substring(this.Offset, this.Length) : null;

        /// <summary>
        /// Gets whether this <see cref="StringSegment"/> contains a valid value.
        /// </summary>
        public bool HasValue => this.Buffer != null;

        /// <summary>
        /// Gets the <see cref="char"/> at a specified position in the current <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="index">The offset into the <see cref="StringSegment"/></param>
        /// <returns>The <see cref="char"/> at a specified position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is greater than or equal to <see cref="Length"/> or less than zero.
        /// </exception>
        public char this[int index]
        {
            get
            {
                if ((uint)index >= (uint) this.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return this.Buffer[this.Offset + index];
            }
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlySpan{T}"/> from the current <see cref="StringSegment"/>.
        /// </summary>
        /// <returns>The <see cref="ReadOnlySpan{T}"/> from this <see cref="StringSegment"/>.</returns>
        public ReadOnlySpan<char> AsSpan() => this.Buffer.AsSpan(this.Offset, this.Length);

        /// <summary>
        /// Gets a <see cref="ReadOnlyMemory{T}"/> from the current <see cref="StringSegment"/>.
        /// </summary>
        /// <returns>The <see cref="ReadOnlyMemory{T}"/> from this <see cref="StringSegment"/>.</returns>
        public ReadOnlyMemory<char> AsMemory() => this.Buffer.AsMemory(this.Offset, this.Length);

        /// <summary>
        /// Compares substrings of two specified <see cref="StringSegment"/> objects using the specified rules,
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="a">The first <see cref="StringSegment"/> to compare.</param>
        /// <param name="b">The second <see cref="StringSegment"/> to compare.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
        /// <returns>
        /// A 32-bit signed integer indicating the lexical relationship between the two comparands.
        /// The value is negative if <paramref name="a"/> is less than <paramref name="b"/>, 0 if the two comparands are equal,
        /// and positive if <paramref name="a"/> is greater than <paramref name="b"/>.
        /// </returns>
        public static int Compare(StringSegment a, StringSegment b, StringComparison comparisonType)
        {
            var minLength = Math.Min(a.Length, b.Length);
            var diff = string.Compare(a.Buffer, a.Offset, b.Buffer, b.Offset, minLength, comparisonType);
            if (diff == 0)
            {
                diff = a.Length - b.Length;
            }

            return diff;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is StringSegment segment && this.Equals(segment);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the other parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(StringSegment other) => this.Equals(other, StringComparison.Ordinal);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true" /> if the current object is equal to the other parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(StringSegment other, StringComparison comparisonType)
        {
            if (this.Length != other.Length)
            {
                return false;
            }

            return string.Compare(this.Buffer, this.Offset, other.Buffer, other.Offset, other.Length, comparisonType) == 0;
        }

        // This handles StringSegment.Equals(string, StringSegment, StringComparison) and StringSegment.Equals(StringSegment, string, StringComparison)
        // via the implicit type converter
        /// <summary>
        /// Determines whether two specified <see cref="StringSegment"/> objects have the same value. A parameter specifies the culture, case, and
        /// sort rules used in the comparison.
        /// </summary>
        /// <param name="a">The first <see cref="StringSegment"/> to compare.</param>
        /// <param name="b">The second <see cref="StringSegment"/> to compare.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
        /// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
        public static bool Equals(StringSegment a, StringSegment b, StringComparison comparisonType)
        {
            return a.Equals(b, comparisonType);
        }

        /// <summary>
        /// Checks if the specified <see cref="string"/> is equal to the current <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="text">The <see cref="string"/> to compare with the current <see cref="StringSegment"/>.</param>
        /// <returns><see langword="true" /> if the specified <see cref="string"/> is equal to the current <see cref="StringSegment"/>; otherwise, <see langword="false" />.</returns>
        public bool Equals(string text)
        {
            return this.Equals(text, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if the specified <see cref="string"/> is equal to the current <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="text">The <see cref="string"/> to compare with the current <see cref="StringSegment"/>.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true" /> if the specified <see cref="string"/> is equal to the current <see cref="StringSegment"/>; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string text, StringComparison comparisonType)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var textLength = text.Length;
            if (!this.HasValue || this.Length != textLength)
            {
                return false;
            }

            return string.Compare(this.Buffer, this.Offset, text, 0, textLength, comparisonType) == 0;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return string.GetHashCode(this.AsSpan());
        }

        /// <summary>
        /// Checks if two specified <see cref="StringSegment"/> have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="StringSegment"/> to compare, or <see langword="null" />.</param>
        /// <param name="right">The second <see cref="StringSegment"/> to compare, or <see langword="null" />.</param>
        /// <returns><see langword="true" /> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(StringSegment left, StringSegment right) => left.Equals(right);

        /// <summary>
        /// Checks if two specified <see cref="StringSegment"/> have different values.
        /// </summary>
        /// <param name="left">The first <see cref="StringSegment"/> to compare, or <see langword="null" />.</param>
        /// <param name="right">The second <see cref="StringSegment"/> to compare, or <see langword="null" />.</param>
        /// <returns><see langword="true" /> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(StringSegment left, StringSegment right) => !left.Equals(right);

        // PERF: Do NOT add a implicit converter from StringSegment to String. That would negate most of the perf safety.
        /// <summary>
        /// Creates a new <see cref="StringSegment"/> from the given <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to convert to a <see cref="StringSegment"/></param>
        public static implicit operator StringSegment(string value) => new StringSegment(value);

        /// <summary>
        /// Creates a see <see cref="ReadOnlySpan{T}"/> from the given <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="segment">The <see cref="StringSegment"/> to convert to a <see cref="ReadOnlySpan{T}"/>.</param>
        public static implicit operator ReadOnlySpan<char>(StringSegment segment) => segment.AsSpan();

        /// <summary>
        /// Creates a see <see cref="ReadOnlyMemory{T}"/> from the given <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="segment">The <see cref="StringSegment"/> to convert to a <see cref="ReadOnlyMemory{T}"/>.</param>
        public static implicit operator ReadOnlyMemory<char>(StringSegment segment) => segment.AsMemory();

        /// <summary>
        /// Checks if the beginning of this <see cref="StringSegment"/> matches the specified <see cref="string"/> when compared using the specified <paramref name="comparisonType"/>.
        /// </summary>
        /// <param name="text">The <see cref="string"/>to compare.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true" /> if <paramref name="text"/> matches the beginning of this <see cref="StringSegment"/>; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(string text, StringComparison comparisonType)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var result = false;
            var textLength = text.Length;

            if (this.HasValue && this.Length >= textLength)
            {
                result = string.Compare(this.Buffer, this.Offset, text, 0, textLength, comparisonType) == 0;
            }

            return result;
        }

        /// <summary>
        /// Checks if the end of this <see cref="StringSegment"/> matches the specified <see cref="string"/> when compared using the specified <paramref name="comparisonType"/>.
        /// </summary>
        /// <param name="text">The <see cref="string"/>to compare.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true" /> if <paramref name="text"/> matches the end of this <see cref="StringSegment"/>; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(string text, StringComparison comparisonType)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var result = false;
            var textLength = text.Length;
            var comparisonLength = this.Offset + this.Length - textLength;

            if (this.HasValue && comparisonLength > 0)
            {
                result = string.Compare(this.Buffer, comparisonLength, text, 0, textLength, comparisonType) == 0;
            }

            return result;
        }

        /// <summary>
        /// Retrieves a substring from this <see cref="StringSegment"/>.
        /// The substring starts at the position specified by <paramref name="offset"/> and has the remaining length.
        /// </summary>
        /// <param name="offset">The zero-based starting character position of a substring in this <see cref="StringSegment"/>.</param>
        /// <returns>A <see cref="string"/> that is equivalent to the substring of remaining length that begins at
        /// <paramref name="offset"/> in this <see cref="StringSegment"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> is greater than or equal to <see cref="Length"/> or less than zero.
        /// </exception>
        public string Substring(int offset) => this.Substring(offset, this.Length - offset);

        /// <summary>
        /// Retrieves a substring from this <see cref="StringSegment"/>.
        /// The substring starts at the position specified by <paramref name="offset"/> and has the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="offset">The zero-based starting character position of a substring in this <see cref="StringSegment"/>.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A <see cref="string"/> that is equivalent to the substring of length <paramref name="length"/> that begins at
        /// <paramref name="offset"/> in this <see cref="StringSegment"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="length"/> is less than zero, or <paramref name="offset"/> + <paramref name="length"/> is
        /// greater than <see cref="Length"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Substring(int offset, int length)
        {
            if (!this.HasValue)
            {
                throw new ArgumentException();
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if ((uint)(offset + length) > (uint) this.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), nameof(length));
            }

            return this.Buffer.Substring(this.Offset + offset, length);
        }

        /// <summary>
        /// Retrieves a <see cref="StringSegment"/> that represents a substring from this <see cref="StringSegment"/>.
        /// The <see cref="StringSegment"/> starts at the position specified by <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The zero-based starting character position of a substring in this <see cref="StringSegment"/>.</param>
        /// <returns>A <see cref="StringSegment"/> that begins at <paramref name="offset"/> in this <see cref="StringSegment"/>
        /// whose length is the remainder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> is greater than or equal to <see cref="Length"/> or less than zero.
        /// </exception>
        public StringSegment Subsegment(int offset) => this.Subsegment(offset, this.Length - offset);

        /// <summary>
        /// Retrieves a <see cref="StringSegment"/> that represents a substring from this <see cref="StringSegment"/>.
        /// The <see cref="StringSegment"/> starts at the position specified by <paramref name="offset"/> and has the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="offset">The zero-based starting character position of a substring in this <see cref="StringSegment"/>.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A <see cref="StringSegment"/> that is equivalent to the substring of length <paramref name="length"/> that begins at <paramref name="offset"/> in this <see cref="StringSegment"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="length"/> is less than zero, or <paramref name="offset"/> + <paramref name="length"/> is
        /// greater than <see cref="Length"/>.
        /// </exception>
        public StringSegment Subsegment(int offset, int length)
        {
            if (!this.HasValue)
            {
                throw new ArgumentException();
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if ((uint)(offset + length) > (uint) this.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), nameof(length));
            }
            
            return new StringSegment(this.Buffer, this.Offset + offset, length);
        }

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the character <paramref name="c"/> in this <see cref="StringSegment"/>.
        /// The search starts at <paramref name="start"/> and examines a specified number of <paramref name="count"/> character positions.
        /// </summary>
        /// <param name="c">The Unicode character to seek.</param>
        /// <param name="start">The zero-based index position at which the search starts. </param>
        /// <param name="count">The number of characters to examine.</param>
        /// <returns>The zero-based index position of <paramref name="c"/> from the beginning of the <see cref="StringSegment"/> if that character is found, or -1 if it is not.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> or <paramref name="count"/> is less than zero, or <paramref name="start"/> + <paramref name="count"/> is
        /// greater than <see cref="Length"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char c, int start, int count)
        {
            var offset = this.Offset + start;

            if (!this.HasValue || start < 0 || (uint)offset > (uint) this.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var index = this.Buffer.IndexOf(c, offset, count);
            if (index != -1)
            {
                index -= this.Offset;
            }

            return index;
        }

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the character <paramref name="c"/> in this <see cref="StringSegment"/>.
        /// The search starts at <paramref name="start"/>.
        /// </summary>
        /// <param name="c">The Unicode character to seek.</param>
        /// <param name="start">The zero-based index position at which the search starts. </param>
        /// <returns>The zero-based index position of <paramref name="c"/> from the beginning of the <see cref="StringSegment"/> if that character is found, or -1 if it is not.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> is greater than or equal to <see cref="Length"/> or less than zero.
        /// </exception>
        public int IndexOf(char c, int start) => this.IndexOf(c, start, this.Length - start);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the character <paramref name="c"/> in this <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="c">The Unicode character to seek.</param>
        /// <returns>The zero-based index position of <paramref name="c"/> from the beginning of the <see cref="StringSegment"/> if that character is found, or -1 if it is not.</returns>
        public int IndexOf(char c) => this.IndexOf(c, 0, this.Length);

        /// <summary>
        /// Reports the zero-based index of the first occurrence in this instance of any character in a specified array
        /// of Unicode characters. The search starts at a specified character position and examines a specified number
        /// of character positions.
        /// </summary>
        /// <param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf"/>
        /// was found; -1 if no character in <paramref name="anyOf"/> was found.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="anyOf"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="count"/> is less than zero, or <paramref name="startIndex"/> + <paramref name="count"/> is
        /// greater than <see cref="Length"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] anyOf, int startIndex, int count)
        {
            var index = -1;

            if (!this.HasValue) return index;
            if (startIndex < 0 || this.Offset + startIndex > this.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (count < 0 || this.Offset + startIndex + count > this.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            index = this.Buffer.IndexOfAny(anyOf, this.Offset + startIndex, count);
            if (index != -1)
            {
                index -= this.Offset;
            }

            return index;
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence in this instance of any character in a specified array
        /// of Unicode characters. The search starts at a specified character position.
        /// </summary>
        /// <param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf"/>
        /// was found; -1 if no character in <paramref name="anyOf"/> was found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is greater than or equal to <see cref="Length"/> or less than zero.
        /// </exception>
        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            return this.IndexOfAny(anyOf, startIndex, this.Length - startIndex);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence in this instance of any character in a specified array
        /// of Unicode characters.
        /// </summary>
        /// <param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf"/>
        /// was found; -1 if no character in <paramref name="anyOf"/> was found.</returns>
        public int IndexOfAny(char[] anyOf)
        {
            return this.IndexOfAny(anyOf, 0, this.Length);
        }

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.
        /// </summary>
        /// <param name="value">The Unicode character to seek.</param>
        /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
        public int LastIndexOf(char value)
        {
            var index = -1;

            if (!this.HasValue) return index;
            index = this.Buffer.LastIndexOf(value, this.Offset + this.Length - 1, this.Length);
            if (index != -1)
            {
                index -= this.Offset;
            }

            return index;
        }

        /// <summary>
        /// Removes all leading and trailing whitespaces.
        /// </summary>
        /// <returns>The trimmed <see cref="StringSegment"/>.</returns>
        public StringSegment Trim() => this.TrimStart().TrimEnd();

        /// <summary>
        /// Removes all leading whitespaces.
        /// </summary>
        /// <returns>The trimmed <see cref="StringSegment"/>.</returns>
        public unsafe StringSegment TrimStart()
        {
            var trimmedStart = this.Offset;
            var length = this.Offset + this.Length;

            fixed (char* p = this.Buffer)
            {
                while (trimmedStart < length)
                {
                    var c = p[trimmedStart];

                    if (!char.IsWhiteSpace(c))
                    {
                        break;
                    }

                    trimmedStart++;
                }
            }

            return new StringSegment(this.Buffer, trimmedStart, length - trimmedStart);
        }

        /// <summary>
        /// Removes all trailing whitespaces.
        /// </summary>
        /// <returns>The trimmed <see cref="StringSegment"/>.</returns>
        public unsafe StringSegment TrimEnd()
        {
            var offset = this.Offset;
            var trimmedEnd = offset + this.Length - 1;

            fixed (char* p = this.Buffer)
            {
                while (trimmedEnd >= offset)
                {
                    var c = p[trimmedEnd];

                    if (!char.IsWhiteSpace(c))
                    {
                        break;
                    }

                    trimmedEnd--;
                }
            }

            return new StringSegment(this.Buffer, offset, trimmedEnd - offset + 1);
        }

        /// <summary>
        /// Splits a string into <see cref="StringSegment"/>s that are based on the characters in an array.
        /// </summary>
        /// <param name="chars">A character array that delimits the substrings in this string, an empty array that
        /// contains no delimiters, or null.</param>
        /// <returns>An <see cref="StringTokenizer"/> whose elements contain the <see cref="StringSegment"/>s from this instance
        /// that are delimited by one or more characters in <paramref name="chars"/>.</returns>
        /*public StringTokenizer Split(char[] chars)
        {
            return new StringTokenizer(this, chars);
        }*/

        /// <summary>
        /// Returns the <see cref="string"/> represented by this <see cref="StringSegment"/> or <see cref="String.Empty" /> if the <see cref="StringSegment"/> does not contain a value.
        /// </summary>
        /// <returns>The <see cref="string"/> represented by this <see cref="StringSegment"/> or <see cref="String.Empty" /> if the <see cref="StringSegment"/> does not contain a value.</returns>
        public override string ToString()
        {
            return this.Value ?? string.Empty;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullOrEmpty() => this.Buffer == null || this.Length == 0;

        public bool IsNullOrWhiteSpace()
        {
            if (!this.HasValue)
                return true;
            for (var index = 0; index < this.Length; ++index)
            {
                if (!char.IsWhiteSpace(this.GetChar(index)))
                    return false;
            }
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetChar( int index) => this.Buffer[this.Offset + index];
    }
    
    public class StringSegmentComparer : IComparer<StringSegment>, IEqualityComparer<StringSegment>
    {
        public static StringSegmentComparer Ordinal { get; }
            = new StringSegmentComparer(StringComparison.Ordinal, StringComparer.Ordinal);

        public static StringSegmentComparer OrdinalIgnoreCase { get; }
            = new StringSegmentComparer(StringComparison.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

        private StringSegmentComparer(StringComparison comparison, StringComparer comparer)
        {
            this.Comparison = comparison;
            this.Comparer = comparer;
        }

        private StringComparison Comparison { get; }
        private StringComparer Comparer { get; }

        public int Compare(StringSegment x, StringSegment y)
        {
            return StringSegment.Compare(x, y, this.Comparison);
        }
		
        public bool Equals(StringSegment x, StringSegment y)
        {
            return StringSegment.Equals(x, y, this.Comparison);
        }

        public int GetHashCode(StringSegment obj)
        {
            return string.GetHashCode(obj.AsSpan(), this.Comparison);
        }
    }
    
    public static class Extensions
    {
        /// <summary>
        /// Add the given <see cref="StringSegment"/> to the <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to add to.</param>
        /// <param name="segment">The <see cref="StringSegment"/> to add.</param>
        /// <returns>The original <see cref="StringBuilder"/>.</returns>
        public static StringBuilder Append(this StringBuilder builder, StringSegment segment)
        {
            return builder.Append(segment.Buffer, segment.Offset, segment.Length);
        }
    }
}