﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GalgameNovelScript
{
    public class Interpreter : NodeVisitor
    {
        public CallStack CallStack { get; set; } = new CallStack();
        public ActivationRecord GlobalScope { get; set; } = new ActivationRecord("global", 1);
        public ActivationRecord CurrenScope => CallStack.Peek();
        public AST Tree { get; set; }
        public Interpreter(AST tree)
        {
            Tree = tree;
            CallStack.Push(GlobalScope);
        }
        public void AddToGlobalScope(string key, object value)
        {
            GlobalScope.AddMember(key, value);
        }
        public object GetFromGlobalScope(string key)
        {
            return GlobalScope.GetMember(key);
        }
        public object VisitNum(Num node)
        {
            return node.Value;
        }
        public object VisitStr(Str node)
        {
            return node.Value;
        }
        public object VisitBoolean(Boolean node)
        {
            return node.Value;
        }
        public object VisitNone(None node)
        {
            return node.Value;
        }
        public object VisitBinOp(BinOp node)
        {
            if (node.Op.Type == TokenType.PLUS)
                return (dynamic)Visit(node.Left) + (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.MINUS)
                return (dynamic)Visit(node.Left) - (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.MUL)
                return (dynamic)Visit(node.Left) * (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.DIV)
                return (float)(dynamic)Visit(node.Left) / (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.FLOORDIV)
                return (int)((dynamic)Visit(node.Left) / (dynamic)Visit(node.Right));
            else if (node.Op.Type == TokenType.MOD)
                return (dynamic)Visit(node.Left) % (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.POW)
                return Math.Pow(Convert.ToSingle(Visit(node.Left)), Convert.ToSingle(Visit(node.Right)));
            else if (node.Op.Type == TokenType.EQ)
                return Visit(node.Left).Equals(Visit(node.Right));
            else if (node.Op.Type == TokenType.NE)
                return !Visit(node.Left).Equals(Visit(node.Right));
            else if (node.Op.Type == TokenType.LT)
                return (dynamic)Visit(node.Left) < (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.LE)
                return (dynamic)Visit(node.Left) <= (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.GT)
                return (dynamic)Visit(node.Left) > (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.GE)
                return (dynamic)Visit(node.Left) >= (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.AND)
                return (dynamic)Visit(node.Left) && (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.OR)
                return (dynamic)Visit(node.Left) || (dynamic)Visit(node.Right);
            else if (node.Op.Type == TokenType.DOT)
            {
                var left = Visit(node.Left);
                var leftType = left.GetType();
                var right = (Var)node.Right;
                object value = null;
                if (left is IDictionary<string, object>)
                    value = ((IDictionary<string, object>)left).TryGetValue(right.Value, out value);
                if (value != null)
                    value = leftType.GetProperty(right.Value)?.GetValue(left);
                if (value == null)
                    value = leftType.GetField(right.Value)?.GetValue(left);
                if (value == null)
                    value = leftType.GetMethod(right.Value)?.Invoke(left, null);
                if (value == null)
                    throw new Exception("未定义的标识符");
                return value;
            }
            else if (node.Op.Type == TokenType.LBRACK)
            {
                var left = Visit(node.Left);
                var right = (Var)node.Right;
                object value = null;
                if (left is IDictionary<string, object>)
                    value = ((IDictionary<string, object>)left).TryGetValue(right.Value, out value);
                if (left is IList<object>)
                    value = ((IList<object>)left).ElementAtOrDefault<object>((int)Visit(right));
                if (value == null)
                    throw new Exception("未定义的标识符");
                return value;
            }
            else if (node.Op.Type == TokenType.ASSIGN)
            {
                if (node.Left is Var)
                {
                    var left = (Var)node.Left;
                    var value = Visit(node.Right);
                    CurrenScope.AddMember(left.Value, value);
                    return value;
                }
                else if (node.Left is BinOp)
                {
                    var left = (BinOp)node.Left;
                    var value = Visit(node.Right);
                    var leftLeft = Visit(left.Left);
                    var leftLeftType = leftLeft.GetType();
                    var leftRight = (Var)left.Right;
                    if (leftLeft is IDictionary<string, object>)
                        ((IDictionary<string, object>)leftLeft)[leftRight.Value] = value;
                    else if (leftLeftType.GetProperty(leftRight.Value) != null)
                        leftLeftType.GetProperty(leftRight.Value).SetValue(leftLeft, value);
                    else if (leftLeftType.GetField(leftRight.Value) != null)
                        leftLeftType.GetField(leftRight.Value).SetValue(leftLeft, value);
                    else if (leftLeftType.GetMethod(leftRight.Value) != null)
                        leftLeftType.GetMethod(leftRight.Value).Invoke(leftLeft, new object[] { value });
                    else
                        throw new Exception("未定义的标识符");
                    return value;
                }
                else
                    throw new Exception("未知的赋值操作符");
            }
            else
                throw new Exception("未知的二元操作符");
        }
        public object VisitUnaryOp(UnaryOp node)
        {
            if (node.Token.Type == TokenType.PLUS)
                return +(dynamic)Visit(node.Expr);
            else if (node.Token.Type == TokenType.MINUS)
                return -(dynamic)Visit(node.Expr);
            else if (node.Token.Type == TokenType.NOT)
                return !(dynamic)Visit(node.Expr);
            else
                throw new Exception("未知的一元操作符");
        }
        public object VisitVar(Var node)
        {
            var value = CurrenScope.GetMember(node.Value) ?? node.Value;
            return value;
        }
        public object VisitProgram(Program node)
        {
            foreach (var statement in node.Stmts)
            {
                Visit(statement);
            }
            return null;
        }
        public object VisitFunCall(FunCall node)
        {
            var fun = CurrenScope.GetMember(node.VarNode.Value) ??
                throw new Exception("未定义的标识符");
            if (fun is Delegate func)
            {
                var args = new List<object>();
                foreach (var arg in node.ActualParams.ArgsList)
                {
                    args.Add(Visit(arg));
                }
                return func.DynamicInvoke(args.ToArray());
            }
            else
                throw new Exception("未定义的标识符");

        }
        public object VisitIfStmt(IfStmt node)
        {
            foreach ((var condition, var thenStmt) in node.Stmts)
            {
                var toVisit = false;
                if (condition is null)
                    toVisit = true;
                else
                {
                    var result = Visit(condition);
                    if (result is bool v && v)
                        toVisit = true;
                }
                if (toVisit)
                {
                    var activeRecord = new ActivationRecord("if", CurrenScope.Level + 1, CurrenScope);
                    CallStack.Push(activeRecord);
                    Visit(thenStmt);
                    CallStack.Pop();
                    break;
                }
            }
            return null;
        }
        public object VisitCaseStmt(CaseStmt node)
        {
            throw new Exception("未实现");
        }
        public object VisitSuite(Suite node)
        {
            foreach (var statement in node.Stmts)
            {
                Visit(statement);
            }
            return null;
        }
        public object Interpret()
        {
            if (Tree == null)
                return null;
            return Visit(Tree);
        }
    }
}
