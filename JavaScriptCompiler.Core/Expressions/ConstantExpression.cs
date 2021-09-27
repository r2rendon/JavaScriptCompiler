
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaScriptCompiler.Core.Expressions
{
    public class ConstantExpression : TypedExpression
    {
        public ConstantExpression(Token token, Type type, string lexeme) : base(token, type)
        {
            Lexeme = lexeme;
        }

        public string Lexeme { get; }

        public override dynamic Evaluate()
        {
            Console.WriteLine(type.Lexeme.ToString());
            switch (type.Lexeme.ToString())
            {
                case "datetime":
                    return DateTime.Parse(Lexeme);
                case "list<int>":
                    return Lexeme;
                case "year":
                case "month":
                case "day":
                    return int.Parse(Lexeme);
                default:
                    return null;
            }

        }

        public override string Generate()
        {
            if (Token.TokenType == TokenType.DateTimeConstant)
            {

                return $"new {Token.Lexeme}{Lexeme.ToString().Substring(0, 3)}/{Lexeme.Substring(3, 2)}/{Lexeme.Substring(5, 5)}";
            }
            return Token.Lexeme;
        }

        public override Type GetExpressionType()
        {
            return type;
        }
    }
}