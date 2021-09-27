using JavaScriptCompiler.Core;
using JavaScriptCompiler.Core.Interfaces;
using System;

namespace JavaScriptCompiler.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }
        public void Parse()
        {
            Program();
        }

        private void Program()
        {
            Block();
        }
        private void Block()
        {
            Match(TokenType.OpenBrace);
            //EnvironmentManager.PushContext();
            Decls();
            Stmts();
            Match(TokenType.CloseBrace);
            //EnvironmentManager.PopContext();
        }

       
        private void Stmts()
        {
            if (this.lookAhead.TokenType != TokenType.CloseBrace)
            {//{}
                Stmt();
                Stmts();
            }
        }

        private void Stmt()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    break;
                case TokenType.IfKeyword:
                    IfStmt();
                    break;
                case TokenType.ForeachKeyword:
                    ForeachStatement();
                    break;
                case TokenType.Identifier:
                    Match(TokenType.Identifier);
                    if (this.lookAhead.TokenType == TokenType.Assignation)
                    {
                        Assignation();
                    }
                    CallStmt();
                    break;
                default:
                    Block();
                    break;
            }
        }

        private void CallStmt()
        {
            Match(TokenType.LeftParens);
            OptParams();
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
        }

        private void OptParams()
        {
            if (this.lookAhead.TokenType != TokenType.RightParens)
            {
                Params();
            }
        }

        private void Params()
        {
            Eq();
            if (this.lookAhead.TokenType == TokenType.Comma)
            {
                Match(TokenType.Comma);
                //expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params() as TypedExpression);
                //return expression;
            }
        }

        private void ForeachStatement()
        {
            Match(TokenType.ForeachKeyword);
            Match(TokenType.LeftParens);
            Match(TokenType.Identifier);
            Match(TokenType.InKeyword);
            Match(TokenType.Identifier);
            Match(TokenType.RightParens);
            Block();
        }

        private void IfStmt()
        {
            Match(TokenType.IfKeyword);
            Match(TokenType.LeftParens);
            Eq();
            Match(TokenType.RightParens);
            Block();
        }

        private void Eq()
        {
            RelOrLog();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                Move();
                RelOrLog();
            }
        }

        private void RelOrLog()
        {
            Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan
                || this.lookAhead.TokenType == TokenType.Equal
                || this.lookAhead.TokenType == TokenType.NotEqual
                || this.lookAhead.TokenType == TokenType.And
                || this.lookAhead.TokenType == TokenType.Or
                || this.lookAhead.TokenType == TokenType.Distinct)
            {
                Move();
                Expr();
            }
        }
        

        private void Expr()
        {
            Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Minus)
            {
                Move();
                Term();
            }
        }

        private void Term()
        {
            Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division || this.lookAhead.TokenType == TokenType.Mod)
            {
                Move();
                Factor();
            }
        }

       


        private void Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        Eq();
                        Match(TokenType.RightParens);
                    }
                    break;
                case TokenType.IntConstant:
                    Match(TokenType.IntConstant);
                    break;
                case TokenType.FloatConstant:
                    Match(TokenType.FloatConstant);
                    break;
                case TokenType.StringConstant:
                    Match(TokenType.StringConstant);
                    break;
                case TokenType.BoolConstant:
                    Match(TokenType.StringConstant);
                    break;
                default:
                    Match(TokenType.Identifier);
                    break;
            }
        }

        private void Init()
        {
            Code();
        }

        private void Code()
        {
            Decls();
            Assignations();
        }

        private void Assignations()
        {
            if (this.lookAhead.TokenType == TokenType.Identifier)
            {
                Assignation();
                Assignations();
            }
        }

        private void Assignation()
        {
            Match(TokenType.Assignation);
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Assignation:
                    {
                        Match(TokenType.Assignation);
                        Eq();
                        Match(TokenType.SemiColon);
                    }
                    break;
                case TokenType.Decrement:
                    Match(TokenType.Decrement);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.Increment:
                    Match(TokenType.Increment);
                    Match(TokenType.SemiColon);
                    break;
            }
        }

        private void Decls()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword
                || this.lookAhead.TokenType == TokenType.FloatKeyword
                || this.lookAhead.TokenType == TokenType.StringKeyword
                || this.lookAhead.TokenType == TokenType.BoolKeyword
                || this.lookAhead.TokenType == TokenType.DateTimeKeyword)
            {
                Decl();
                Decls();
            }
        }

        private void InnerDecls()
        {
            if (this.LookAheadIsType())
            {
                Decls();
            }
        }

        private void Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.DateTimeKeyword:
                    Match(TokenType.DateTimeKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.ClassKeyword:
                    Match(TokenType.ClassKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.BoolKeyword:
                    Match(TokenType.BoolKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                default:
                    Match(TokenType.IntKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }

        private bool LookAheadIsType()
        {
            return this.lookAhead.TokenType == TokenType.IntKeyword   ||
                this.lookAhead.TokenType == TokenType.StringKeyword   ||
                this.lookAhead.TokenType == TokenType.FloatKeyword    ||
                this.lookAhead.TokenType == TokenType.DateTimeKeyword ||
                this.lookAhead.TokenType == TokenType.ClassKeyword;
        }
    }
}
