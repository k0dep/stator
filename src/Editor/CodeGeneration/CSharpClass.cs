using System.Collections.Generic;

namespace Stator.Editor
{
    public class CSharpClass : CSharpType
    {
        public string Name { get; set; }
        public IEnumerable<CSharpClassMember> Members { get; set; }
        public bool IsPartial { get; set; }

        public CSharpClass(string name, IEnumerable<CSharpClassMember> members, bool isPartial)
        {
            Name = name;
            Members = members;
            IsPartial = isPartial;
        }

        public override void Generate(IndentedStringBuilder builder)
        {
            var modifiers = IsPartial ? "partial " : "";
            builder.AppendLine($"public {modifiers}class {Name}");
            builder.AppendLine("{");

            var indentBuilder = new IndentedStringBuilder(builder, 1);
            foreach(var member in Members)
            {
                member.Generate(indentBuilder);
            }

            builder.AppendLine("}");
        }
    }
}