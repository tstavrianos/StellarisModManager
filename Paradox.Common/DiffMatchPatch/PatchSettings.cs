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

namespace Paradox.Common.DiffMatchPatch
{
    public struct PatchSettings
    {
        /// <summary>
        /// When deleting a large block of text (over ~64 characters), how close
        /// do the contents have to be to match the expected contents. (0.0 =
        /// perfection, 1.0 = very loose).  Note that Match_Threshold controls
        /// how closely the end points of a delete need to match.
        /// </summary>
        public float PatchDeleteThreshold { get; }

        /// <summary>
        /// Chunk size for context length.
        /// </summary>
        public short PatchMargin { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="patchDeleteTreshold"> How far to search for a match (0 = exact location, 1000+ = broad match).
        /// A match this many characters away from the expected location will add
        /// 1.0 to the score (0.0 is a perfect match).</param>
        /// <param name="patchMargin">Chunk size for context length.</param>
        public PatchSettings(float patchDeleteTreshold, short patchMargin)
        {
            this.PatchDeleteThreshold = patchDeleteTreshold;
            this.PatchMargin = patchMargin;
        }

        public static PatchSettings Default { get; } = new PatchSettings(0.5f, 4);
    }
}