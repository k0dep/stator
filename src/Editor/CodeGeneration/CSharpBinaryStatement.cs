namespace Stator.Editor
{
    public class CSharpBinaryStatement : CSharpStatement
    {
        public CSharpStatement Left { get; set; }
        public CSharpStatement Right { get; set; }
        public string Operator { get; set; }
        public bool UseIndent { get; set; }

        public CSharpBinaryStatement(CSharpStatement left, CSharpStatement right, string @operator, bool useIndent = false)
        {
            Left = left;
            Right = right;
            Operator = @operator;
            UseIndent = useIndent;
        }

        public override void Generate(IndentedStringBuilder builder)
        {
            if(UseIndent)
            {
                builder.BeginAppend();
            }

            Left.Generate(builder);
            builder.Append($" {Operator} ");
            Right.Generate(builder);

            if(UseIndent)
            {
                builder.Append(";");
                builder.NewLine();
            }
        }
    }
}