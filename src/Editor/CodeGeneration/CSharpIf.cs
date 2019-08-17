using System.Collections.Generic;

namespace Stator.Editor
{
    public class CSharpIf : CSharpStatement
    {
        public CSharpStatement Condition { get; set; }
        public IEnumerable<CSharpStatement> Statements { get; set; }

        public CSharpIf(CSharpStatement condition, IEnumerable<CSharpStatement> statements)
        {
            Condition = condition;
            Statements = statements;
        }

        public override void Generate(IndentedStringBuilder builder)
        {
            builder.BeginAppend();
            builder.Append("if (");
            Condition.Generate(builder);
            builder.Append(")");
            builder.NewLine();

            builder.AppendLine("{");

            var indentedBuilder = new IndentedStringBuilder(builder, 1);
            foreach (var statement in Statements)
            {
                statement.Generate(indentedBuilder);
            }

            builder.AppendLine("}");
        }
    }
}