using System.Collections.Generic;

namespace Stator.Editor
{
    public class CSharpFile : ICodeGenerator
    {
        public IEnumerable<string> Usings { get; set; }
        public IEnumerable<CSharpNamespace> Namespaces { get; set; }

        public CSharpFile(IEnumerable<string> usings, IEnumerable<CSharpNamespace> namespaces)
        {
            Usings = usings;
            Namespaces = namespaces;
        }

        public void Generate(IndentedStringBuilder builder)
        {
            foreach(var usingItem in Usings)
            {
                builder.AppendLine($"using {usingItem};");
            }

            builder.AppendLine("");

            foreach(var namespaceItem in Namespaces)
            {
                namespaceItem.Generate(builder);
            }
        }
    }
}