namespace Paradox.Common.DiffMatchPatch
{
    public sealed class CalculatedBlock
    {
        public Side Side { get; set; }
        public int Offset { get; set; }
        public int EndOffset { get; set; }
        public ResultBlock Block { get; set; }

        public CalculatedBlock GetNext()
        {
            var nextBlock = this.Block.NextBlock;
            if (nextBlock == null)
            {
                return null;
            }

            return new CalculatedBlock
            {
                Side = this.Side,
                Offset = this.EndOffset,
                EndOffset = this.EndOffset + nextBlock.Length(this.Side),
                Block = nextBlock
            };
        }
    }
}