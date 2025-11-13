using System.Collections.Generic;

namespace GalgameNovelScript
{
    public class Lexer
    {
        public string Text { get; private set; }
        public int Pos { get; private set; }
        public char CurrentChar { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public Stack<int> Indent { get; private set; }
        public Queue<int> UneatIndent { get; private set; }
        public bool LineStart { get; private set; }
        public Lexer(string text)
        {
            Text = TextNormalize(text);
            Pos = 0;
            CurrentChar = Text[Pos];
            Line = 1;
            Column = 1;
            Indent = new Stack<int>();
            UneatIndent = new Queue<int>();
            Indent.Push(0);
            LineStart = true;
        }
        public void Error()
        {
            var message = string.Format("无法识别的字符{0}，位于{1}行{2}列。", CurrentChar, Line, Column);
            throw new LexerException(ErrorCode.UNEXPECTED_TOKEN, Line, Column, CurrentChar, message);
        }
        private string TextNormalize(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
        /// <summary>
        /// 朝前移动一个字符
        /// </summary>
        public void Advance()
        {
            Pos++;
            if (Pos >= Text.Length)
            {
                CurrentChar = '\0';
            }
            else
            {
                CurrentChar = Text[Pos];
                Column++;
            }
        }
        /// <summary>
        /// 返回当前字符的下一个字符
        /// </summary>
        /// <returns></returns>
        public char Peek()
        {
            var peekPos = Pos + 1;
            if (peekPos >= Text.Length)
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
            while (CurrentChar != '\0' && CurrentChar != '\n' && char.IsWhiteSpace(CurrentChar))
                Advance();
        }
        public void SkipComment()
        {
            while (CurrentChar != '\0' && CurrentChar != '\n')
                Advance();
            if (CurrentChar == '\n')
                NewLine();
        }
        public Token NewLine()
        {
            if (CurrentChar == '\n')
            {
                Advance();
            }
            Line++;
            Column = 1;
            LineStart = true;
            return new Token(TokenType.NEWLINE, null, Line, Column);
        }
        public bool HasDent()
        {
            var spaces = 0;
            while (CurrentChar == ' ' || CurrentChar == '\t')
            {
                if (CurrentChar == '\t')
                {
                    spaces += 4;
                }
                else
                {
                    spaces++;
                }
                Advance();
            }

            // 空行
            if (CurrentChar == '\n')
            {
                NewLine();
                return false;
            }
            if (CurrentChar == '#' || CurrentChar == '；' || CurrentChar == ';')
            {
                SkipComment();
                return false;
            }

            // 不是空行，处理缩进
            LineStart = false;
            while (spaces < Indent.Peek())
            {
                var topIndent = Indent.Pop();
                UneatIndent.Enqueue(-topIndent);
            }
            if (spaces > Indent.Peek())
            {
                UneatIndent.Enqueue(spaces);
            }
            return true;
        }
        public Token? CheckUneatIndent()
        {
            Token? token = null;
            if (UneatIndent.Count > 0)
            {
                var indent = UneatIndent.Dequeue();
                if (indent > 0)
                {
                    Indent.Push(indent);
                    token = new Token(TokenType.INDENT, indent, Line, Column);
                }
                else
                {
                    token = new Token(TokenType.DEDENT, -indent, Line, Column);
                }
            }
            return token;
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
        public Token IDOrStr()
        {
            var result = new List<char>();
            // 第一次遍历时，CurrentChar是字母，外部已经判断过了
            while (CurrentChar != '\n' && CurrentChar != '\0' && !char.IsWhiteSpace(CurrentChar) && Token.ChineseSymbol.IndexOf(CurrentChar) == -1)
            {
                // 支持转义字符
                if (CurrentChar == '\\')
                    Advance();
                result.Add(CurrentChar);
                Advance();
            }
            var str = new string(result.ToArray());
            var token = Token.GetReservedKeywords(str, Line, Column);
            token ??= new Token(TokenType.NAME, str, Line, Column);
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
                endChar = '”';
            if (endChar == '‘')
                endChar = '’';
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
                if (LineStart && !HasDent())
                    continue;

                var uneatIndentToken = CheckUneatIndent();
                if (uneatIndentToken != null)
                    return uneatIndentToken;
                if (CurrentChar != '\n' && char.IsWhiteSpace(CurrentChar))
                {
                    SkipWhitespace();
                    continue;
                }
                if (CurrentChar == '\n')
                {
                    return NewLine();
                }
                if (char.IsDigit(CurrentChar))
                    return Number();
                if (CurrentChar == '“' || CurrentChar == '‘' || CurrentChar == '"'
                    || CurrentChar == '\'')
                {
                    return Str();
                }

                if (CurrentChar == ':' || CurrentChar == '：')
                {
                    Advance();
                    return new Token(TokenType.COLON, ':', Line, Column);
                }
                if (CurrentChar == '=')
                {
                    Advance();
                    if (CurrentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.EQ, "==", Line, Column);
                    }
                    return new Token(TokenType.ASSIGN, '=', Line, Column);
                }
                if (CurrentChar == '!' || CurrentChar == '！')
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
                if (CurrentChar == '.' || CurrentChar == '。' || CurrentChar == '的')
                {
                    Advance();
                    return new Token(TokenType.DOT, ".", Line, Column);
                }
                if (CurrentChar == '\0')
                {
                    return new Token(TokenType.EOF, null, Line, Column);
                }
                if (IsLetter())
                    return IDOrStr();
                Error();
            }
        }
        private bool IsLetter()
        {
            return char.IsLetter(CurrentChar) || CurrentChar == '_' || CurrentChar == '\\' ||
                (CurrentChar > '\u007F' && Token.ChineseSymbol.IndexOf(CurrentChar) == -1);
        }
    }
}
