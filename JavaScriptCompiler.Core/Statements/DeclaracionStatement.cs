using JavaScriptCompiler.Core.Expressions;
using JavaScriptCompiler.Core.Interfaces;
using System;

namespace JavaScriptCompiler.Core.Statements
{
    public class DeclarationStatement : Statement
    {
        public DeclarationStatement(Id id)
        {
            Id = id;
          
        }

        public Id Id { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"var {Id.Generate()};{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            //EnvironmentManager.UpdateVariable(Id.Token.Lexeme, Expression.Evaluate());
        }

        public override void ValidateSemantic()
        {
            if (Id.GetExpressionType() != Type.Int && Id.GetExpressionType() != Type.Bool && Id.GetExpressionType() != Type.Float && Id.GetExpressionType() != Type.String)
            {
                throw new ApplicationException($"Type {Id.GetExpressionType()} is not a declarable type.");
            }
        }
    }
}