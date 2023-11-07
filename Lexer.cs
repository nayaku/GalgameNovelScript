﻿using System;
using System.Collections.Generic;

namespace GalgameNovelScript
{
    public class Lexer
    {
        public string Text { get; set; }
        public int Pos { get; set; }
        public char CurrentChar { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public Stack<int> Indent { get; set; }
        public bool LineStart { get; set; }
        public Lexer(string text)
        {
            Text = text;
            Pos = 0;
            CurrentChar = Text[Pos];
            Line = 1;
            Column = 1;
            Indent = new Stack<int>();
            Indent.Push(0);
            LineStart = true;
        }
        public void Error()
        {
            var message = string.Format("无法识别的字符{0}，位于{1}行{2}列。", CurrentChar, Line, Column);
            throw new Exception(message);
        }
        public void Advance()
        {
            Pos++;
            if (Pos > Text.Length - 1)
            {
                CurrentChar = '\0';
            }
            else
            {
                CurrentChar = Text[Pos];
                Column++;
            }
        }
        public char Peek()
        {
            var peekPos = Pos + 1;
            if (peekPos > Text.Length - 1)
            {
                return '\0';
            }
            else
            {
                return Text[peekPos];
            }
        }
        public void SkipWhitespace()
        {
            while (CurrentChar == ' ' || CurrentChar == '\t')
            {
                Advance();
            }
        }
        public void SkipComment()
        {
            while (CurrentChar != '\0' && CurrentChar != '\u000C' ||
                CurrentChar != '\n' || CurrentChar != '\r')
            {
                Advance();
            }
        }
        public Token NewLine()
        {
            if (CurrentChar == '\f')
                Advance();
            else if (CurrentChar == '\r')
            {
                Advance();
                if (CurrentChar == '\n')
                    Advance();
            }
            else if (CurrentChar == '\n')
            {
                Advance();
            }
            Line++;
            Column = 1;
            LineStart = true;
            return new Token(TokenType.NEWLINE, null, Line, Column);
        }
        public Token Dent()
        {
            var spaces = 0;
            while (CurrentChar == ' ' || CurrentChar == '\t')
            {
                if (CurrentChar == '\t')
                {
                    spaces += 8;
                }
                else
                {
                    spaces++;
                }
                Advance();
            }
            // 判断是否是空行
            if (CurrentChar == '\f' || CurrentChar == '\r' || CurrentChar == '\n')
                return null;
            if (spaces > Indent.Peek())
            {
                Indent.Push(spaces);
                return new Token(TokenType.INDENT, spaces, Line, Column);
            }
            else if (spaces < Indent.Peek() && Column == 1)
            {
                Indent.Pop();
                return new Token(TokenType.DEDENT, spaces, Line, Column);
            }
            return null;
        }
        /// <summary>
        /// 返回一个数值
        /// </summary>
        /// <returns></returns>
        public Token Number()
        {
            var result = new List<char>();
            Token token;
            while (char.IsDigit(CurrentChar))
            {
                result.Add(CurrentChar);
                Advance();
            }
            if (CurrentChar == '.')
            {
                result.Add(CurrentChar);
                Advance();

                while (char.IsDigit(CurrentChar))
                {
                    result.Add(CurrentChar);
                    Advance();
                }
                var str = new string(result.ToArray());
                token = new Token(TokenType.REAL_CONST, float.Parse(str), Line, Column);
            }
            else
            {
                var str = new string(result.ToArray());
                token = new Token(TokenType.INT_CONST, int.Parse(str), Line, Column);
            }
            return token;
        }
        /// <summary>
        /// 返回一个保留字
        /// </summary>
        /// <returns></returns>
        public Token ID()
        {
            var result = new List<char>();
            if (char.IsLetter(CurrentChar) || CurrentChar == '_')
            {
                result.Add(CurrentChar);
                Advance();
                while (char.IsLetter(CurrentChar) || CurrentChar == '_' || char.IsDigit(CurrentChar))
                {
                    result.Add(CurrentChar);
                    Advance();
                }
            }
            var str = new string(result.ToArray());
            var token = Token.GetReservedKeywords(str, Line, Column);
            if (token == null)
            {
                token = new Token(TokenType.NAME, str, Line, Column);
            }
            return token;
        }
        /// <summary>
        /// 返回一个字符串
        /// </summary>
        /// <returns></returns>
        public Token Str()
        {
            var result = new List<char>();
            var endChar = CurrentChar;
            if (endChar == '“')
            {
                endChar = '”';
            }
            Advance();
            while (CurrentChar != endChar)
            {
                result.Add(CurrentChar);
                Advance();
            }
            Advance();
            var str = new string(result.ToArray());
            return new Token(TokenType.STR, str, Line, Column);
        }
        /// <summary>
        /// 词法分析
        /// </summary>
        public Token GetNextToken()
        {
            while (true)
            {
                if (LineStart)
                {
                    var token = Dent();
                    if (token != null)
                        return token;
                    LineStart = false;
                }
                if (CurrentChar == ' ' || CurrentChar == '\t')
                {
                    SkipWhitespace();
                    continue;
                }
                if (CurrentChar == '\f' || CurrentChar == '\r' || CurrentChar == '\n')
                {
                    return NewLine();
                }
                if (CurrentChar == '#' || (CurrentChar == '注' && Peek() == '释'))
                {
                    SkipComment();
                    continue;
                }
                if (char.IsLetter(CurrentChar))
                    return ID();
                if (char.IsDigit(CurrentChar))
                    return Number();
                if (CurrentChar == '“' || CurrentChar == '”' || CurrentChar == '"' || CurrentChar == '\'')
                    return Str();
                if (CurrentChar == ':' || CurrentChar == '：')
                {
                    Advance();
                    return new Token(TokenType.COLON, ":", Line, Column);
                }
                if (CurrentChar == '=')
                {
                    Advance();
                    if (CurrentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.EQ, "==", Line, Column);
                    }
                    return new Token(TokenType.ASSIGN, "=", Line, Column);
                }
                if (CurrentChar == '!')
                {
                    Advance();
                    if (CurrentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.NE, "!=", Line, Column);
                    }
                    return new Token(TokenType.NOT, "!", Line, Column);
                }
                if (CurrentChar == '<')
                {
                    Advance();
                    if (CurrentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.LE, "<=", Line, Column);
                    }
                    return new Token(TokenType.LT, "<", Line, Column);
                }
                if (CurrentChar == '>')
                {
                    Advance();
                    if (CurrentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.GE, ">=", Line, Column);
                    }
                    return new Token(TokenType.GT, ">", Line, Column);
                }
                if (CurrentChar == '+')
                {
                    Advance();
                    return new Token(TokenType.PLUS, "+", Line, Column);
                }
                if (CurrentChar == '-')
                {
                    Advance();
                    return new Token(TokenType.MINUS, "-", Line, Column);
                }
                if (CurrentChar == '*')
                {
                    Advance();
                    if (CurrentChar == '*')
                    {
                        Advance();
                        return new Token(TokenType.POW, "**", Line, Column);
                    }
                    return new Token(TokenType.MUL, "*", Line, Column);
                }
                if (CurrentChar == '/')
                {
                    Advance();
                    if (CurrentChar == '/')
                    {
                        Advance();
                        return new Token(TokenType.FLOORDIV, "//", Line, Column);
                    }
                    return new Token(TokenType.DIV, "/", Line, Column);
                }
                if (CurrentChar == '%')
                {
                    Advance();
                    return new Token(TokenType.MOD, "%", Line, Column);
                }
                if (CurrentChar == '(' || CurrentChar == '（')
                {
                    Advance();
                    return new Token(TokenType.LPAREN, "(", Line, Column);
                }
                if (CurrentChar == ')' || CurrentChar == '）')
                {
                    Advance();
                    return new Token(TokenType.RPAREN, ")", Line, Column);
                }
                if (CurrentChar == '[' || CurrentChar == '【')
                {
                    Advance();
                    return new Token(TokenType.LBRACK, "[", Line, Column);
                }
                if (CurrentChar == ']' || CurrentChar == '】')
                {
                    Advance();
                    return new Token(TokenType.RBRACK, "]", Line, Column);
                }
                if (CurrentChar == '.')
                {
                    Advance();
                    return new Token(TokenType.DOT, ".", Line, Column);
                }
                if (CurrentChar == '\0')
                {
                    return new Token(TokenType.EOF, null, Line, Column);
                }
                Error();
            }
        }
    }
}
