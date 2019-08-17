using System;

namespace Stator.Editor
{
    public class CSharpInitVariable : CSharpStatement
    {
        public Type VariableType { get; set; }
        public string Name { get; set; }
        public CSharpStatement Initialization { get; set; }

        public CSharpInitVariable(Type variableType, string name, CSharpStatement initialization)
        {
            VariableType = variableType;
            Name = name;
            Initialization = initialization;
        }

        public override void Generate(IndentedStringBuilder builder)
        {
            builder.BeginAppend();
            if(VariableType == null)
            {
                builder.Append("var ");
            }
            else
            {
                builder.Append(VariableType.GetRightFullName() + " ");
            }

            builder.Append(Name);
            builder.Append(" = ");

            if(Initialization != null)
            {
                Initialization.Generate(builder);
            }
            builder.Append(";");
            builder.NewLine();
        }
    }
}