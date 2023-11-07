using System;
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
        public static Dictionary<string, TokenType> ReservedKeywords = new Dictionary<string, TokenType>()
        {
            {"注释",TokenType.COMMENT},
            {"如果",TokenType.IF},
            {"否则",TokenType.ELSE},
            {"否则如果",TokenType.ELIF},
            {"选择",TokenType.CASE},
            {"选项",TokenType.WHEN},
            {"等于",TokenType.EQ},
            {"不等于",TokenType.NE},
            {"小于",TokenType.LT},
            {"小等于",TokenType.LE},
            {"大于",TokenType.GT},
            {"大等于",TokenType.GE},
            {"加",TokenType.PLUS},
            {"减",TokenType.MINUS},
            {"乘",TokenType.MUL},
            {"除",TokenType.DIV},
            {"取整",TokenType.FLOORDIV},
            {"取余",TokenType.MOD},
            {"幂",TokenType.POW},
            {"非",TokenType.NOT},
            {"与",TokenType.AND},
            {"或",TokenType.OR},
            {"赋值",TokenType.ASSIGN},
            {"true",TokenType.TRUE},
            {"false",TokenType.FALSE},
            {"none",TokenType.NONE},
            {"真",TokenType.TRUE},
            {"开",TokenType.TRUE},
            {"假",TokenType.FALSE},
            {"关",TokenType.FALSE},
            {"空",TokenType.NONE},
        };

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
