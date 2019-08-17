using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class CSharpInvoke : CSharpStatement
    {
        public string Method { get; set; }
        public IEnumerable<CSharpStatement> Arguments { get; set; }

        public CSharpInvoke(string method, IEnumerable<CSharpStatement> arguments)
        {
            Method = method;
            Arguments = arguments;
        }

        public override void Generate(IndentedStringBuilder builder)
        {
            var argBuilders = Arguments.Select(t => {
                var b = new IndentedStringBuilder();
                t.Generate(b);
                return b.Builder.ToString();
            });
            var arguments = string.Join(", ", argBuilders);
            builder.Append($"{Method}({arguments})");
        }
    }
}
