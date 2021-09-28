
using System;
using System.Collections.Generic;
using System.Linq;
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

        private int tmpInt;
        private float tmpFloat;
        private bool tmpBool;

        public override dynamic Evaluate()
        {
            Console.WriteLine(type.Lexeme.ToString());
            switch (type.Lexeme.ToString())
            {
                case "list<int>":
                    return Lexeme.Split(',').Where(m => int.TryParse(m, out tmpInt)).Select(m => int.Parse(m)).ToList();
                case "list<float>":
                    return Lexeme.Split(',').Where(m => float.TryParse(m, out tmpFloat)).Select(m => int.Parse(m)).ToList();
                case "list<string>":
                    return Lexeme.Split(',').ToList();
                case "list<bool>":
                    return Lexeme.Split(',').Where(m => bool.TryParse(m, out tmpBool)).Select(m => int.Parse(m)).ToList();
                default:
                    return null;
            }

        }

        public override string Generate()
        {
            if (Token.TokenType == TokenType.ListKeyword)
            {
                return $"[{Lexeme}]";
            }
            return Token.Lexeme;
        }

        public override Type GetExpressionType()
        {
            return type;
        }
    }
}