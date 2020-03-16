using StellarisModManager.Core.Parsers.pck;

namespace StellarisModManager.Core.Parsers
{
    public sealed class Parser : Lalr1TableParser
    {
        public const int config = 0;
        public const int assignment = 1;
        public const int assignmentList = 2;
        public const int field = 3;
        public const int value = 4;
        public const int valueList = 5;
        public const int symbol = 6;
        public const int @string = 7;
        public const int integer = 8;
        public const int real = 9;
        public const int date = 10;
        public const int percent = 11;
        public const int map = 12;
        public const int array = 13;
        public const int OPERATOR = 14;
        public const int STRING = 15;
        public const int INT = 16;
        public const int SYMBOL = 17;
        public const int REAL = 18;
        public const int DATE = 19;
        public const int PCT = 20;
        public const int @implicit = 21;
        public const int implicit2 = 22;
        public const int whitespace = 23;
        public const int lineComment = 24;
        public const int _EOS = 25;
        public const int _ERROR = 26;

        private static readonly string[] _Symbols = new string[] {
                                                                     "config",
                                                                     "assignment",
                                                                     "assignmentList",
                                                                     "field",
                                                                     "value",
                                                                     "valueList",
                                                                     "symbol",
                                                                     "string",
                                                                     "integer",
                                                                     "real",
                                                                     "date",
                                                                     "percent",
                                                                     "map",
                                                                     "array",
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

        private static readonly int[][][] _ParseTable = new int[][][] {
                                                                          new int[][] {
                                                                                          new int[] {
                                                                                                        1},
                                                                                          new int[] {
                                                                                                        3},
                                                                                          new int[] {
                                                                                                        2},
                                                                                          new int[] {
                                                                                                        5},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        33},
                                                                                          new int[] {
                                                                                                        32},
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        34},
                                                                                          new int[] {
                                                                                                        21},
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        -1}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        0,
                                                                                                        0,
                                                                                                        2}},
                                                                          new int[][] {
                                                                                          null,
                                                                                          new int[] {
                                                                                                        3},
                                                                                          new int[] {
                                                                                                        4},
                                                                                          new int[] {
                                                                                                        5},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        33},
                                                                                          new int[] {
                                                                                                        32},
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        34},
                                                                                          new int[] {
                                                                                                        21},
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        3,
                                                                                                        2,
                                                                                                        1},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        3,
                                                                                                        2,
                                                                                                        1}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        4,
                                                                                                        2,
                                                                                                        1,
                                                                                                        2},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        4,
                                                                                                        2,
                                                                                                        1,
                                                                                                        2}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        6},
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
                                                                                          null},
                                                                          new int[][] {
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        7},
                                                                                          null,
                                                                                          new int[] {
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        14},
                                                                                          new int[] {
                                                                                                        15},
                                                                                          null,
                                                                                          new int[] {
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        21},
                                                                                          new int[] {
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        22},
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        2,
                                                                                                        1,
                                                                                                        3,
                                                                                                        14,
                                                                                                        4},
                                                                                          new int[] {
                                                                                                        2,
                                                                                                        1,
                                                                                                        3,
                                                                                                        14,
                                                                                                        4},
                                                                                          new int[] {
                                                                                                        2,
                                                                                                        1,
                                                                                                        3,
                                                                                                        14,
                                                                                                        4},
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        2,
                                                                                                        1,
                                                                                                        3,
                                                                                                        14,
                                                                                                        4},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        2,
                                                                                                        1,
                                                                                                        3,
                                                                                                        14,
                                                                                                        4}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        7,
                                                                                                        4,
                                                                                                        8}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        8,
                                                                                                        4,
                                                                                                        11}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        9,
                                                                                                        4,
                                                                                                        9}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        10,
                                                                                                        4,
                                                                                                        10}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        13,
                                                                                                        4,
                                                                                                        12}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        14,
                                                                                                        4,
                                                                                                        13}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        24,
                                                                                                        11,
                                                                                                        20}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        22,
                                                                                                        9,
                                                                                                        18}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        23,
                                                                                                        10,
                                                                                                        19}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        20,
                                                                                                        7,
                                                                                                        15}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        21,
                                                                                                        8,
                                                                                                        16}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        19,
                                                                                                        6,
                                                                                                        17}},
                                                                          new int[][] {
                                                                                          null,
                                                                                          new int[] {
                                                                                                        3},
                                                                                          new int[] {
                                                                                                        23},
                                                                                          new int[] {
                                                                                                        5},
                                                                                          new int[] {
                                                                                                        28},
                                                                                          new int[] {
                                                                                                        26},
                                                                                          new int[] {
                                                                                                        31},
                                                                                          new int[] {
                                                                                                        30},
                                                                                          new int[] {
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        14},
                                                                                          new int[] {
                                                                                                        15},
                                                                                          null,
                                                                                          new int[] {
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        21},
                                                                                          new int[] {
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        24},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        25,
                                                                                                        12,
                                                                                                        21,
                                                                                                        2,
                                                                                                        22}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        26,
                                                                                                        12,
                                                                                                        21,
                                                                                                        22}},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        27},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        27,
                                                                                                        13,
                                                                                                        21,
                                                                                                        5,
                                                                                                        22}},
                                                                          new int[][] {
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        28},
                                                                                          new int[] {
                                                                                                        29},
                                                                                          new int[] {
                                                                                                        13},
                                                                                          new int[] {
                                                                                                        12},
                                                                                          new int[] {
                                                                                                        8},
                                                                                          new int[] {
                                                                                                        10},
                                                                                          new int[] {
                                                                                                        11},
                                                                                          new int[] {
                                                                                                        9},
                                                                                          new int[] {
                                                                                                        14},
                                                                                          new int[] {
                                                                                                        15},
                                                                                          null,
                                                                                          new int[] {
                                                                                                        19},
                                                                                          new int[] {
                                                                                                        20},
                                                                                          new int[] {
                                                                                                        21},
                                                                                          new int[] {
                                                                                                        17},
                                                                                          new int[] {
                                                                                                        18},
                                                                                          new int[] {
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        22},
                                                                                          new int[] {
                                                                                                        15,
                                                                                                        5,
                                                                                                        4},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        16,
                                                                                                        5,
                                                                                                        4,
                                                                                                        5},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        5,
                                                                                                        3,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          new int[] {
                                                                                                        11,
                                                                                                        4,
                                                                                                        7},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        6,
                                                                                                        3,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          new int[] {
                                                                                                        12,
                                                                                                        4,
                                                                                                        6},
                                                                                          null,
                                                                                          null,
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        5,
                                                                                                        3,
                                                                                                        7},
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
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        6,
                                                                                                        3,
                                                                                                        6},
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
                                                                                          null},
                                                                          new int[][] {
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
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16},
                                                                                          null,
                                                                                          null,
                                                                                          new int[] {
                                                                                                        18,
                                                                                                        6,
                                                                                                        16}}};

        private static readonly int[] _NodeFlags = new int[] {
                                                                 0,
                                                                 0,
                                                                 1,
                                                                 0,
                                                                 0,
                                                                 1,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 0,
                                                                 2,
                                                                 2,
                                                                 0,
                                                                 0};

        private static readonly int[] _Substitutions = new int[] {
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     -1};

        private static readonly ParseAttribute[][] _AttributeSets = new ParseAttribute[][] {
                                                                                                       new ParseAttribute[] {
                                                                                                                                    new ParseAttribute("start", true)},
                                                                                                       null,
                                                                                                       new ParseAttribute[] {
                                                                                                                                    new ParseAttribute("collapsed", true)},
                                                                                                       null,
                                                                                                       null,
                                                                                                       new ParseAttribute[] {
                                                                                                                                    new ParseAttribute("collapsed", true)},
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
                                                                                                       new ParseAttribute[] {
                                                                                                                                    new ParseAttribute("hidden", true)},
                                                                                                       new ParseAttribute[] {
                                                                                                                                    new ParseAttribute("hidden", true)},
                                                                                                       null,
                                                                                                       null};
        public Parser(ITokenizer tokenizer) :
            base(_ParseTable, _Symbols, _NodeFlags, _Substitutions, _AttributeSets, tokenizer)
        {
        }
        public Parser() :
            this(null)
        {
        }
    }
}