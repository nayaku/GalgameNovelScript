using System.Collections.Generic;

namespace GalgameNovelScript
{
    public enum TokenType
    {
        NAME,
        INT_CONST,
        REAL_CONST,
        STR,
        NEWLINE,
        INDENT,
        DEDENT,
        COLON,
        LPAREN,
        RPAREN,
        LBRACK,
        RBRACK,
        ASSIGN,
        COMMENT,
        DOT,
        IF,
        ELSE,
        ELIF,
        CASE,
        WHEN,
        EQ,
        NE,
        LT,
        LE,
        GT,
        GE,
        PLUS,
        MINUS,
        MUL,
        DIV,
        FLOORDIV,
        MOD,
        POW,
        NOT,
        AND,
        OR,
        TRUE,
        FALSE,
        NONE,
        EOF
    }
    public class Token
    {
        public TokenType Type { get; set; }
        public object Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public static Dictionary<string, TokenType> ReservedKeywords { get; } = new Dictionary<string, TokenType>()
        {
            {"的",TokenType.DOT },
            {"如果",TokenType.IF},
            {"否则",TokenType.ELSE},
            {"否则如果",TokenType.ELIF},
            {"选择",TokenType.CASE},
            {"选项",TokenType.WHEN},
            {"非",TokenType.NOT},
            {"并且",TokenType.AND},
            {"或者",TokenType.OR},
            {"幂",TokenType.POW},
            {"true",TokenType.TRUE},
            {"false",TokenType.FALSE},
            {"none",TokenType.NONE},
            {"真",TokenType.TRUE},
            {"假",TokenType.FALSE},
            {"开",TokenType.TRUE},
            {"关",TokenType.FALSE},
            {"空",TokenType.NONE},
        };
        public static string ChineseSymbol { get; } = "（）【】：“”‘’　";

        public Token(TokenType type, object value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
        public static Token GetReservedKeywords(string value, int line, int column)
        {
            if (ReservedKeywords.ContainsKey(value))
                return new Token(ReservedKeywords[value], value, line, column);
            return null;
        }
        public override string ToString()
        {
            return string.Format("Token({0}, {1}, position = {2}:{3})", Type, Value, Line, Column);
        }
    }
}
