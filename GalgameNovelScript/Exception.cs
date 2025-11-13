using System;

namespace GalgameNovelScript
{
    public enum ErrorCode
    {
        UNEXPECTED_TOKEN,
        ID_NOT_FOUND,
        DUPLICATE_ID,
    }
    public class ErrorCodeHandle
    {
        public static string GetError(ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.UNEXPECTED_TOKEN => "无法识别的字符",
                ErrorCode.ID_NOT_FOUND => "未定义的标识符",
                ErrorCode.DUPLICATE_ID => "重复定义的标识符",
                _ => "未知错误",
            };
        }
    }
    public class BaseException : Exception
    {
        public ErrorCode ErrorCode { get; }
        public BaseException(ErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
    public class LexerException : BaseException
    {
        public int Line { get; }
        public int Column { get; }
        public char UnexpectedChar { get; }
        public LexerException(ErrorCode errorCode, int line, int column, char unexpectedChar, string message)
            : base(errorCode, message)
        {
            Line = line;
            Column = column;
            UnexpectedChar = unexpectedChar;
        }
    }
    public class ParserException : BaseException
    {
        public Token CurrentToken { get; }
        public TokenType[] ExpectTokenType { get; }
        public ParserException(ErrorCode errorCode, Token currentToken, TokenType[] expectTokenType, string message)
            : base(errorCode, message)
        {
            CurrentToken = currentToken;
            ExpectTokenType = expectTokenType;
        }
    }
    public class SemanticException : BaseException
    {
        public SemanticException(ErrorCode errorCode, Token token, string message)
            : base(errorCode, message)
        {
        }
    }

}
