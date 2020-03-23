using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradox.Common.DiffMatchPatch
{
    public class Comparison
    {
        public ResultBlock Root { get; private set; }

        public Comparison(IReadOnlyList<Diff> source)
        {
            ResultBlock current = null;
            ResultBlock result;

            var i = 0;
            for (; i < source.Count - 1; i++)
            {
                if (source[i].Operation == Operation.Equal)
                {
                    result = new ResultBlock(source[i].Text, current);
                }
                else if (source[i + 1].Operation != Operation.Equal)
                {
                    var left = source[i].Operation == Operation.Delete ? source[i].Text : source[i + 1].Text;
                    var right = source[i].Operation == Operation.Delete ? source[i + 1].Text : source[i].Text;
                    result = new ResultBlock(left, right, current);
                    i += 1;
                }
                else
                {
                    result = new ResultBlock(source[i].Text, source[i].Operation == Operation.Delete, current);
                }

                if (current == null)
                {
                    this.Root = result;
                }

                current = result;
                result.RebuildRequested += this.Result_RebuildRequested;
                result.RedrawRequested += this.Result_RedrawRequested;
            }

            if (i >= source.Count) return;
            result = source.Last().Operation == Operation.Equal ? new ResultBlock(source[i].Text, current) : new ResultBlock(source[i].Text, source[i].Operation == Operation.Delete, current);

            if (current == null)
            {
                this.Root = result;
            }

            result.RebuildRequested += this.Result_RebuildRequested;
            result.RedrawRequested += this.Result_RedrawRequested;
        }

        public override string ToString()
        {
            return this.Root?.GetAsString(Side.Result);
        }

        private void Result_RedrawRequested(object sender, EventArgs e)
        {
            this.RedrawRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Result_RebuildRequested(object sender, RebuildRequestEventArgs e)
        {
            if (this.Root == sender)
            {
                this.Root = e.First;
            }

            (sender as ResultBlock).RebuildRequested -= this.Result_RebuildRequested;
            this.RebuildRequested?.Invoke(sender, e);
        }

        internal CalculatedBlock GetBlockContainingOffset(int offset, Side side)
        {
            var current = this.Root;

            var start = 0;

            while (current != null)
            {
                var len = current.Length(side);
                if (start <= offset && (start + len) > offset)
                {
                    return new CalculatedBlock
                    {
                        Side = side,
                        Offset = start,
                        EndOffset = start + len,
                        Block = current
                    };
                }
                start += len;
                current = current.NextBlock;
            }
            return null;
        }

        internal int GetOffsetToBlock(ResultBlock block, Side side)
        {
            var current = this.Root;
            var start = 0;

            while (current != null)
            {
                if (current == block)
                {
                    return start;
                }
                start += current.Length(side);
                current = current.NextBlock;
            }

            return -1;
        }

        internal void Append(string insertedText)
        {
            var block = this.Root;
            while (block?.NextBlock != null)
            {
                block = block.NextBlock;
            }

            var newBlock = new ResultBlock(insertedText, block);
        }

        public event EventHandler RedrawRequested;
        public event EventHandler<RebuildRequestEventArgs> RebuildRequested;
    }
}