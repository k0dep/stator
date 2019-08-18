namespace Stator.Editor
{
    public class CSharpSpaceMember : CSharpClassMember
    {
        public int NewLineCount { get; set; }

        public CSharpSpaceMember(int newLineCount)
        {
            NewLineCount = newLineCount;
        }

        public override void Generate(IndentedStringBuilder builder)
        {
            for (int i = 0; i < NewLineCount; i++)
            {
                builder.NewLine();
            }
        }
    }
}