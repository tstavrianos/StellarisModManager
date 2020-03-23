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
using System.Collections.Generic;
using System.Threading;

namespace Paradox.Common.DiffMatchPatch
{
    public struct Diff
    {
        internal static Diff Create(Operation operation, string text) => new Diff(operation, text);
        internal static Diff Equal(string text) => Create(Operation.Equal, text);
        internal static Diff Insert(string text) => Create(Operation.Insert, text);
        internal static Diff Delete(string text) => Create(Operation.Delete, text);

        public readonly Operation Operation;
        // One of: INSERT, DELETE or EQUAL.
        public readonly string Text;
        // The text associated with this diff operation.

        private Diff(Operation operation, string text)
        {
            // Construct a diff with the specified operation and text.
            this.Operation = operation;
            this.Text = text;
        }

        /// <summary>
        /// Generate a human-readable version of this Diff.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var prettyText = this.Text.Replace('\n', '\u00b6');
            return "Diff(" + this.Operation + ",\"" + prettyText + "\")";
        }

        /// <summary>
        /// Is this Diff equivalent to another Diff?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => !ReferenceEquals(obj, null) && this.Equals((Diff) obj);

        public bool Equals(Diff obj) => obj.Operation == this.Operation && obj.Text == this.Text;

        public static bool operator==(Diff left, Diff right) => left.Equals(right);

        public static bool operator !=(Diff left, Diff right) => !(left == right);


        public override int GetHashCode() => this.Text.GetHashCode() ^ this.Operation.GetHashCode();

        internal Diff Replace(string toString) => Create(this.Operation, toString);

        internal Diff Copy() => Create(this.Operation, this.Text);

        /// <summary>
        /// Find the differences between two texts.
        /// </summary>
        /// <param name="text1">Old string to be diffed</param>
        /// <param name="text2">New string to be diffed</param>
        /// <param name="timeoutInSeconds">if specified, certain optimizations may be enabled to meet the time constraint, possibly resulting in a less optimal diff</param>
        /// <param name="checklines">If false, then don't run a line-level diff first to identify the changed areas. If true, then run a faster slightly less optimal diff.</param>
        /// <returns></returns>
        public static List<Diff> Compute(string text1, string text2, float timeoutInSeconds = 0f, bool checklines = true)
        {
            using (var cts = timeoutInSeconds <= 0 
                ? new CancellationTokenSource() 
                : new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds))
                )
            {
                return Compute(text1, text2, checklines, cts.Token, timeoutInSeconds > 0);
            }
        }

        public static List<Diff> Compute(string text1, string text2, bool checkLines, CancellationToken token, bool optimizeForSpeed) 
            => DiffAlgorithm.Compute(text1, text2, checkLines, token, optimizeForSpeed);
    }
}