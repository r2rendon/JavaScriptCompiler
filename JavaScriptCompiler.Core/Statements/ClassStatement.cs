using JavaScriptCompiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaScriptCompiler.Core.Statements
{
    public class ClassStatement : Statement
    {
        public ClassStatement(Id id, Statement statement)
        {
            this.id = id;
            this.statement = statement;
        }

        public Id id { get; }

        public Statement statement { get; }


        public override string Generate(int tabs)
        {
            var key = "{";
            var closeKey = "}";
            var code = GetCodeInit(tabs);
            code += $"{Environment.NewLine}class {id.Generate()}{key}{Environment.NewLine}";
            code += GetCodeInit(tabs);
            code += $"{statement.Generate(tabs)}{closeKey}{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            statement.Interpret();
        }

        public override void ValidateSemantic()
        {
            statement.ValidateSemantic();
        }
    }
}
