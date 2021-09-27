using JavaScriptCompiler.Core.Expressions;
using JavaScriptCompiler.Core.Interfaces;
using System;

namespace JavaScriptCompiler.Core.Statements
{
    public class DecrementStatement : Statement
    {
        public DecrementStatement(Id id)
        {
            Id = id;
        }

        public Id Id { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"{Id.Generate()}--;{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            //Console.WriteLine(Id.Evaluate());
            //EnvironmentManager.UpdateVariable(Id.Token.Lexeme, Expression.Evaluate());
        }

        public override void ValidateSemantic()
        {
            if (Id.GetExpressionType() != Type.Int && Id.GetExpressionType() != Type.Float)
            {
                throw new ApplicationException($"Cannot decrement type: {Id.GetExpressionType()}");
            }
        }
    }
}