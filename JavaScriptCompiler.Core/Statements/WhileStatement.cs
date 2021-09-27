using JavaScriptCompiler.Core.Expressions;
using JavaScriptCompiler.Core.Interfaces;
using System;

namespace JavaScriptCompiler.Core.Statements
{
    public class WhileStatement : Statement
    {
        public WhileStatement(TypedExpression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
        }

        public TypedExpression Expression { get; }
        public Statement Statement { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"while({Expression.Generate()}){{{Environment.NewLine}";
            code += $"{Statement.Generate(tabs + 1)}{Environment.NewLine}}}{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            if (Expression.Evaluate())
            {
                Statement.Interpret();
            }
        }

        public override void ValidateSemantic()
        {
            if (Expression.GetExpressionType() != Type.Bool)
            {
                throw new ApplicationException("A boolean is required in a while");
            }
        }
    }
}