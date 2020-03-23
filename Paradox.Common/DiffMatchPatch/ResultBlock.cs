using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ReactiveUI;

namespace Paradox.Common.DiffMatchPatch
{
    public sealed class ResultBlock
    {
        private readonly string _left;
        private readonly string _effectiveLeft;
        private readonly string _right;
        private readonly string _effectiveRight;
        private string _effectiveResult;

        private bool _isSelected;

        private readonly bool _hasLeft;
        private readonly bool _hasRight;

        public bool IsConflict { get; }
        public bool IsEqual { get; }
        public bool IsWhiteSpace { get; }

        public ResultBlock PrevBlock { get; private set; }
        public ResultBlock NextBlock { get; set; }

        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if (this._isSelected == value) return;
                this._isSelected = value;
                this.RedrawRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string this[Side side]
        {
            get
            {
                switch (side)
                {
                    case Side.Left: return this._effectiveLeft;
                    case Side.Right: return this._effectiveRight;
                }

                return this._effectiveResult;
            }
        }

        public bool HasSide(Side side)
        {
            switch (side)
            {
                case Side.Left: return this._hasLeft;
                case Side.Right: return this._hasRight;
            }
            return true;
        }

        public ICommand TakeLeft { get; }
        public ICommand TakeRight { get; }
        public ICommand TakeLeftThenRight { get; }
        public ICommand TakeRightThenLeft { get; }

        private ResultBlock(ResultBlock previous)
        {
            this.TakeLeft = ReactiveCommand.Create(() => this.ResolveAs(Side.Left));
            this.TakeRight = ReactiveCommand.Create(() => this.ResolveAs(Side.Right));
            this.TakeLeftThenRight = ReactiveCommand.Create(() => this.ResolveAs(Side.Left, Side.Right));
            this.TakeRightThenLeft = ReactiveCommand.Create(() => this.ResolveAs(Side.Right, Side.Left));
            this.PrevBlock = previous;
            if (this.PrevBlock != null)
            {
                this.PrevBlock.NextBlock = this;
            }
        }

        public ResultBlock(string both, ResultBlock previous)
            : this(previous)
        {
            this._left = this._right = this._effectiveLeft = this._effectiveRight = this._effectiveResult = both;
            this.IsEqual = this._hasLeft = this._hasRight = true;
            this.IsWhiteSpace = string.IsNullOrWhiteSpace(both);
        }


        public ResultBlock(string either, bool isAdd, ResultBlock previous)
            : this(previous)
        {
            if (isAdd)
            {
                this._left = this._effectiveLeft = this._effectiveResult = either;
                var rightLines = this._left.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                this._right = this._effectiveRight = string.Join(Environment.NewLine, rightLines.Select(l => new string(' ', l.Length)));
                this._hasLeft = true;
            }
            else
            {
                this._right = this._effectiveRight = this._effectiveResult = either;
                var leftLines = this._right.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                this._left = this._effectiveLeft = string.Join(Environment.NewLine, leftLines.Select(l => new string(' ', l.Length)));
                this._hasRight = true;
            }
            this.IsWhiteSpace = string.IsNullOrWhiteSpace(either);
        }

        public ResultBlock(string left, string right, ResultBlock previous)
            : this(previous)
        {
            var totalLen = Math.Max(left.Length, right.Length);

            var leftLines = left.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var rightLines = right.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            this._effectiveLeft = left;
            this._effectiveRight = right;

            if (leftLines.Length < rightLines.Length)
            {
                var contents = string.Join(Environment.NewLine, rightLines.Skip(leftLines.Length).Select(s => new string(' ', s.Length)) );

                this._effectiveLeft = $"{this._effectiveLeft}{Environment.NewLine}{contents}";
            }
            else if (leftLines.Length > rightLines.Length)
            {
                var contents = string.Join(Environment.NewLine, leftLines.Skip(rightLines.Length).Select(s => new string(' ', s.Length)) );

                this._effectiveRight = $"{this._effectiveRight}{Environment.NewLine}{contents}";
            }

            this._effectiveResult = string.Empty;
            var lineCount = Math.Max(leftLines.Length, rightLines.Length) ;

            for (var i = 0; i < lineCount; i++)
            {
                var l = i < leftLines.Length ? leftLines[i] : string.Empty;
                var r = i < rightLines.Length ? rightLines[i] : string.Empty;

                var ll = Math.Max(l.Length, r.Length);

                this._effectiveResult += string.Concat(new string(' ', ll), Environment.NewLine);
            }

            this._left = left;
            this._right = right;

            this._hasLeft = this._hasRight = true;

            this.IsConflict = true;
            this.IsWhiteSpace = string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right);
        }

        public void ResolveAs(Side side)
        {
            ResultBlock result;
            if (!this.HasSide(side))
            {
                result = new ResultBlock(string.Empty, this.PrevBlock);
            }
            else if (side == Side.Left)
            {
                result = new ResultBlock(this._left, this.PrevBlock);
            }
            else
            {
                result = new ResultBlock(this._right, this.PrevBlock);
            }

            result.NextBlock = this.NextBlock;
            if (this.NextBlock != null)
            {
                this.NextBlock.PrevBlock = result;
            }

            this.RebuildRequested?.Invoke(this, new RebuildRequestEventArgs(result));
        }

        public void ResolveAs(Side first, Side second)
        {
            ResultBlock firstBlock = null;
            ResultBlock secondBlock = null;

            var current = this;
            if (this.HasSide(first))
            {
                var result = first == Side.Left ? new ResultBlock(this._left, this.PrevBlock) : new ResultBlock(this._right, this.PrevBlock);

                result.NextBlock = this.NextBlock;

                if (this.NextBlock != null)
                {
                    current.NextBlock.PrevBlock = result;
                }

                firstBlock = current = result;
            }

            if (this.HasSide(second))
            {
                var prev = this.HasSide(first) ? current : this.PrevBlock;

                var result = second == Side.Left ? new ResultBlock(this._left, prev) : new ResultBlock(this._right, prev);

                result.NextBlock = this.NextBlock;

                if (this.NextBlock != null)
                {
                    current.NextBlock.PrevBlock = result;
                }

                if (current == this)
                {
                    firstBlock = current = result;
                }
                else
                {
                    secondBlock = result;
                }
            }

            var args = new RebuildRequestEventArgs(firstBlock, secondBlock);

            this.RebuildRequested?.Invoke(this, args);
            this.RebuildRequested = null;
        }

        public int Length(Side side)
        {
            return this[side].Length;
        }

        internal string GetAsString(Side side)
        {
            var sb = new StringBuilder();

            this.AddToString(sb, side);

            return sb.ToString();
        }

        private void AddToString(StringBuilder sb, Side side)
        {
            sb.Append(this[side]);
            this.NextBlock?.AddToString(sb, side);
        }

        internal void Update(int delta, int insertionLength, string insertedText, int removalLength)
        {
            // assumption is that the block is equal in both sides.

            var output = this._effectiveResult;

            if (insertionLength > 0)
            {
                output = output.Insert(delta, insertedText);
            }

            if (removalLength > 0)
            {
                output = output.Remove(delta, removalLength);
            }

            this._effectiveResult = output;

            this.RedrawRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RedrawRequested;
        public event EventHandler<RebuildRequestEventArgs> RebuildRequested;

    }
}