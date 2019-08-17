using System.Linq;

namespace Stator.Editor
{
    public class CSharpSymbol : CSharpStatement
    {
        public static readonly CSharpSymbol NULL = new CSharpSymbol("null");
        public static readonly CSharpSymbol TRUE = new CSharpSymbol("true");
        public static readonly CSharpSymbol FALSE = new CSharpSymbol("false");

        public string Symbol { get; set; }
        public CSharpSymbol(string symbol)
        {
            Symbol = symbol;
        }

        public override void Generate(IndentedStringBuilder stringBuilder)
        {
            stringBuilder.Append(Symbol);
        }
    }
}