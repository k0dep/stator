namespace Stator.Editor
{
    public abstract class CSharpStatement : ICodeGenerator
    {
        public abstract void Generate(IndentedStringBuilder builder);
    }
}