using System.Text;

namespace Stator.Editor
{
    public class IndentedStringBuilder
    {
        public StringBuilder Builder { get; set; }
        public int CurrentIndend { get; set; }

        public IndentedStringBuilder() : this(new StringBuilder(), 0)
        {
        }

        public IndentedStringBuilder(StringBuilder builder, int currentIndend)
        {
            this.Builder = builder;
            this.CurrentIndend = currentIndend;
        }

        public IndentedStringBuilder(IndentedStringBuilder builder, int inc)
        {
            this.Builder = builder.Builder;
            this.CurrentIndend = builder.CurrentIndend + inc;
        }

        public IndentedStringBuilder AppendLine(string value)
        {
            BeginAppend();
            Builder.AppendLine(value);
            return this;
        }

        public void BeginAppend()
        {
            for (int i = 0; i < CurrentIndend; i++)
            {
                Builder.Append("\t");
            }
        }

        public void Append(string value)
        {
            Builder.Append(value);
        }

        public void NewLine()
        {
            Builder.AppendLine();
        }

        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}