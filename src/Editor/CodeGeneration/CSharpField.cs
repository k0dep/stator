using System;

namespace Stator.Editor
{
    public class CSharpField : CSharpClassMember
    {
        public Type FieldType { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }

        public CSharpField(Type fieldType, string name, bool isPublic)
        {
            this.FieldType = fieldType;
            this.Name = name;
            this.IsPublic = isPublic;
        }

        public override void Generate(IndentedStringBuilder stringBuilder)
        {
            var modifier = IsPublic ? "public" : "private";
            stringBuilder.AppendLine($"{modifier} {FieldType.GetRightFullName()} {Name};");
        }
    }
}