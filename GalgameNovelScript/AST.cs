using System;
using System.Collections.Generic;

namespace GalgameNovelScript
{
    public abstract class AST
    {
    }
    public class Num : AST
    {
        public double Value { get; }
        public Num(Token token)
        {
            Value = Convert.ToDouble(token.Value);
        }
    }
    public class Str : AST
    {
        public string Value { get; }
        public Str(Token token)
        {
            Value = (string)token.Value!;
        }
    }
    public class Boolean : AST
    {
        public bool Value { get; }
        public Boolean(Token token)
        {
            Value = token.Type == TokenType.TRUE;
        }
    }
    public class None : AST
    {
    }
    public class BinOp : AST
    {
        public AST Left { get; }
        public Token Op { get; }
        public AST Right { get; }
        public BinOp(AST left, Token op, AST right)
        {
            Left = left;
            Op = op;
            Right = right;
        }
    }
    public class UnaryOp : AST
    {
        public Token Token { get; }
        public AST Expr { get; }
        public UnaryOp(Token token, AST expr)
        {
            Token = token;
            Expr = expr;
        }
    }
    public class Var : AST
    {
        public Token Token { get; }
        public string Value { get; }
        public Var(Token token)
        {
            Token = token;
            Value = (string)token.Value!;
        }
    }
    public class Program : AST
    {
        public List<AST> Stmts { get; }
        public Program()
        {
            Stmts = new List<AST>();
        }
    }
    public class FunCall : AST
    {
        public Var? VarNode { get; }
        public List<AST> Parms { get; }
        public FunCall(Var? varNode, List<AST> parms)
        {
            VarNode = varNode;
            Parms = parms;
        }
    }
    public class IfStmt : AST
    {
        public List<(AST? Condition, AST ThenStmt)> Stmts { get; }
        public IfStmt(List<(AST? condition, AST thenStmt)> stmts)
        {
            Stmts = stmts;
        }
    }
    public class Case : AST
    {
        public AST? CaseTip { get; }
        public List<When> Whens { get; }
        public Case(AST? caseTip, List<When> whens)
        {
            CaseTip = caseTip;
            Whens = whens;
        }
    }
    public class When : AST
    {
        public AST Option { get; }
        public AST Then { get; }
        public When(AST option, AST then)
        {
            Option = option;
            Then = then;
        }
    }
    public class Suite : AST
    {
        public List<AST> Stmts { get; }
        public Suite(List<AST> stmts)
        {
            Stmts = stmts;
        }
    }
    public class Args : AST
    {
        public List<AST> ArgsList { get; }
        public Args(List<AST> argsList)
        {
            ArgsList = argsList;
        }
    }
    public class Loop : AST
    {
        public AST? Condition { get; }
        public AST Body { get; }
        public Loop(AST? condition, AST body)
        {
            Condition = condition;
            Body = body;
        }
    }
    public class Break : AST
    {
    }
    public class Continue : AST
    {
    }
    public class Return : AST
    {
        public AST? Expr { get; }
        public Return(AST? expr)
        {
            Expr = expr;
        }
    }
}
