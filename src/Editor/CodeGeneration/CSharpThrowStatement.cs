namespace Stator.Editor
{
    public class CSharpThrowStatement : CSharpStatement
    {
        public CSharpStatement Statement { get; set; }
        
        public CSharpThrowStatement(CSharpStatement statement)
        {
            Statement = statement;
        }
        
        public override void Generate(IndentedStringBuilder builder)
        {
            builder.BeginAppend();
            builder.Append("throw ");
            Statement?.Generate(builder);
            builder.Append(";");
            builder.NewLine();
        }
    }
}