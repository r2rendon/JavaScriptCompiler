using JavaScriptCompiler.Core;
using JavaScriptCompiler.Core.Expressions;
using JavaScriptCompiler.Core.Interfaces;
using JavaScriptCompiler.Core.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Type = JavaScriptCompiler.Core.Type;

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

        public Statement Parse()
        {
            return Program();
        }

        private Statement Program()
        {
            EnvironmentManager.PushContext();
            EnvironmentManager.AddMethod("Console.WriteLine", new Id(new Token
            {
                Lexeme = "console.log",
            }, Type.Void),
            new ArgumentExpression(new Token
            {
                Lexeme = ""
            },
            new Id(new Token
            {
                Lexeme = "arg1"
            }, Type.String)));

            EnvironmentManager.AddMethod("Console.ReadLine", new Id(new Token
            {
                Lexeme = "window.prompt",
            }, Type.Void),
            new ArgumentExpression(new Token
            {
                Lexeme = ""
            },
            new Id(new Token
            {
                Lexeme = "arg1"
            }, Type.String)));

            var block = Block();
            block.ValidateSemantic();
            var code = block.Generate(0);
            //code = code.Replace($"else:{Environment.NewLine}\tif", "elif");
            //File.WriteAllText("code.js", code);
            Console.WriteLine(code);
            return block;
        }

        private Statement Block()
        {
            Match(TokenType.OpenBrace);
            EnvironmentManager.PushContext();
            var stat = Decls();
            var statements = Stmts();
            Match(TokenType.CloseBrace);
            EnvironmentManager.PopContext();
            return new SequenceStatement(stat, statements);
            
        }

        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.CloseBrace)
            {//{}
                return null;
            }
            return new SequenceStatement(Stmt(), Stmts());
        }

        private Statement Stmt()
        {
            Expression expression;
            Statement statement1, statement2;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Identifier:
                    {
                        var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        Match(TokenType.Identifier);
                        switch (this.lookAhead.TokenType)
                        {
                            case TokenType.Assignation:
                                return AssignStmt(symbol.Id);
                            case TokenType.Increment:
                                return IncrementStmt(symbol.Id);
                            case TokenType.Decrement:
                                return DecrementStatement(symbol.Id);
                            default:
                                return CallStmt(symbol);
                        }
                    }
                case TokenType.IfKeyword:
                    {
                        Match(TokenType.IfKeyword);
                        Match(TokenType.LeftParens);
                        expression = Logical();
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        if (this.lookAhead.TokenType != TokenType.ElseKeyword)
                        {
                            return new IfStatement(expression as TypedExpression, statement1);
                        }
                        Match(TokenType.ElseKeyword);
                        statement2 = Stmt();
                        return new ElseStatement(expression as TypedExpression, statement1, statement2);
                    }
                case TokenType.ForeachKeyword:
                    return ForeachStatement();
                case TokenType.WhileKeyword:
                    {
                        Match(TokenType.WhileKeyword);
                        Match(TokenType.LeftParens);
                        expression = Logical();
                        Match(TokenType.RightParens);
                        statement1 = Stmt();

                        return new WhileStatement(expression as TypedExpression, statement1);
                    }
                default:
                    return Block();
            }
        }

        private Statement DecrementStatement(Id id)
        {
            Match(TokenType.Decrement);
            Match(TokenType.SemiColon);
            return new DecrementStatement(id);
        }

        private Statement IncrementStmt(Id id)
        {
            Match(TokenType.Increment);
            Match(TokenType.SemiColon);
            return new IncrementStatement(id);
        }
        private Expression Logical()
        {
            var expression = Eq();
            while (this.lookAhead.TokenType == TokenType.Or || this.lookAhead.TokenType == TokenType.And)
            {
                var token = lookAhead;
                Move();
                expression = new ConditionalExpression(token, expression as TypedExpression, Eq() as TypedExpression);

            }

            return expression;
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }

            return expression;
        }

        private Expression Rel()
        {
            var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }
            return expression;
        }

        private Expression Expr()
        {
            var expression = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Minus)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division || this.lookAhead.TokenType == TokenType.Mod)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Logical();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.BoolConstant:
                    constant = new Constant(lookAhead, Type.Bool);
                    Match(TokenType.BoolConstant);
                    return constant;
                case TokenType.Distinct:
                    Match(TokenType.Distinct);
                    return Logical();
                case TokenType.DateTimeConstant:
                    var token = this.lookAhead;
                    List<Token> list = new List<Token>();
                    //list.Add(lookAhead);
                    Match(TokenType.DateTimeConstant);
                    list.Add(lookAhead);
                    Match(TokenType.LeftParens);
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    //list.Add(lookAhead);
                    Match(TokenType.Division);
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    //list.Add(lookAhead);
                    Match(TokenType.Division);
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    list.Add(lookAhead);
                    Match(TokenType.RightParens);
                    return new ConstantExpression(token, Type.Date, string.Concat(list.Select(x => x.Lexeme)));
                case TokenType.ListKeyword:
                    {
                        token = lookAhead;
                        list = new List<Token>();
                        Match(TokenType.ListKeyword);
                        Match(TokenType.LessThan);
                        var listType = lookAhead;
                        switch (listType.TokenType)
                        {
                            case TokenType.IntKeyword:
                                Match(TokenType.IntKeyword);
                                break;
                            case TokenType.FloatKeyword:
                                Match(TokenType.FloatKeyword);
                                break;
                            case TokenType.BoolKeyword:
                                Match(TokenType.BoolKeyword);
                                break;
                            case TokenType.StringKeyword:
                                Match(TokenType.StringKeyword);
                                break;
                            default:
                                // Array type not recognized
                                break;
                        }
                        Match(TokenType.GreaterThan);
                        Match(TokenType.LeftParens);
                        list.Add(lookAhead);
                        switch (listType.TokenType)
                        {
                            case TokenType.FloatKeyword:
                                {
                                    Match(TokenType.FloatConstant);
                                    while (lookAhead.TokenType != TokenType.RightParens)
                                    {
                                        list.Add(lookAhead);
                                        Match(TokenType.Comma);
                                        list.Add(lookAhead);
                                        Match(TokenType.FloatConstant);
                                    }
                                    Match(TokenType.RightParens);
                                    return new ConstantExpression(token, Type.FloatList, string.Concat(list.Select(x => x.Lexeme)));
                                }
                            case TokenType.StringKeyword:
                                {
                                    Match(TokenType.StringConstant);
                                    while (lookAhead.TokenType != TokenType.RightParens)
                                    {
                                        list.Add(lookAhead);
                                        Match(TokenType.Comma);
                                        list.Add(lookAhead);
                                        Match(TokenType.StringConstant);
                                    }
                                    Match(TokenType.RightParens);
                                    return new ConstantExpression(token, Type.StringList, string.Concat(list.Select(x => x.Lexeme)));
                                }
                            case TokenType.BoolKeyword:
                                {
                                    Match(TokenType.BoolConstant);
                                    while (lookAhead.TokenType != TokenType.RightParens)
                                    {
                                        list.Add(lookAhead);
                                        Match(TokenType.Comma);
                                        list.Add(lookAhead);
                                        Match(TokenType.BoolConstant);
                                    }
                                    Match(TokenType.RightParens);
                                    return new ConstantExpression(token, Type.BoolList, string.Concat(list.Select(x => x.Lexeme)));
                                }
                            default:
                                Match(TokenType.IntConstant);
                                while (lookAhead.TokenType != TokenType.RightParens)
                                {
                                    list.Add(lookAhead);
                                    Match(TokenType.Comma);
                                    list.Add(lookAhead);
                                    Match(TokenType.IntConstant);
                                }
                                Match(TokenType.RightParens);
                                return new ConstantExpression(token, Type.IntList, string.Concat(list.Select(x => x.Lexeme)));
                        }
                    }
                default:
                    var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
            }
        }

        private Statement CallStmt(Symbol symbol)
        {
            Match(TokenType.LeftParens);
            var @params = OptParams();
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
            return new CallStatement(symbol.Id, @params, symbol.Attributes);
        }

        private Expression OptParams()
        {
            if (this.lookAhead.TokenType != TokenType.RightParens)
            {
                return Params();
            }
            return null;
        }

        private Expression Params()
        {
            //var expression = Eq();
            var expression = Logical();
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params() as TypedExpression);
            return expression;
        }

        private Statement AssignStmt(Id id)
        {
            Match(TokenType.Assignation);
            //var expression = Eq();
            var expression = Logical();
            Match(TokenType.SemiColon);
            return new AssignationStatement(id, expression as TypedExpression);
        }

        private Statement Decls()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.ListKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.BoolKeyword
                //this.lookAhead.TokenType == TokenType.DateKeyword
                )
            {
                return new SequenceStatement(Decl(), Decls());
            }
            return null;
        }

        private Statement Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    return new DeclarationStatement(id);
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    return new DeclarationStatement(id);
                case TokenType.DateTimeKeyword:
                    Match(TokenType.DateTimeKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Date);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    return new DeclarationStatement (id);
                case TokenType.BoolKeyword:
                    Match(TokenType.BoolKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Bool);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    return new DeclarationStatement(id);
                case TokenType.ListKeyword:
                    {
                        Match(TokenType.ListKeyword);
                        Match(TokenType.LessThan);
                        var listType = lookAhead;
                        switch (listType.TokenType)
                        {
                            case TokenType.FloatKeyword:
                                Match(TokenType.FloatKeyword);
                                Match(TokenType.GreaterThan);
                                token = lookAhead;
                                Match(TokenType.Identifier);
                                Match(TokenType.SemiColon);
                                id = new Id(token, Type.FloatList);
                                break;
                            case TokenType.StringKeyword:
                                Match(TokenType.StringKeyword);
                                Match(TokenType.GreaterThan);
                                token = lookAhead;
                                Match(TokenType.Identifier);
                                Match(TokenType.SemiColon);
                                id = new Id(token, Type.StringList);
                                break;
                            case TokenType.BoolKeyword:
                                Match(TokenType.BoolKeyword);
                                Match(TokenType.GreaterThan);
                                token = lookAhead;
                                Match(TokenType.Identifier);
                                Match(TokenType.SemiColon);
                                id = new Id(token, Type.BoolList);
                                break;
                            default:
                                Match(TokenType.IntKeyword);
                                Match(TokenType.GreaterThan);
                                token = lookAhead;
                                Match(TokenType.Identifier);
                                Match(TokenType.SemiColon);
                                id = new Id(token, Type.IntList);
                                break;
                        }

                        EnvironmentManager.AddVariable(token.Lexeme, id);
                        return new DeclarationStatement(id);

                    }
                default:
                    Match(TokenType.IntKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    return new DeclarationStatement(id);
               
            }
        }

        private Statement ForeachStatement()
        {
            //Match(TokenType.IfKeyword);
            //Match(TokenType.LeftParens);
            //expression = Logical();
            //Match(TokenType.RightParens);
            //statement1 = Stmt();
            //if (this.lookAhead.TokenType != TokenType.ElseKeyword)
            //{
            //    return new IfStatement(expression as TypedExpression, statement1);
            //}
            //Match(TokenType.ElseKeyword);
            //statement2 = Stmt();
            //return new ElseStatement(expression as TypedExpression, statement1, statement2);
            Match(TokenType.ForeachKeyword);
            Match(TokenType.LeftParens);
            var tmpVar = lookAhead;
            var tmpVarSymbol = EnvironmentManager.GetSymbol(tmpVar.Lexeme);
            Match(TokenType.Identifier);
            Match(TokenType.InKeyword);
            var array = lookAhead;
            var arraySymbol = EnvironmentManager.GetSymbol(array.Lexeme);
            Match(TokenType.Identifier);
            Match(TokenType.RightParens);
            var statement = Stmt();

            return new ForeachStatement(tmpVarSymbol.Id, arraySymbol.Id, statement);
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

        void IParser.Parse()
        {
            throw new NotImplementedException();
        }
    }
}