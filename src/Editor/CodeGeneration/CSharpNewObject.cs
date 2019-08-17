using System;
using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class CSharpNewObject : CSharpStatement
    {
        public Type NewType { get; set; }
        public IEnumerable<CSharpStatement> Arguments { get; set; }

        public CSharpNewObject(Type newType, IEnumerable<CSharpStatement> arguments)
        {
            NewType = newType;
            Arguments = arguments;
        }

        public override void Generate(IndentedStringBuilder stringBuilder)
        {
            var argBuilders = Arguments.Select(t => {
                var b = new IndentedStringBuilder();
                t.Generate(b);
                return b.Builder.ToString();
            });
            var arguments = string.Join(", ", argBuilders);
            stringBuilder.Append($"new {NewType.GetRightFullName()}({arguments})");
        }
    }
}