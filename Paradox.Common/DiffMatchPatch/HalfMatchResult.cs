/*
 * Copyright 2008 Google Inc. All Rights Reserved.
 * Author: fraser@google.com (Neil Fraser)
 * Author: anteru@developer.shelter13.net (Matthaeus G. Chajdas)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Diff Match and Patch
 * http://code.google.com/p/google-diff-match-patch/
 */

using System;

namespace Paradox.Common.DiffMatchPatch
{
    internal struct HalfMatchResult : IEquatable<HalfMatchResult>
    {
        public HalfMatchResult(string prefix1, string suffix1, string prefix2, string suffix2, string commonMiddle)
        {
            this.Prefix1 = prefix1 ?? throw new ArgumentNullException(nameof(prefix1));
            this.Suffix1 = suffix1 ?? throw new ArgumentNullException(nameof(suffix1));
            this.Prefix2 = prefix2 ?? throw new ArgumentNullException(nameof(prefix2));
            this.Suffix2 = suffix2 ?? throw new ArgumentNullException(nameof(suffix2));
            this.CommonMiddle = commonMiddle ?? throw new ArgumentNullException(nameof(commonMiddle));
        }

        public HalfMatchResult Reverse()
        {
            return new HalfMatchResult(this.Prefix2, this.Suffix2, this.Prefix1, this.Suffix1, this.CommonMiddle);
        }

        public string Prefix1 { get; }
        public string Suffix1 { get; }
        public string CommonMiddle { get; }
        public string Prefix2 { get; }
        public string Suffix2 { get; }
        public bool IsEmpty => string.IsNullOrEmpty(this.CommonMiddle);

        public static readonly HalfMatchResult Empty = new HalfMatchResult();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == this.GetType() && this.Equals((HalfMatchResult) obj);
        }

        public bool Equals(HalfMatchResult other)
        {
            return string.Equals(this.Prefix1, other.Prefix1) && string.Equals(this.Suffix1, other.Suffix1) && string.Equals(this.CommonMiddle, other.CommonMiddle) && string.Equals(this.Prefix2, other.Prefix2) && string.Equals(this.Suffix2, other.Suffix2);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (this.Prefix1 != null ? this.Prefix1.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (this.Suffix1 != null ? this.Suffix1.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (this.CommonMiddle != null ? this.CommonMiddle.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (this.Prefix2 != null ? this.Prefix2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (this.Suffix2 != null ? this.Suffix2.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(HalfMatchResult left, HalfMatchResult right) => Equals(left, right);

        public static bool operator !=(HalfMatchResult left, HalfMatchResult right) => !Equals(left, right);

        public static bool operator >(HalfMatchResult left, HalfMatchResult right) => left.CommonMiddle.Length > right.CommonMiddle.Length;

        public static bool operator <(HalfMatchResult left, HalfMatchResult right) => left.CommonMiddle.Length < right.CommonMiddle.Length;

        public override string ToString() => $"[{this.Prefix1}/{this.Prefix2}] - {this.CommonMiddle} - [{this.Suffix1}/{this.Suffix2}]";
    }
}