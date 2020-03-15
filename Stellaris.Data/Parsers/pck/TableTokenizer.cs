using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Stellaris.Data.Parsers.pck
{
    public class TableTokenizer : ITokenizer
    {
        private readonly CharDfaEntry[] _dfaTable;

        private readonly int _eosSymbol;

        private readonly int _errorSymbol;

        private IEnumerable<char> _input;

        private readonly string[] _symbols;

        private readonly string[] _blockEnds;

        protected TableTokenizer(CharDfaEntry[] dfaTable, string[] symbols, string[] blockEnds, IEnumerable<char> input)
        {
            this._dfaTable = dfaTable;
            this._symbols = symbols;
            this._blockEnds = blockEnds;
            this._input = input;
            this._eosSymbol = -1;
            this._errorSymbol = -1;
            for (var i = symbols.Length - 1; i >= 0; i--)
            {
                switch (symbols[i])
                {
                    case "#ERROR":
                        this._errorSymbol = i;
                        break;
                    case "#EOS":
                        this._eosSymbol = i;
                        break;
                }

                if (-1 != this._errorSymbol && -1 != this._eosSymbol)
                    break;
            }
            if (-1 == this._errorSymbol || -1 == this._eosSymbol)
                throw new ArgumentException("Error in symbol table", nameof(symbols));


        }
        public IEnumerator<Token> GetEnumerator()
        {
            return new TokenEnumerator(this._dfaTable, this._errorSymbol, this._eosSymbol, this._symbols, this._blockEnds, this._input);
        }

        public void Restart(IEnumerable<char> input)
        {
            this._input = input;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        private sealed class TokenEnumerator : IEnumerator<Token>
        {
            private readonly string[] _blockEnds;

            private readonly string[] _symbols;
            // our underlying input enumerator - works on strings or char arrays
            private IEnumerator<char> _input;
            // location information
            private long _position;

            private int _line;

            private int _column;

            private readonly int _errorSymbol;

            private readonly int _eosSymbol;
            // an integer we use so we can tell if the enumeration is started or running, or past the end.
            private int _state;
            // this holds the current token we're on.
            private Token _token;

            // the DFA Table is a composite "regular expression" with tagged symbols for each one.
            private readonly CharDfaEntry[] _dfaTable;
            // this holds our current value
            private readonly StringBuilder _buffer;
            internal TokenEnumerator(CharDfaEntry[] dfaTable, int errorSymbol, int eosSymbol, string[] symbols, string[] blockEnds, IEnumerable<char> @string)
            {
                this._dfaTable = dfaTable;
                this._errorSymbol = errorSymbol;
                this._eosSymbol = eosSymbol;
                this._symbols = symbols;
                this._blockEnds = blockEnds;
                if (null != @string)
                    this._input = @string.GetEnumerator();
                this._buffer = new StringBuilder();
                this._state = -1;
                this._line = 1;
                this._column = 1;
                this._position = 0;
            }
            public Token Current => this._token;
            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this._state = -3;
                if (null == this._input) return;
                this._input.Dispose();
                this._input = null;
            }
            public bool MoveNext()
            {
                switch (this._state)
                {
                    case -3:
                        throw new ObjectDisposedException(this.GetType().FullName);
                    case -2:
                        if (this._token.SymbolId == this._eosSymbol) return false;
                        this._state = -2;
                        goto case 0;
                        return false;
                    case -1:
                    case 0:
                        this._token = new Token();
                        // store our current location before we advance
                        this._token.Column = this._column;
                        this._token.Line = this._line;
                        this._token.Position = this._position;
                        // this is where the real work happens:
                        this._token.SymbolId = this._Lex();
                        this._token.Symbol = this._symbols[this._token.SymbolId];
                        // store our value and length from the lex
                        this._token.Value = this._buffer.ToString();
                        this._token.Length = this._buffer.Length;
                        return true;
                    default:
                        return false;
                }

            }
            /// <summary>
            /// This is where the work happens
            /// </summary>
            /// <returns>The symbol that was matched. members _state _line,_column,_position,_buffer and _input are also modified.</returns>
            private int _Lex()
            {
                int acc;
                var state = 0;
                this._buffer.Clear();
                switch (this._state)
                {
                    case -1: // initial
                        if (!this._MoveNextInput())
                        {
                            this._state = -2;
                            acc = this._dfaTable[state].AcceptSymbolId;
                            return -1 != acc ? acc : this._errorSymbol;
                        }
                        this._state = 0; // running
                        break;
                    case -2: // end of stream
                        return this._eosSymbol;
                }
                // Here's where we run most of the match. we run one interation of the DFA state machine.
                // We match until we can't match anymore (greedy matching) and then report the symbol of the last 
                // match we found, or an error ("#ERROR") if we couldn't find one.
                while (true)
                {
                    var next = -1;
                    // go through all the transitions
                    foreach (var entry in this._dfaTable[state].Transitions)
                    {
                        var found = false;
                        // go through all the ranges to see if we matched anything.
                        for (var j = 0; j < entry.PackedRanges.Length; j++)
                        {
                            var ch = this._input.Current;
                            var first = entry.PackedRanges[j];
                            ++j;
                            var last = entry.PackedRanges[j];
                            if (ch > last) continue;
                            if (first > ch) break;
                            found = true;
                            break;

                        }

                        if (!found) continue;
                        // set the transition destination
                        next = entry.Destination;
                        break;
                    }

                    if (-1 == next) // couldn't find any states
                        break;
                    this._buffer.Append(this._input.Current);

                    state = next;
                    if (this._MoveNextInput()) continue;
                    // end of stream
                    this._state = -2;
                    acc = this._dfaTable[state].AcceptSymbolId;
                    return -1 != acc ? acc : this._errorSymbol;
                }
                acc = this._dfaTable[state].AcceptSymbolId;
                if (-1 != acc) // do we accept?
                {
                    var be = this._blockEnds[acc];
                    if (string.IsNullOrEmpty(be)) return acc;
                    // we have to resolve our blockends. This is tricky. We break out of the FA 
                    // processing and instead we loop until we match the block end. We have to 
                    // be very careful when we match only partial block ends and we have to 
                    // handle the case where there's no terminating block end.
                    var more = true;
                    while (more)
                    {
                        while (more)
                        {
                            if (this._input.Current != be[0])
                            {
                                this._buffer.Append(this._input.Current);
                                more = this._MoveNextInput();
                                if (!more)
                                    return this._errorSymbol;
                                break;
                            }

                            var i = 0;
                            var found = true;
                            while (i < be.Length && this._input.Current == be[i])
                            {
                                if (!(more = this._MoveNextInput()))
                                {
                                    ++i;
                                    found = false;
                                    if (i < be.Length)
                                        acc = this._errorSymbol;
                                    break;
                                }
                                ++i;

                            }
                            if (be.Length != i)
                                found = false;
                            if (!found)
                            {
                                this._buffer.Append(be.Substring(0, i));
                            }
                            else
                            {
                                more = false;
                                this._buffer.Append(be);
                                break;
                            }

                            if (!found) continue;
                            more = this._MoveNextInput();
                            if (!more)
                                break;

                        }
                    }
                    return acc;
                }

                // handle the error condition
                this._buffer.Append(this._input.Current);
                if (!this._MoveNextInput())
                    this._state = -2;
                return this._errorSymbol;
            }
            /// <summary>
            /// Advances the input, and tracks location information
            /// </summary>
            /// <returns>True if the underlying MoveNext returned true, otherwise false.</returns>
            private bool _MoveNextInput()
            {
                if (this._input.MoveNext())
                {
                    if (-1 == this._state) return true;
                    ++this._position;
                    if ('\n' == this._input.Current)
                    {
                        this._column = 1;
                        ++this._line;
                    }
                    else
                        ++this._column;
                    return true;
                }

                if (0 == this._state)
                {
                    ++this._position;
                    ++this._column;
                }
                this._state = -2;
                return false;
            }

            public void Reset()
            {
                this._input.Reset();
                this._state = -1;
                this._line = 1;
                this._column = 1;
                this._position = 0;
                this._token = default(Token);
            }
        }
    }
}
