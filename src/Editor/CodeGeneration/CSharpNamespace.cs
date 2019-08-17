using System.Collections.Generic;

namespace Stator.Editor
{
    public class CSharpNamespace : ICodeGenerator
    {
        public string Name { get; set; }
        public IEnumerable<CSharpType> Types { get; set; }

        public CSharpNamespace(string name, IEnumerable<CSharpType> types)
        {
            Name = name;
            Types = types;
        }

        public void Generate(IndentedStringBuilder builder)
        {

            if (string.IsNullOrEmpty(Name))
            {
                foreach (var type in Types)
                {
                    type.Generate(builder);
                }
            }

            var indentBuilder = new IndentedStringBuilder(builder, 1);

            builder.AppendLine($"namespace {Name}")
                    .AppendLine("{");

            foreach (var type in Types)
            {
                type.Generate(indentBuilder);
            }

            builder.AppendLine("}");
        }
    }
}