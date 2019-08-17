namespace Stator.Editor
{
    public class CSharpReturn : CSharpStatement
    {
        public CSharpStatement ReturnValue { get; set; }
        
        public CSharpReturn(CSharpStatement returnValue)
        {
            ReturnValue = returnValue;
        }

        public override void Generate(IndentedStringBuilder stringBuilder)
        {
            stringBuilder.BeginAppend();
            stringBuilder.Append("return ");

            ReturnValue.Generate(stringBuilder);

            stringBuilder.Append(";");
            stringBuilder.NewLine();
        }
    }
}