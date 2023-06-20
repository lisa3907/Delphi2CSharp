using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DelphiParser;

namespace Delphi.Generated
{
    static class Util
    {
        private static Dictionary<string, string> OperatorsMap = new Dictionary<string, string>
        {
            ["AND"] = "&&",
            ["DIV"] = "/",
            ["MOD"] = "%",
            ["NOT"] = "!",
            ["OR"] = "||",
            ["+"] = "+",
            ["-"] = "-",
            ["*"] = "*",
            ["="] = "==",
            ["<>"] = "!=",
            ["<"] = "<",
            ["<="] = "<=",
            [">"] = ">",
            [">="] = ">=",
        };

        public static string GetString(IParseTree tree)
        {
            string s = tree.GetText();
            s = s.Substring(1, s.Length - 2);
            s = s.Replace("\"", "\\\"");
            s = s.Replace("''", "'");
            return $"\"{s}\"";
        }

        public static string FormatTerm(IList<IToken> operations, IEnumerable<string> operands)
        {
            using (var e = operands.GetEnumerator())
            {
                e.MoveNext();

                if (operations.Count == 0)
                    return e.Current;

                var sb = new StringBuilder("(")
                    .Append(e.Current);

                for (int i = 0; e.MoveNext(); ++i)
                    sb.Append(OperatorsMap[operations[i].Text.ToUpper()]).Append(e.Current);

                return sb.Append(")").ToString();
            }
        }
    }

    public class DelphiListener : DelphiBaseListener
    {
        private readonly List<object> _codeParts = new List<object>();

        private readonly Dictionary<string, bool[]> _procedures = new Dictionary<string, bool[]>()
        {
            ["write"] = null,
            ["writeln"] = null,
            ["readln"] = new[] { true }
        };

        public string Code
        {
            get
            {
                return string.Join("", _codeParts.Select(i => i is IParseTree ? ((IParseTree)i).GetText() : i));
            }
        }

        protected void Append(params object[] objects)
        {
            _codeParts.AddRange(objects);
        }
        protected void AppendLine(params object[] objects)
        {
            _codeParts.AddRange(objects);
            _codeParts.Add(Environment.NewLine);
        }

        private static string AsType(IParseTree tree)
        {
            return tree.GetText() == "String" ? "string" : "int";
        }

        public override void EnterProgram(ProgramContext context)
        {
            AppendLine("using System;");
            AppendLine("class Program {");
            AppendLine("static void readln<T>(ref T arg) {");
            AppendLine("    arg = (T)Convert.ChangeType(Console.ReadLine(), typeof(T));");
            AppendLine("}");
            AppendLine("static void writeln(params object[] args) {");
            AppendLine("    Console.WriteLine(string.Join(string.Empty, args));");
            AppendLine("}");
            AppendLine("static void write(params object[] args) {");
            AppendLine("    Console.Write(string.Join(string.Empty, args));");
            AppendLine("}");

        }
        public override void ExitProgram(ProgramContext context)
        {
            AppendLine("} // program");
        }

        public override void EnterBlock(BlockContext context)
        {
            AppendLine("{ // block");
        }
        public override void ExitBlock(BlockContext context)
        {
            AppendLine("} // block");
        }

        public override void EnterCompoundStatement(CompoundStatementContext context)
        {
            AppendLine("{ // compound");
        }
        public override void ExitCompoundStatement(CompoundStatementContext context)
        {
            AppendLine("} // compound");
        }
    }
}
