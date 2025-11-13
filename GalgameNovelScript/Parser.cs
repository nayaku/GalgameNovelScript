using System.Collections.Generic;

namespace GalgameNovelScript
{
    public class Parser
    {
        public Lexer Lexer { get; set; }
        public Token CurrentToken { get; set; }
        public Parser(Lexer lexer)
        {
            Lexer = lexer;
            CurrentToken = Lexer.GetNextToken();
        }
        public void Error(ErrorCode errorCode, TokenType[] expectTokenType)
        {
            var expectTypes = string.Join(", ", expectTokenType);
            var message = string.Format("错误：{0}: 期望的TokenType是 {1}，但得到的是 {2}", ErrorCodeHandle.GetError(errorCode), expectTypes, CurrentToken);
            throw new ParserException(errorCode, CurrentToken, expectTokenType, message);
        }
        /// <summary>
        /// 将当前令牌类型与传递的令牌进行比较输入，
        /// 如果匹配则“吃掉”当前令牌并将下一个令牌分配给CurrentToken,
        /// 否则引发异常。
        /// </summary>
        /// <param name="tokenType"></param>
        public void Eat(TokenType tokenType)
        {
            if (CurrentToken.Type == tokenType)
                CurrentToken = Lexer.GetNextToken();
            else
                Error(ErrorCode.UNEXPECTED_TOKEN, new TokenType[] { tokenType });
        }
        public AST Program()
        {
            // program: (NEWLINE | stmt)*
            var node = new Program();
            while (true)
            {
                if (CurrentToken.Type == TokenType.NEWLINE)
                {
                    Eat(TokenType.NEWLINE);
                }
                else if (CurrentToken.Type == TokenType.EOF)
                {
                    break;
                }
                else
                {
                    var stmt = Stmt();
                    if (stmt != null)
                        node.Stmts.Add(stmt);
                }
            }
            return node;
        }
        public AST Stmt()
        {
            // stmt: simple_stmt | compound_stmt
            if (CurrentToken.Type == TokenType.IF
                || CurrentToken.Type == TokenType.CASE
                || CurrentToken.Type == TokenType.LOOP)
            {
                return CompoundStmt();
            }
            else
            {
                return SimpleStmt();
            }
        }
        public AST SimpleStmt()
        {
            // simple_stmt: small_stmt (NEWLINE | EOF)
            var node = SmallStmt();
            if (CurrentToken.Type == TokenType.NEWLINE)
                Eat(TokenType.NEWLINE);
            return node;
        }
        public AST SmallStmt()
        {
            // small_stmt: (fun | assign_stmt | break_stmt | continue_stmt | return_stmt)?
            if (CurrentToken.Type == TokenType.COLON)
                return Fun(null);
            else if (CurrentToken.Type == TokenType.BREAK)
                return BreakStmt();
            else if (CurrentToken.Type == TokenType.CONTINUE)
                return ContinueStmt();
            else if (CurrentToken.Type == TokenType.RETURN)
                return ReturnStmt();

            var var = new Var(CurrentToken);
            Eat(TokenType.NAME);
            if (CurrentToken.Type == TokenType.COLON)
                return Fun(var);
            else
                return AssginStmt(var);
        }
        public AST Fun(Var? var)
        {
            // fun: (NAME)? COLON (expr)?
            // 上一步已经吃掉了NAME
            Eat(TokenType.COLON);
            var parms = new List<AST>();
            while (CurrentToken.Type != TokenType.NEWLINE
                && CurrentToken.Type != TokenType.EOF)
            {
                var value = Expr();
                parms.Add(value);
            }
            return new FunCall(var, parms);
        }
        public AST AssginStmt(Var var)
        {
            // assign_stmt: NAME trailer* = expr
            // 上一步已经吃掉了NAME
            AST node = var;
            Token op;
            while (CurrentToken.Type == TokenType.LBRACK || CurrentToken.Type == TokenType.DOT)
            {
                op = CurrentToken;
                var trailer = Trailer();
                node = new BinOp(node, op, trailer);
            }
            op = CurrentToken;
            Eat(TokenType.ASSIGN);

            var right = Expr();
            return new BinOp(node, op, right);
        }
        public AST BreakStmt()
        {
            // break_stmt: BREAK
            var node = new Break();
            Eat(TokenType.BREAK);
            return node;
        }
        public AST ContinueStmt()
        {
            // continue_stmt: CONTINUE
            var node = new Continue();
            Eat(TokenType.CONTINUE);
            return node;
        }
        public AST ReturnStmt()
        {
            // return_stmt: RETURN (expr)*
            Eat(TokenType.RETURN);
            AST? expr = null;
            if (CurrentToken.Type != TokenType.NEWLINE)
                expr = Expr();
            return new Return(expr);
        }
        public AST CompoundStmt()
        {
            // compound_stmt: if_stmt | case_stmt | loop_stmt
            if (CurrentToken.Type == TokenType.IF)
                return IfStmt();
            else if (CurrentToken.Type == TokenType.LOOP)
                return LoopStmt();
            else
                return CaseStmt();
        }
        public AST IfStmt()
        {
            // if_stmt: IF expr suite (ELIF expr suite)* (ELSE suite)?
            Eat(TokenType.IF);
            var ifStmt = new List<(AST? Condition, AST ThenStmt)>();
            var expr = Expr();
            var thenSuite = Suite();
            ifStmt.Add((expr, thenSuite));
            if (CurrentToken.Type == TokenType.ELIF)
            {
                while (CurrentToken.Type == TokenType.ELIF)
                {
                    Eat(TokenType.ELIF);
                    var elifExpr = Expr();
                    var elifSuite = Suite();
                    ifStmt.Add((elifExpr, elifSuite));
                }
            }
            if (CurrentToken.Type == TokenType.ELSE)
            {
                Eat(TokenType.ELSE);
                var elseSuite = Suite();
                ifStmt.Add((null, elseSuite));
            }

            return new IfStmt(ifStmt);
        }
        public AST CaseStmt()
        {
            // case_stmt: CASE value? NEWLINE INDENT (when_stmt)+ DEDENT
            Eat(TokenType.CASE);
            Var? caseTip = null;
            if (CurrentToken.Type != TokenType.NEWLINE)
            {
                caseTip = new Var(CurrentToken);
                Eat(TokenType.NAME);
            }
            Eat(TokenType.NEWLINE);
            Eat(TokenType.INDENT);
            var whens = new List<When>();
            while (CurrentToken.Type == TokenType.WHEN)
            {
                var whenStmt = WhenStmt();
                whens.Add((When)whenStmt);
            }
            Eat(TokenType.DEDENT);
            var node = new Case(caseTip, whens);
            return node;
        }
        public AST WhenStmt()
        {
            // when_stmt: WHEN value suit
            Eat(TokenType.WHEN);
            var value = Value();
            var suite = Suite();
            return new When(value, suite);
        }
        public AST LoopStmt()
        {
            // loop_stmt: LOOP (expr)? suit
            Eat(TokenType.LOOP);
            AST? expr = null;
            if (CurrentToken.Type != TokenType.NEWLINE)
                expr = Expr();
            var suite = Suite();
            return new Loop(expr, suite);
        }
        public AST Suite()
        {
            // suite: NEWLINE INDENT stmt* DEDENT
            Eat(TokenType.NEWLINE);
            Eat(TokenType.INDENT);
            var stmts = new List<AST>();
            while (CurrentToken.Type != TokenType.DEDENT)
            {
                var stmt = Stmt();
                if (stmt != null)
                    stmts.Add(stmt);
            }
            Eat(TokenType.DEDENT);
            return new Suite(stmts);
        }
        public AST Expr()
        {
            // expr: or_test
            return OrTest();
        }
        public AST OrTest()
        {
            // or_test: and_test (OR and_test)*
            var node = AndTest();
            while (CurrentToken.Type == TokenType.OR)
            {
                var op = CurrentToken;
                Eat(TokenType.OR);
                var right = AndTest();
                node = new BinOp(node, op, right);
            }
            return node;

        }
        public AST AndTest()
        {
            // and_test: not_test (AND not_test)*
            var node = NotTest();
            while (CurrentToken.Type == TokenType.AND)
            {
                var op = CurrentToken;
                Eat(TokenType.AND);
                var right = NotTest();
                node = new BinOp(node, op, right);
            }
            return node;
        }
        public AST NotTest()
        {
            // not_test: NOT not_test | comparison
            if (CurrentToken.Type == TokenType.NOT)
            {
                var op = CurrentToken;
                Eat(TokenType.NOT);
                var right = NotTest();
                return new UnaryOp(op, right);
            }
            else
            {
                return Comparsion();
            }
        }
        public AST Comparsion()
        {
            // comparsion: calculator (comp_op calculator)*
            var node = Calculator();
            while (CurrentToken.Type == TokenType.EQ || CurrentToken.Type == TokenType.NE
                || CurrentToken.Type == TokenType.LT || CurrentToken.Type == TokenType.LE
                || CurrentToken.Type == TokenType.GT || CurrentToken.Type == TokenType.GE)
            {
                var op = CurrentToken;
                CompOp();
                var right = Calculator();
                node = new BinOp(node, op, right);
            }
            return node;
        }
        public void CompOp()
        {
            // comp_op: < | > | == | >= | <= | !=
            if (CurrentToken.Type == TokenType.EQ)
                Eat(TokenType.EQ);
            else if (CurrentToken.Type == TokenType.NE)
                Eat(TokenType.NE);
            else if (CurrentToken.Type == TokenType.LT)
                Eat(TokenType.LT);
            else if (CurrentToken.Type == TokenType.LE)
                Eat(TokenType.LE);
            else if (CurrentToken.Type == TokenType.GT)
                Eat(TokenType.GT);
            else if (CurrentToken.Type == TokenType.GE)
                Eat(TokenType.GE);
        }
        public AST Calculator()
        {
            // calculator: term (CalculatorOp term)*
            var node = Term();
            while (CurrentToken.Type == TokenType.PLUS || CurrentToken.Type == TokenType.MINUS)
            {
                var op = CurrentToken;
                CalculatorOp();
                var right = Term();
                node = new BinOp(node, op, right);
            }
            return node;
        }
        public void CalculatorOp()
        {
            // calculator_op: + | -
            if (CurrentToken.Type == TokenType.PLUS)
                Eat(TokenType.PLUS);
            else if (CurrentToken.Type == TokenType.MINUS)
                Eat(TokenType.MINUS);
        }
        public AST Term()
        {
            // term: factor (( * | / | // | %) factor)*
            var node = Factor();
            while (CurrentToken.Type == TokenType.MUL || CurrentToken.Type == TokenType.DIV
                               || CurrentToken.Type == TokenType.FLOORDIV || CurrentToken.Type == TokenType.MOD)
            {
                var op = CurrentToken;
                TermOp();
                var right = Factor();
                node = new BinOp(node, op, right);
            }
            return node;
        }
        public void TermOp()
        {
            // term_op: * | / | // | %
            if (CurrentToken.Type == TokenType.MUL)
                Eat(TokenType.MUL);
            else if (CurrentToken.Type == TokenType.DIV)
                Eat(TokenType.DIV);
            else if (CurrentToken.Type == TokenType.FLOORDIV)
                Eat(TokenType.FLOORDIV);
            else if (CurrentToken.Type == TokenType.MOD)
                Eat(TokenType.MOD);
        }
        public AST Factor()
        {
            // factor: ( +|- ) factor | power
            if (CurrentToken.Type == TokenType.PLUS || CurrentToken.Type == TokenType.MINUS)
            {
                var op = CurrentToken;
                CalculatorOp();
                var right = Factor();
                return new UnaryOp(op, right);
            }
            else
            {
                return Power();
            }
        }
        public AST Power()
        {
            // power: atom trailer* (POW factor)*
            var node = Atom();
            if (CurrentToken.Type == TokenType.LBRACK || CurrentToken.Type == TokenType.DOT)
            {
                var op = CurrentToken;
                var trailer = Trailer();
                node = new BinOp(node, op, trailer);
            }
            while (CurrentToken.Type == TokenType.POW)
            {
                var op = CurrentToken;
                Eat(TokenType.POW);
                var right = Factor();
                node = new BinOp(node, op, right);
            }
            return node;
        }
        public AST Atom()
        {
            // atom: LPAREN expr RPAREN | fun | value
            if (CurrentToken.Type == TokenType.LPAREN)
            {
                Eat(TokenType.LPAREN);
                var node = Expr();
                Eat(TokenType.RPAREN);
                return node;
            }
            else if (CurrentToken.Type == TokenType.NAME)
            {
                var node = new Var(CurrentToken);
                Eat(TokenType.NAME);
                if (CurrentToken.Type == TokenType.COLON)
                    return Fun(node);
                return node;
            }
            else
            {
                return Value();
            }
        }
        public AST? Value()
        {
            // value: NAME | INT_CONST | REAL_CONST | STR | TRUE | FALSE | NONE
            if (CurrentToken.Type == TokenType.NAME)
            {
                var node = new Var(CurrentToken);
                Eat(TokenType.NAME);
                return node;
            }
            else if (CurrentToken.Type == TokenType.INT_CONST)
            {
                var node = new Num(CurrentToken);
                Eat(TokenType.INT_CONST);
                return node;
            }
            else if (CurrentToken.Type == TokenType.REAL_CONST)
            {
                var node = new Num(CurrentToken);
                Eat(TokenType.REAL_CONST);
                return node;
            }
            else if (CurrentToken.Type == TokenType.STR)
            {
                var node = new Str(CurrentToken);
                Eat(TokenType.STR);
                return node;
            }
            else if (CurrentToken.Type == TokenType.TRUE)
            {
                var node = new Boolean(CurrentToken);
                Eat(TokenType.TRUE);
                return node;
            }
            else if (CurrentToken.Type == TokenType.FALSE)
            {
                var node = new Boolean(CurrentToken);
                Eat(TokenType.FALSE);
                return node;
            }
            else if (CurrentToken.Type == TokenType.NONE)
            {
                var node = new None();
                Eat(TokenType.NONE);
                return node;
            }
            else
            {
                Error(ErrorCode.UNEXPECTED_TOKEN, new TokenType[] {
                    TokenType.NAME,
                    TokenType.INT_CONST,
                    TokenType.REAL_CONST,
                    TokenType.STR,
                    TokenType.TRUE,
                    TokenType.FALSE,
                    TokenType.NONE
                });
                return null;
            }
        }
        public AST? Trailer()
        {
            // trailer: LBRACK args RBRACK | DOT NAME
            if (CurrentToken.Type == TokenType.LBRACK)
            {
                Eat(TokenType.LBRACK);
                var node = Args();
                Eat(TokenType.RBRACK);
                return node;
            }
            else if (CurrentToken.Type == TokenType.DOT)
            {
                Eat(TokenType.DOT);
                var node = new Var(CurrentToken);
                Eat(TokenType.NAME);
                return node;
            }
            else
            {
                Error(ErrorCode.UNEXPECTED_TOKEN, new TokenType[] {
                    TokenType.LBRACK, TokenType.DOT });
                return null;
            }
        }
        public Args Args()
        {
            // arglist: (expr)*
            var args = new List<AST>();
            while (CurrentToken.Type != TokenType.RBRACK
                && CurrentToken.Type != TokenType.NEWLINE
                && CurrentToken.Type != TokenType.EOF)
            {
                var expr = Expr();
                args.Add(expr);
            }
            return new Args(args);
        }
        public AST Parse()
        {
            var node = Program();
            if (CurrentToken.Type != TokenType.EOF)
                Error(ErrorCode.UNEXPECTED_TOKEN, new TokenType[] { TokenType.EOF });
            return node;
        }
    }
}
