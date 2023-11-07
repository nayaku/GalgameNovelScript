using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalgameNovelScript
{
    public abstract class AST
    {
    }
    public class Num : AST
    {
        public Token Token { get; set; }
        public object Value { get; set; }
        public Num(Token token)
        {
            Token = token;
            Value = token.Value;
        }
    }
    public class Str : AST
    {
        public Token Token { get; set; }
        public object Value { get; set; }
        public Str(Token token)
        {
            Token = token;
            Value = token.Value;
        }
    }
    public class Boolean : AST
    {
        public Token Token { get; set; }
        public object Value { get; set; }
        public Boolean(Token token)
        {
            Token = token;
            Value = token.Value;
        }
    }
    public class None : AST
    {
        public Token Token { get; set; }
        public object Value { get; set; }
        public None(Token token)
        {
            Token = token;
            Value = token.Value;
        }
    }
    public class BinOp : AST
    {
        public AST Left { get; set; }
        public Token Op { get; set; }
        public AST Right { get; set; }
        public BinOp(AST left, Token op, AST right)
        {
            Left = left;
            Op = op;
            Right = right;
        }
    }
    public class UnaryOp : AST
    {
        public Token Token { get; set; }
        public AST Expr { get; set; }
        public UnaryOp(Token token, AST expr)
        {
            Token = token;
            Expr = expr;
        }
    }
    public class Compound : AST
    {
        public List<AST> Children { get; set; }
        public Compound()
        {
            Children = new List<AST>();
        }
    }
    public class Assign : AST
    {
        public Var Left { get; set; }
        public Token Op { get; set; }
        public AST Right { get; set; }
        public Assign(Var left, Token op, AST right)
        {
            Left = left;
            Op = op;
            Right = right;
        }
    }
    public class Var : AST
    {
        public Token Token { get; set; }
        public object Value { get; set; }
        public Var(Token token)
        {
            Token = token;
            Value = token.Value;
        }
    }
    public class NoOp : AST
    {
    }
    public class Program : AST
    {
        public List<AST> Stmts { get; set; }
        public Program()
        {
            Stmts = new List<AST>();
        }
    }
    public class Fun : AST
    {
        public Var VarNode { get; set; }
        public Args ActualParams { get; set; }
        public Fun(Var varNode, Args actualParams)
        {
            VarNode = varNode;
            ActualParams = actualParams;
        }
    }
    public class IfStmt : AST
    {
        public AST Condition { get; set; }
        public AST ThenStmt { get; set; }
        public AST ElseStmt { get; set; }
        public IfStmt(AST condition, AST thenStmt, AST elseStmt)
        {
            Condition = condition;
            ThenStmt = thenStmt;
            ElseStmt = elseStmt;
        }
    }
    public class CaseStmt : AST
    {
        public AST Condition { get; set; }
        public List<WhenStmt> WhenStmts { get; set; }
        public CaseStmt(AST condition, List<WhenStmt> whenStmts)
        {
            Condition = condition;
            WhenStmts = whenStmts;
        }
    }
    public class WhenStmt : AST
    {
        public AST Condition { get; set; }
        public AST ThenStmt { get; set; }
        public WhenStmt(AST condition, AST thenStmt)
        {
            Condition = condition;
            ThenStmt = thenStmt;
        }
    }
    public class Suite : AST
    {
        public List<AST> Stmts { get; set; }
        public Suite(List<AST> stmts)
        {
            Stmts = stmts;
        }
    }
    public class ProcedureCall : AST
    {
        public Var ProcedureName { get; set; }
        public List<AST> ActualParams { get; set; }
        public Token Token { get; set; }
        public object ProcedureSymbol { get; set; }
        public ProcedureCall(Var procedureName, List<AST> actualParams, Token token)
        {
            ProcedureName = procedureName;
            ActualParams = actualParams;
            Token = token;
        }
    }
    public class Args : AST
    {
        public List<AST> ArgsList { get; set; }
        public Args(List<AST> argsList)
        {
            ArgsList = argsList;
        }
    }

}
