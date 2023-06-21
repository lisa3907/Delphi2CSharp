using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public override void EnterUnit([NotNull] UnitContext context)
        {
            base.EnterUnit(context);
        }

        public override void ExitUnit([NotNull] UnitContext context)
        {
            base.ExitUnit(context);
        }

        public override void EnterUnitHead([NotNull] UnitHeadContext context)
        {
            base.EnterUnitHead(context);
        }

        public override void ExitUnitHead([NotNull] UnitHeadContext context)
        {
            base.ExitUnitHead(context);

            AppendLine($"namespace {context.children[1].GetText()};");
        }

        public override void EnterUnitBlock([NotNull] UnitBlockContext context)
        {
            base.EnterUnitBlock(context);
        }

        public override void ExitUnitBlock([NotNull] UnitBlockContext context)
        {
            base.ExitUnitBlock(context);
        }

        public override void EnterIfStatement([NotNull] IfStatementContext context)
        {
            Append("if (");
        }

        public override void ExitIfStatement([NotNull] IfStatementContext context)
        {
            Append(context.expression().GetText()); // 여기에 실제 문장 변환 로직이 들어갈 수 있습니다.
            AppendLine(") {");
            Append("}\n");
        }

        public override void EnterFactor([NotNull] FactorContext context)
        {
            base.EnterFactor(context);
        }

        public override void ExitFactor([NotNull] FactorContext context)
        {
            base.ExitFactor(context);
        }

        public override void EnterStringFactor([NotNull] StringFactorContext context)
        {
            base.EnterStringFactor(context);
        }

        public override void ExitStringFactor([NotNull] StringFactorContext context)
        {
            base.ExitStringFactor(context);
        }

        public override void EnterUsedKeywordsAsNames([NotNull] UsedKeywordsAsNamesContext context)
        {
            base.EnterUsedKeywordsAsNames(context);
        }

        public override void ExitUsedKeywordsAsNames([NotNull] UsedKeywordsAsNamesContext context)
        {
            base.ExitUsedKeywordsAsNames(context);
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
