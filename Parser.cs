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
        public void Error(ErrorCode errorCode, Token token)
        {
            var message = string.Format("错误：{0}->{1}", ErrorCodeHandle.GetError(errorCode), token);
            throw new ParserException(errorCode, token, message);
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
            {
                CurrentToken = Lexer.GetNextToken();
            }
            else
            {
                Error(ErrorCode.UNEXPECTED_TOKEN, CurrentToken);
            }
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
                    node.Stmts.Add(stmt);
                }
            }
            return node;
        }
        public AST Stmt()
        {
            // stmt: simple_stmt | compound_stmt
            if (CurrentToken.Type == TokenType.IF || CurrentToken.Type == TokenType.CASE)
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
            // simple_stmt: small_stmt NEWLINE
            var node = SmallStmt();
            Eat(TokenType.NEWLINE);
            return node;
        }
        public AST SmallStmt()
        {
            // small_stmt: (fun | assign_stmt)?
            if (CurrentToken.Type == TokenType.NAME)
            {
                var var = new Var(CurrentToken);
                Eat(TokenType.NAME);
                if (CurrentToken.Type == TokenType.COLON)
                    return Fun(var);
                else
                    return AssginStmt(var);
            }
            else if (CurrentToken.Type == TokenType.COLON)
                return Fun(null);
            return null;
        }
        public AST Fun(Var var)
        {
            // fun: (NAME)? : args
            // 上一步已经吃掉了NAME
            Eat(TokenType.COLON);
            return new FunCall(var, Args());
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
        public AST CompoundStmt()
        {
            // compound_stmt: if_stmt | case_stmt
            if (CurrentToken.Type == TokenType.IF)
                return IfStmt();
            else
                return CaseStmt();
        }
        public AST IfStmt()
        {
            // if_stmt: IF expr suite (ELIF expr suite)* (ELSE suite)?
            Eat(TokenType.IF);
            var expr = Expr();
            var thenSuite = Suite();
            List<IfStmt> elifs = null;
            if (CurrentToken.Type == TokenType.ELIF)
            {
                elifs = new List<IfStmt>();
                while (CurrentToken.Type == TokenType.ELIF)
                {
                    Eat(TokenType.ELIF);
                    var elifExpr = Expr();
                    var elifSuite = Suite();
                    elifs.Add(new IfStmt(elifExpr, elifSuite, null));
                }
            }
            if (CurrentToken.Type == TokenType.ELSE)
            {
                if (elifs == null)
                    elifs = new List<IfStmt>();
                Eat(TokenType.ELSE);
                var elseSuite = Suite();
                var el = new IfStmt(null, elseSuite, null);
                elifs.Add(el);
            }
            if (elifs != null)
                for (int i = 0; i < elifs.Count - 1; i++)
                    elifs[i].ElseStmt = elifs[i + 1];

            return new IfStmt(expr, thenSuite, elifs?[0]);
        }
        public AST CaseStmt()
        {
            // case_stmt: CASE expr? NEWLINE (WHEN expr? suite)*
            Eat(TokenType.CASE);
            AST expr = null;
            if (CurrentToken.Type != TokenType.NEWLINE)
                expr = Expr();
            Eat(TokenType.NEWLINE);
            var whens = new List<WhenStmt>();
            while (CurrentToken.Type == TokenType.WHEN)
            {
                Eat(TokenType.WHEN);
                AST whenExpr = null;
                if (CurrentToken.Type != TokenType.NEWLINE)
                    whenExpr = Expr();
                var whenSuite = Suite();
                whens.Add(new WhenStmt(whenExpr, whenSuite));
            }
            var node = new CaseStmt(expr, whens);
            return node;
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
                stmts.Add(stmt);
            }
            Eat(TokenType.DEDENT);
            return new Suite(stmts);
        }
        public AST Expr()
        {
            // expr: test
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
            if (CurrentToken.Type == TokenType.LPAREN || CurrentToken.Type == TokenType.DOT)
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
            // atom: LPAREN expr RPAREN | fun | NAME | INT_CONST | REAL_CONST | STR | TRUE | FALSE | NONE 
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
                var node = new None(CurrentToken);
                Eat(TokenType.NONE);
                return node;
            }
            else
            {
                Error(ErrorCode.UNEXPECTED_TOKEN, CurrentToken);
                return null;
            }
        }
        public AST Trailer()
        {
            // trailer: [ args ] | . NAME
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
                Error(ErrorCode.UNEXPECTED_TOKEN, CurrentToken);
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
                Error(ErrorCode.UNEXPECTED_TOKEN, CurrentToken);
            return node;
        }
    }
}
