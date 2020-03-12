//https://github.com/Vanlightly/DslParser

using System;

namespace Stellaris.Data.Parsers
{
    public sealed class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }
}