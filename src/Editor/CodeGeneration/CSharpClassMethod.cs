using System;
using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class CSharpClassMethod : CSharpClassMember
    {
        public Type ReturnType { get; set; }
        public string Name { get; set; }
        public IEnumerable<MethodParameter> Parameters { get; set; }
        public bool IsPublic { get; set; }
        public IEnumerable<CSharpStatement> Statements { get; set; }

        public CSharpClassMethod(Type returnType, string name, bool isPublic)
        {
            this.ReturnType = returnType;
            this.Name = name;
            this.IsPublic = isPublic;
        }

        public CSharpClassMethod(Type returnType, string name, IEnumerable<MethodParameter> parameters, bool isPublic, IEnumerable<CSharpStatement> statements)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
            IsPublic = isPublic;
            Statements = statements;
        }

        public override void Generate(IndentedStringBuilder stringBuilder)
        {
            var accessMode = IsPublic ? "public" : "private";
            var returnType = ReturnType == typeof(void) ? "void" : ReturnType.GetRightFullName();
            var parameters = string.Join(", ", Parameters.Select(t => t.ToString()));
            stringBuilder.AppendLine($"{accessMode} {returnType} {Name}({parameters})");
            stringBuilder.AppendLine("{");

            var indentedBuilde = new IndentedStringBuilder(stringBuilder.Builder, stringBuilder.CurrentIndend + 1);
            foreach(var statement in Statements)
            {
                statement.Generate(indentedBuilde);
            }

            stringBuilder.AppendLine("}");
        }
    }
}