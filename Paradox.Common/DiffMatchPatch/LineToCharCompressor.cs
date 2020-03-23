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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paradox.Common.DiffMatchPatch
{
    internal sealed class LineToCharCompressor
    {
        /// <summary>
        /// Compresses all lines of a text to a series of indexes (starting at \u0001, ending at (char)text.Length)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLines"></param>
        /// <returns></returns>
        public string Compress(string text, int maxLines = char.MaxValue) 
            =>
                this.Encode(text, maxLines).Aggregate(new StringBuilder(), (sb, c) => sb.Append(c)).ToString();

        private IEnumerable<char> Encode(string text, int maxLines)
        {
            var start = 0;
            var end = -1;
            while (end < text.Length - 1)
            {
                end = this._lineArray.Count == maxLines ? text.Length - 1 : text.IndexOf('\n', start);
                if (end == -1)
                {
                    end = text.Length - 1;
                }
                var line = text.Substring(start, end + 1 - start);
                this.EnsureHashed(line);
                yield return this[line];
                start = end + 1;
            }
        }

        /// <summary>
        /// Decompresses a series of characters that was previously compressed back to the original lines of text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Decompress(string text) 
            => text.Aggregate(new StringBuilder(), (sb, c) => sb.Append(this[c])).Append(text.Length == char.MaxValue ? this[char.MaxValue] : "").ToString();

        // e.g. _lineArray[4] == "Hello\n"
        // e.g. _lineHash["Hello\n"] == 4
        private readonly List<string> _lineArray = new List<string>();
        private readonly Dictionary<string, char> _lineHash = new Dictionary<string, char>();

        private void EnsureHashed(string line)
        {
            if (this._lineHash.ContainsKey(line)) return;
            this._lineArray.Add(line);
            this._lineHash.Add(line, (char) (this._lineArray.Count - 1));
        }

        private char this[string line] => this._lineHash[line];
        private string this[int c] => this._lineArray[c];

    }
}