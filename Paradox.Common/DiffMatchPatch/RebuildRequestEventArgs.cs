using System;

namespace Paradox.Common.DiffMatchPatch
{
    public sealed class RebuildRequestEventArgs : EventArgs
    {
        public ResultBlock First { get; }
        public ResultBlock Second { get; }

        public RebuildRequestEventArgs(ResultBlock first, ResultBlock second)
        {
            this.First = first;
            this.Second = second;
        }

        public RebuildRequestEventArgs(ResultBlock first)
        {
            this.First = first;
        }
    }
}