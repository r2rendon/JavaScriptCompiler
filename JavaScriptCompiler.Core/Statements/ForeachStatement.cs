using JavaScriptCompiler.Core.Expressions;
using JavaScriptCompiler.Core.Interfaces;
using System;

namespace JavaScriptCompiler.Core.Statements
{
    public class ForeachStatement : Statement
    {
        public ForeachStatement(Id tmpVar, Id arrayVar, Statement statement)
        {
            TmpVar = tmpVar;
            ArrayVar = arrayVar;
            Statement = statement;
        }

        public Id TmpVar { get; }
        public Id ArrayVar { get; }
        public Statement Statement { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"{this.ArrayVar.Generate()}.forEach(({this.TmpVar.Generate()}) => ){{{Environment.NewLine}";
            code += $"{this.Statement.Generate(tabs+1)}{Environment.NewLine}";
            code += $"}});{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            //if (Expression.Evaluate())
            //{
            //    Statement.Interpret();
            //}
        }

        public override void ValidateSemantic()
        {
            if (ArrayVar.GetExpressionType() != Type.List)
            {
                throw new ApplicationException($"Type: {ArrayVar.GetExpressionType()} is not iterable");
            }
        }
    }
}