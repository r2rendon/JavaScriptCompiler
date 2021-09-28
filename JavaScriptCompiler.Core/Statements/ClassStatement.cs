using JavaScriptCompiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaScriptCompiler.Core.Statements
{
    class ClassStatement : Statement
    {
        public ClassStatement(Id iD, Statement state)
        {
            this.iD = iD;
            this.state = state;
        }

        public Id iD { get; }

        public Statement state { get; }


        public override string Generate(int tabs)
        {
            var key = "{";
            var closeKey = "}";
            var code = GetCodeInit(tabs);
            code += $"class {iD.Generate()}{key}{Environment.NewLine}";
            code += GetCodeInit(tabs);
            code += $"{state.Generate(tabs)}{closeKey}{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            state.Interpret();
        }

        public override void ValidateSemantic()
        {
            state.ValidateSemantic();
        }
    }
}
