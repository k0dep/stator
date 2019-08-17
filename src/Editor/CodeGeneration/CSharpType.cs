namespace Stator.Editor
{
    public abstract class CSharpType : ICodeGenerator
    {
        public abstract void Generate(IndentedStringBuilder stringBuilder);
    }
}