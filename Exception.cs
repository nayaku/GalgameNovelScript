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
            switch (errorCode)
            {
                case ErrorCode.UNEXPECTED_TOKEN:
                    return "无法识别的字符";
                case ErrorCode.ID_NOT_FOUND:
                    return "未定义的标识符";
                case ErrorCode.DUPLICATE_ID:
                    return "重复定义的标识符";
                default:
                    return "未知错误";
            }
        }
    }
    public class BaseException : Exception
    {
        public ErrorCode ErrorCode { get; set; }
        public Token Token { get; set; }
        public string Message { get; set; }
        public BaseException(ErrorCode errorCode, Token token, string message)
        {
            ErrorCode = errorCode;
            Token = token;
            Message = message;
        }
    }
    public class LexerException : BaseException
    {
        public LexerException(ErrorCode errorCode, Token token, string message)
            : base(errorCode, token, message)
        {
        }
    }
    public class ParserException : BaseException
    {
        public ParserException(ErrorCode errorCode, Token token, string message)
            : base(errorCode, token, message)
        {
        }
    }
    public class SemanticException : BaseException
    {
        public SemanticException(ErrorCode errorCode, Token token, string message)
            : base(errorCode, token, message)
        {
        }
    }

}
