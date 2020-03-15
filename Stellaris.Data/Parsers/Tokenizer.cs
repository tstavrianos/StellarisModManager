using System.Collections.Generic;
using Stellaris.Data.Parsers.pck;

namespace Stellaris.Data.Parsers
{
    public sealed class Tokenizer : TableTokenizer
    {
        public const int OPERATOR = 14;
        public const int INT = 16;
        public const int PCT = 20;
        public const int REAL = 18;
        public const int DATE = 19;
        public const int STRING = 15;
        public const int SYMBOL = 17;
        public const int whitespace = 23;
        public const int lineComment = 24;
        public const int @implicit = 21;
        public const int implicit2 = 22;
        public const int _EOS = 25;
        public const int _ERROR = 26;

        private static readonly string[] _Symbols = new string[] {
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     null,
                                                                     "OPERATOR",
                                                                     "STRING",
                                                                     "INT",
                                                                     "SYMBOL",
                                                                     "REAL",
                                                                     "DATE",
                                                                     "PCT",
                                                                     "implicit",
                                                                     "implicit2",
                                                                     "whitespace",
                                                                     "lineComment",
                                                                     "#EOS",
                                                                     "#ERROR"};

        private static readonly string[] _BlockEnds = new string[] {
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null,
                                                                       null};

        private static readonly CharDfaEntry[] _DfaTable = new CharDfaEntry[] {
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '<',
                                                                                                                                                                                                      '<'}, 1),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '>',
                                                                                                                                                                                                      '>'}, 4),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '!',
                                                                                                                                                                                                      '!'}, 6),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '=',
                                                                                                                                                                                                      '='}, 8),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '+',
                                                                                                                                                                                                      '+',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-'}, 9),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 14),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\"',
                                                                                                                                                                                                      '\"'}, 22),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'A',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 27),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\t',
                                                                                                                                                                                                      '\t',
                                                                                                                                                                                                      '\n',
                                                                                                                                                                                                      '\n',
                                                                                                                                                                                                      '',
                                                                                                                                                                                                      '\r',
                                                                                                                                                                                                      ' ',
                                                                                                                                                                                                      ' '}, 28),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '#',
                                                                                                                                                                                                      '#'}, 29),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '{',
                                                                                                                                                                                                      '{'}, 32),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '}',
                                                                                                                                                                                                      '}'}, 33)}),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '=',
                                                                                                                                                                                                      '='}, 2),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '>',
                                                                                                                                                                                                      '>'}, 3)}),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '=',
                                                                                                                                                                                                      '='}, 5)}),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '=',
                                                                                                                                                                                                      '='}, 7)}),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(14, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 10)}),
                                                                                          new CharDfaEntry(16, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 10),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%'}, 11),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.'}, 12)}),
                                                                                          new CharDfaEntry(20, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 13)}),
                                                                                          new CharDfaEntry(18, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 13)}),
                                                                                          new CharDfaEntry(16, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 15),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%'}, 16),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.'}, 18),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(16, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 15),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%'}, 16),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.'}, 18),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(20, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(17, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(17, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 19),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(18, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 19),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.'}, 20),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(17, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 21),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(19, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9'}, 21),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\0',
                                                                                                                                                                                                      '!',
                                                                                                                                                                                                      '#',
                                                                                                                                                                                                      '[',
                                                                                                                                                                                                      ']',
                                                                                                                                                                                                      ((char)(65535))}, 23),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\\',
                                                                                                                                                                                                      '\\'}, 24),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\"',
                                                                                                                                                                                                      '\"'}, 26)}),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\0',
                                                                                                                                                                                                      '!',
                                                                                                                                                                                                      '#',
                                                                                                                                                                                                      '[',
                                                                                                                                                                                                      ']',
                                                                                                                                                                                                      ((char)(65535))}, 23),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\\',
                                                                                                                                                                                                      '\\'}, 24),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\"',
                                                                                                                                                                                                      '\"'}, 26)}),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\0',
                                                                                                                                                                                                      ((char)(65535))}, 25)}),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\0',
                                                                                                                                                                                                      '!',
                                                                                                                                                                                                      '#',
                                                                                                                                                                                                      '[',
                                                                                                                                                                                                      ']',
                                                                                                                                                                                                      ((char)(65535))}, 23),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\\',
                                                                                                                                                                                                      '\\'}, 24),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\"',
                                                                                                                                                                                                      '\"'}, 26)}),
                                                                                          new CharDfaEntry(15, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(17, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '%',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '-',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '.',
                                                                                                                                                                                                      '0',
                                                                                                                                                                                                      '9',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      ':',
                                                                                                                                                                                                      '@',
                                                                                                                                                                                                      'Z',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      '_',
                                                                                                                                                                                                      'a',
                                                                                                                                                                                                      'z'}, 17)}),
                                                                                          new CharDfaEntry(23, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\t',
                                                                                                                                                                                                      '\t',
                                                                                                                                                                                                      '\n',
                                                                                                                                                                                                      '\n',
                                                                                                                                                                                                      '',
                                                                                                                                                                                                      '\r',
                                                                                                                                                                                                      ' ',
                                                                                                                                                                                                      ' '}, 28)}),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\0',
                                                                                                                                                                                                      '\t',
                                                                                                                                                                                                      '',
                                                                                                                                                                                                      ((char)(65535))}, 30),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\n',
                                                                                                                                                                                                      '\n'}, 31)}),
                                                                                          new CharDfaEntry(-1, new CharDfaTransitionEntry[] {
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\0',
                                                                                                                                                                                                      '\t',
                                                                                                                                                                                                      '',
                                                                                                                                                                                                      ((char)(65535))}, 30),
                                                                                                                                                        new CharDfaTransitionEntry(new char[] {
                                                                                                                                                                                                      '\n',
                                                                                                                                                                                                      '\n'}, 31)}),
                                                                                          new CharDfaEntry(24, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(21, new CharDfaTransitionEntry[0]),
                                                                                          new CharDfaEntry(22, new CharDfaTransitionEntry[0])};
        public Tokenizer(IEnumerable<char> input) :
            base(_DfaTable, _Symbols, _BlockEnds, input)
        {
        }
    }
}
