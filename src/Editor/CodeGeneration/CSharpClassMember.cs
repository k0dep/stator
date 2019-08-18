namespace Stator.Editor
{
    public abstract class CSharpClassMember : ICodeGenerator
    {
        public abstract void Generate(IndentedStringBuilder builder);
    }
}