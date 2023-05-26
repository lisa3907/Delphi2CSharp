using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ConvertDelphiToCSharp
{
	/// <summary>
	/// This class converts NON-VISUAL Delphi code to C#.  
	/// It is not complete, but it has been able
	/// to adequately convert about 80% of my own 
	/// libraries.  The program flow is procedural,
	/// not OO, and it therfore does not use a tokenizer.  
	/// 
	/// It is suggested that all Delphi
	/// source code be standardized using DelForEx 
	/// (Delphi for Experts), which is freeware, and is
	/// available for versions 2-7.
	/// 
	/// The class does not do a good-looking job of formatting. 
	/// It would be great to figure out how to call/apply the 
	/// VS.Net code formatter on the results. 
	/// 
	/// Also, there are known deficiencies such as Delphi
	/// VCL concepts like streams and StringReplace not being supported
	/// yet.  Let's make it better!
	/// 
	/// 2003 08 11	RHRuff  Added code to provide closing brackets on methods
	///						that contained VAR or CONST sections.  
	///						Fixed decrementing FOR loops.
	///						
	///	PRIORITY TO DO:		Fix conversion of strings which contain C# string literals.
	/// 
	/// </summary>
	public class frmD2C : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.RichTextBox tb1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtDelphiDotNetSourceFolder;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmD2C()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.tb1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConvert = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDelphiDotNetSourceFolder = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tb1
            // 
            this.tb1.AutoSize = true;
            this.tb1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tb1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb1.Location = new System.Drawing.Point(0, 92);
            this.tb1.Name = "tb1";
            this.tb1.Size = new System.Drawing.Size(904, 537);
            this.tb1.TabIndex = 2;
            this.tb1.Text = "";
            this.tb1.WordWrap = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 34);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(730, 9);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(124, 34);
            this.btnConvert.TabIndex = 1;
            this.btnConvert.Text = "Convert";
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(104, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(662, 26);
            this.label2.TabIndex = 5;
            this.label2.Text = "label2";
            // 
            // txtDelphiDotNetSourceFolder
            // 
            this.txtDelphiDotNetSourceFolder.Location = new System.Drawing.Point(106, 17);
            this.txtDelphiDotNetSourceFolder.Name = "txtDelphiDotNetSourceFolder";
            this.txtDelphiDotNetSourceFolder.Size = new System.Drawing.Size(547, 21);
            this.txtDelphiDotNetSourceFolder.TabIndex = 6;
            // 
            // frmD2C
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(904, 629);
            this.Controls.Add(this.txtDelphiDotNetSourceFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb1);
            this.Controls.Add(this.btnConvert);
            this.Name = "frmD2C";
            this.Text = "Convert Delphi to C#";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmD2C());
		}

/*		const string CSharpStringReplaceTemplate = 
			"	int zz;\n" + 
			"	zz = CompareInfo.IndexOf(InString, OfString, CompareOptions);\n" + 
			"	while (zz > 0)\n" +
			"	{\n" +
			"		InString.Remove(zz, OfString.Length);\n" +
			"		InString.Insert(zz, WithString);\n" +
			"	}\n";
		const string IgnoreCase = "CompareOptions.IgnoreCase";
*/		

		private static string DeleteExtraSpaces(string someText)
		{
			StringBuilder tmp = new StringBuilder(someText);
			tmp = tmp.Replace("  ", " ");
			return tmp.ToString(); 
		}
		private bool HasNoInternalSpaces(string aPhrase)
		{
			aPhrase = aPhrase.Trim();
			return (aPhrase.IndexOf(" ") == -1);
		}

		private int CountOccurences(string someText, char[] someChars)
		{
			someText = someText.Trim();
			int lngth = someText.Length;
			int i;
			int j;
			int cnt = 0;
			for (i = 0; i < lngth; i++)
			{
				for (j = 0; j < someChars.Length; j++)
				{
					if (someText[i] == someChars[j])
					{
						cnt++;
					}
				}
				
			}
			return cnt;
		}
		
		private string ChangeNonBracketedSyntax(string delphiCode)
		{
			string[] delphi = new string[] {"{",  "}", " True", " False", " Continue", " Break", " If",
											   "Else", "end;", " end\n", "Then", "TStringList", "TStrings", "begin",
											   "<>", "Exit",   ".Strings[", "String", "#13#10",
											   " not ", " or ", " and ", "if ", " then",
											   " while ",  " do ", "String", " Integer", " integer", "TDateTime",
											   ";", "overload", "\\", "\"", "''"
										   };
			string[] CSharp = new String[] {"/*", "*/", " true", " false", " continue", " break", " if",
											   "else", "}",     "}",     "then", "string[]",    "string[]", "{\n", 
											   "!=", "return", "[",         "string", "'\\n'",
											   " !",    " | ", " & "   , " if (", ")",
											   " while (", ") ",    "string", " int",     " int",     "DateTime",
											   ";\n", " ", "\\\\", "\\\"","\'"
											};
			string csharpCode;
			int i;

			csharpCode = delphiCode;
			for (i = delphi.GetLowerBound(0); i <= delphi.GetUpperBound(0); i++)
			{
				csharpCode = csharpCode.Replace(delphi[i], CSharp[i]);
			}

			return csharpCode;
		}

		private string[] MakeLinesWhole(string[] lines)
		{
			int i;
			int pos;
			int nbrLines = lines.Length;
			int lineNbr = 0;
			int leftParen = 0;
			int rightParen = 0;
			string aLine = "";
			string[] newCode = new string[nbrLines];
			string textToExamine;
			bool inVars = false;
			bool inComment = false;
			bool inMethod = false;

			for (i = 0; i < nbrLines; i++)
			{
				textToExamine = lines[i];

				if (inComment)
				{
					pos = textToExamine.IndexOf("*/");
					if (pos > -1)
					{
						inComment = false;
						textToExamine.Replace("*/", "*/\n");
						aLine = aLine + " " + textToExamine.Substring(0, pos + 2);
						newCode.SetValue(aLine, lineNbr++);
						textToExamine = textToExamine.Substring(pos + 2, textToExamine.Length - pos - 2);
					}
				}

				if (inVars)
				{
					if ((textToExamine.TrimStart().StartsWith("begin ")) | (textToExamine.Trim() == "begin") | 
						(textToExamine.Trim() == "{") | (textToExamine.TrimStart().StartsWith("{ ")) |
						(textToExamine.IndexOf(")") > -1))
					{
						inVars = false;
						newCode.SetValue(aLine, lineNbr++);
						aLine = "";
					}
					else
					{
						aLine = aLine + " " + textToExamine.TrimEnd();
					}
					continue;
				}

				if ((aLine == "") && ((textToExamine.TrimStart().StartsWith("var ")) | (textToExamine.Trim() == "var")))
				{
					inVars = true;
					aLine = textToExamine;
					continue;
				}

				if (StandsAlone(textToExamine.Trim()))
				{
					if (aLine.Trim().Length > 0)
					{
						newCode.SetValue(aLine, lineNbr++);
					}
					newCode.SetValue(textToExamine, lineNbr++);
					aLine = "";
					continue;
				}

				//  See if we're in a method prototype
				if ( textToExamine.TrimStart().ToLower().StartsWith("procedure ") | textToExamine.TrimStart().ToLower().StartsWith("function "))
				{
					inMethod = true;
					leftParen = 0;
					rightParen = 0;
				}
				//  If we are in a method prototype, make sure that the whole prototype gets included (do not get confused by "var" params)
				if (inMethod)
				{
					leftParen = leftParen + CountOccurences(textToExamine, new char[] {'('});
					rightParen = rightParen + CountOccurences(textToExamine, new char[] {')'});
					if (leftParen == rightParen)
					{
						inMethod = false;
					}
				}
								
				aLine = aLine + " " + textToExamine.TrimEnd();
				if ( aLine.EndsWith(";") && (!inMethod))
				{
					aLine = DoMoreComplexChanges(aLine);
					newCode.SetValue(aLine, lineNbr++);
					aLine = "";
				}

				pos = textToExamine.IndexOf("/*");
				if (pos > -1)
				{
					if (!(textToExamine.IndexOf("*/", pos) > -1))
					{
						inComment = true;
					}
				}
			
			}

			return newCode;
		}

		private bool StandsAlone(string aPhrase)
		{
			if ((aPhrase.Trim() == "{") | (aPhrase.Trim() == "}") | (aPhrase.Trim() == "else"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private string DoMoreComplexChanges(string oneLine)
		{
			string[] words = oneLine.Split(' ');
			int i;
			int nbrWords = words.Length;
			for (i = 1; i < nbrWords; i++)
			{
				if ((words[i].ToLower() == "else") & (!words[i - 1].TrimEnd().EndsWith(";")) & (!words[i - 1].TrimEnd().EndsWith("}")))
				{
					words[i - 1] = words[i - 1] + ";";
				}
			}
			return String.Join(" ", words);
		}

		private string[] FindUsesClause(string[] inLines)
		{
			int i;
			int j;
			int p;
			int length;
			string[] answer = new string[50];  // arbitrary limit
			string aLine, oneLine;
			for (i=0; i< inLines.Length; i++)
			{
				aLine = inLines[i];
				if (aLine.TrimStart().ToLower().StartsWith("uses "))
				{
					for (j = i; j < inLines.Length; j++)
					{
						oneLine = inLines[j];
						if (j == i) 
						{
							length = oneLine.Length;
							if (length > "uses ".Length)
							{
								oneLine = oneLine.Substring(6, length - 6 + 1);
							}
						}

						p = oneLine.IndexOf(";");
						if (p > 0)
						{
							oneLine = oneLine.Substring(1, p - 1);
						}

						oneLine.Split(new char[]{','}).CopyTo(answer, answer.Length);

					}
				}
			}
			return answer;
		}

		private string[] ConvertUsesClause(string[] uses)
		{
			bool foundMatch;
			int i, n;
			int length;
			string oneUses;
			string aFile;
			string aDelphiDotNetUses;
			string[] files = Directory.GetFiles(txtDelphiDotNetSourceFolder.Text);
			string[] usesFiles = new string[files.Length];
			string[] answer = new string[uses.Length];
			for (i=0; i < files.Length; i++)
			{
				aFile = files[i];
				length = aFile.Split(new char[]{'.'}).Length;
				if (length > 1)
				{
					aDelphiDotNetUses = aFile.Split(new char[]{'.'})[length - 2];
					usesFiles[i] = aDelphiDotNetUses;
				}
			}
			for (i = 0; i < uses.Length; i++)
			{
				oneUses = uses[i].Trim();
				if (oneUses.Length == 0) continue;
				foundMatch = false;
				for (n=0; n < uses.Length; n++)
				{
					if (oneUses == usesFiles[n])
					{
						foundMatch = true;
						answer[i] = files[n];
						break;
					}
				}
				if (!foundMatch) answer[i] = oneUses;
			}
			return answer;
		}


		private void FindParenPair(string aLine, int startFrom, out int left, out int right)
		{
			left = aLine.IndexOf("(", startFrom);
			char[] letters = aLine.ToCharArray();
			int i;
			int nbrLetters = letters.Length;
			int cnt = 0;
			right = 0;
			for (i = left; i < nbrLetters; i++)
			{
				if (letters[i] == '(')
				{
					cnt++;
				}
				if (letters[i] == ')')
				{
					cnt--;
					if (cnt == 0)
					{
						right = i;
						break;
					}
				}
			}
		}

		private void FindParamOffsets(string dataFromInsideParens, out int[] offsets)
		{
			const int MaxArrayElements = 5;
			int lngth = dataFromInsideParens.Length;
			int i;
			int cnt = 0;
			int ndx = 0;
			char letter;
			int[] localOffsets = new int[MaxArrayElements];  // artificial limit
			
			//start from right
			for (i = lngth; i > 0; i--)
			{
				letter = dataFromInsideParens[i];
				if (letter == ')')
				{
					cnt++;
				}	
				if (letter == '(')
				{
					cnt--;
				}
				if (letter == ',')
				{
					if (cnt == 0)
					{
						localOffsets[ndx++] = i;
					}
				}
			}
			offsets = localOffsets;
		}

		private void FindParams(string dataFromInsideParens, out string[] paramArray)
		{
			const int MaxArrayElements = 25;  // arbitray
			int lngth = dataFromInsideParens.Length;
			int i;
			int cnt = 0;
			int ndx = 0;
			char letter;
			string[] localParams = new string[MaxArrayElements];  // artificial limit
			StringBuilder aParam = new StringBuilder(); 

			//start from right
			for (i = lngth - 1; i >= 0; i--)
			{
				letter = dataFromInsideParens[i];
				
				if (letter == ')')
				{
					cnt++;
				}	
				
				if (letter == '(')
				{
					cnt--;
				}
				
				if (letter == ',')
				{
					if (cnt == 0)
					{
						localParams[ndx++] = aParam.ToString();
						aParam.Length = 0;
					}
				}
				else
				{
					aParam.Insert(0, letter);
				}
				
				if (ndx == MaxArrayElements)
				{
					break;
				}
			}

			if ((ndx < MaxArrayElements) && (aParam.Length > 0))
			{
				localParams[ndx] = aParam.ToString();
			}
			paramArray = localParams;
		}

		private bool GetParamsForDelphiCommand(string aCommand, string aLine, out string[] paramsArray,
			out int starts, out int ends)
		{
			// locals
			bool result = false;
			string[] localArray = new string[25]; 
			starts = -1;
			ends = -1;

			starts = aLine.ToLower().IndexOf(aCommand);
			if (starts > -1)
			{
				// find closing paren
				int iStarts = -1;
				FindParenPair(aLine, starts, out iStarts, out ends);
				if ((iStarts + 1 > 0) && (ends - iStarts - 1 > 0)) 
				{
					string z = aLine.Substring(iStarts + 1, ends - iStarts - 1);
					FindParams(z, out localArray);
					result = true;
				}
			}
			paramsArray = localArray;
			return result;
		}

		private void ExtractComments(ref string aLine, ref string comments)
		{
			int pos = aLine.IndexOf("//");
			if (pos > -1)
			{
				comments = aLine.Substring(pos, aLine.Length - pos);
				aLine = aLine.Remove(pos, aLine.Length - pos);
			}
			pos = aLine.IndexOf("/*");
			int pos2 = aLine.IndexOf("*/");
			if ((pos > -1) && (pos2 > -1) && (pos2 > pos)) 
			{
				comments = comments + " " + aLine.Substring(pos, pos2 - pos);
				aLine = aLine.Remove(pos, pos2 - pos);
			}
		}

		private void ExtractComments(ref StringBuilder aLine, ref string comments)
		{
			string tmpLine = aLine.ToString();
			int pos = tmpLine.IndexOf("//");
			if (pos > -1)
			{
				comments = tmpLine.Substring(pos, aLine.Length - pos);
				tmpLine = tmpLine.Remove(pos, tmpLine.Length - pos);
			}
			aLine.Length = 0;
			aLine.Append(tmpLine);

			pos = tmpLine.IndexOf("/*");
			int pos2 = tmpLine.IndexOf("*/");
			if ((pos > -1) && (pos2 > -1) && (pos2 > pos)) 
			{
				comments = comments + " " + tmpLine.Substring(pos, pos2 + 2 - pos);
				comments = comments.Trim();
				tmpLine = tmpLine.Remove(pos, pos2 + 2 - pos);
			}
			aLine.Length = 0;
			aLine.Append(tmpLine);
		}

		private string ConvertMethodParamsForOneVarType(string parenText)
		{
			string y = parenText;
			int j, k;
			string[] paramArray;
			string varType;
			string modifierType;

			if (CountOccurences(y, new char[] {',', ';'}) == 0)
			{
				paramArray = y.Split(':');
				j = paramArray.Length;
				if (j == 2)
				{
					y = paramArray[1] + " " + paramArray[0] + ", ";;
				}
				else if (j == 3)
				{
					y = paramArray[0] + " " + paramArray[2] + " " + paramArray[1] + ", ";;
				}
			}
			else if (CountOccurences(y, new char[] {';'}) == 0)
			{
				paramArray = y.Split(':');
				if (paramArray.Length > 1) 
				{
					varType = paramArray[1];
				}
				else
				{
					varType = "";
				}
				paramArray = paramArray[0].Split(',');
				j = paramArray.Length;
				paramArray[0] = paramArray[0].Trim();
				if (CountOccurences(paramArray[0], new char[] {' '}) == 1)
				{
					modifierType = paramArray[0].Substring(0, paramArray[0].Trim().IndexOf(" "));
					paramArray[0] = paramArray[0].Substring(paramArray[0].IndexOf(" ") + 1, paramArray[0].Length - (paramArray[0].IndexOf(" ") + 1));
				}
				else
					modifierType = "";
				y = "";
				for (k = 0; k < j; k++)
				{
					y = y + modifierType + " " + varType + " " + paramArray[k] + ", ";
				}
			}
			return y;
		}

		private void AddLine(ref StringBuilder aLineObj, string newLine, string comments)
		{
			aLineObj.Length = 0;
			aLineObj.Append(newLine + " " + comments + "\n");
		}

		private bool GetBothSides(string aLine, string breakPhrase, ref string leftSide, ref string rightSide)
		{
			leftSide = "";
			rightSide = "";
			int pos1 = aLine.IndexOf(breakPhrase);
			if (pos1 == -1)
			{
				return false;
			}
			else
			{
				int pos2 = pos1 + breakPhrase.Length;
				leftSide = aLine.Substring(0, pos1);
				rightSide = aLine.Substring(pos2, aLine.Length - pos2);
				return true;
			}
		}

		private bool ConvertPos(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string y;
			string result;
			if (GetParamsForDelphiCommand("pos(", aLine, out paramArray, out starts, out ends))
			{
				x = paramArray[0];
				y = paramArray[1];
				result = aLine.Substring(0, starts) + " " + x + ".IndexOf(" + y + ") ";
				result = result + aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

/*
 * NOT WORKING RIGHT YET, SO COMMENTED OUT.
 		private string DoConvertStringReplace(string InString, string OfString, string WithString, bool CaseSensitive)
		{
			StringBuilder CSReplaceTemplate = new StringBuilder(CSharpStringReplaceTemplate);
			CSReplaceTemplate = CSReplaceTemplate.Replace("InString", InString);
			CSReplaceTemplate = CSReplaceTemplate.Replace("OfString", OfString);
			CSReplaceTemplate = CSReplaceTemplate.Replace("WithString", WithString);
			if (!CaseSensitive) 
			  CSReplaceTemplate = CSReplaceTemplate.Replace(", CompareOptions.Ignore Case", " ");
			return CSReplaceTemplate.ToString();
		}

		private bool ConvertStringReplace(ref string aLine)
		{
			string[] outParams;
			int starts;
			int ends;
			if (GetParamsForDelphiCommand("stringreplace", aLine, out outParams, out starts, out ends))
			{
				if (outParams.Length < 3) return false;
				string InString = outParams[0];
				string OfWhat = outParams[1];
				string WithWhat = outParams[2];
				bool ReplaceAll = aLine.ToLower().IndexOf("rfreplaceall") > 0;
				bool CaseSensitive = aLine.ToLower().IndexOf("rfcaseinsensitive") > 0;
				if (!ReplaceAll)
				{
					int foundStart;
					if (CaseSensitive) 
						foundStart = aLine.IndexOf(OfWhat);
					else
						foundStart = aLine.ToLower().IndexOf(OfWhat);
					if (foundStart > 0)
					{
						aLine = aLine.Remove(foundStart, OfWhat.Length);
						aLine = aLine.Insert(foundStart, WithWhat);
						return true;
					}
					return false;
				}
				
				string result = DoConvertStringReplace(InString, OfWhat, WithWhat, CaseSensitive);
				if (CaseSensitive) 
				{
					result = result.Replace("CompareOptions", IgnoreCase);
				}
				else
				{
					result = result.Replace("CompareOptions", " ");
				}
				aLine = result;
				return true;
			}
			return false;
		}

*/
		private bool ConvertCopy(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string y;
			string z;
			string result;
			if (GetParamsForDelphiCommand("copy(", aLine, out paramArray, out starts, out ends))
			{
				x = paramArray[2];
				y = paramArray[1];
				z = paramArray[0];

				result = aLine.Substring(0, starts) + " " + x + ".Substring(" + y + ", " + z + ")";
				result = result + aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool ConvertLength(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string result;
			if (GetParamsForDelphiCommand("length(", aLine.ToString(), out paramArray, out starts, out ends))
			{
				x = paramArray[0];
				result = aLine.Substring(0, Math.Max(0, starts)) + " " + x + ".Length ";
				if (aLine.Length > ends)
					result = result + aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool ConvertInc(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string result;
			if (GetParamsForDelphiCommand("inc(", aLine.ToString(), out paramArray, out starts, out ends))
			{
				// get inside-paren text
				x = paramArray[0];
				result = aLine.Substring(0, Math.Max(0, starts)) + " " + x + "++";
				if (aLine.Length > ends)
					result = result +	aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool ConvertDec(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string result;
			if (GetParamsForDelphiCommand("dec(", aLine.ToString(), out paramArray, out starts, out ends))
			{
				// get inside-paren text
				x = paramArray[0];
				result = aLine.Substring(0, Math.Max(0, starts)) + " " + x + "--";
				if (aLine.Length > ends)
					result = result +	aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool ConvertIntToStr(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string result;
			if (GetParamsForDelphiCommand("inttostr(", aLine.ToString(), out paramArray, out starts, out ends))
			{
				x = paramArray[0];
				result = aLine.Substring(0, Math.Max(0, starts)) + " " + x + ".ToString()";
				if (aLine.Length > ends)
					result = result + aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

		
		private bool ConvertLowercase(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string result;
			if (GetParamsForDelphiCommand("lowercase(", aLine.ToString(), out paramArray, out starts, out ends))
			{
				x = paramArray[0];
				result = aLine.Substring(0, Math.Max(0, starts)) + " " + x + ".ToLower()";
				if (aLine.Length > ends)
					result = result + aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}


		private bool ConvertUppercase(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string result;
			string x;
			if (GetParamsForDelphiCommand("uppercase(", aLine.ToString(), out paramArray, out starts, out ends))
			{
				x = paramArray[0];
				result = aLine.Substring(0, Math.Max(0, starts)) + " " + x + ".ToUpper()";
				if (aLine.Length > ends)
					result = result + aLine.Substring(ends + 1, aLine.Length - ends - 1);
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}


	/*
		private bool Convert(ref string aLine)
		{
			string[] paramArray;
			int starts;
			int ends;
			string x;
			string y;
			string z;
			string result;
			if (GetParamsForDelphiCommand("", aLine.ToString(), out paramArray, out starts, out ends))
				aLine = result;
				return true;
			}
			else
			{
				return false;
			}
		}

*/		

		
		private void btnConvert_Click(object sender, System.EventArgs e)
		{
			int i;
			int lineNbr = 0;
			StringBuilder aLine = new StringBuilder();
			string[] words = new string[] {};
			string w;
			string x;
			string y;
			string z;
			int cnt;
			int nbrWords;
			int starts;
			int ends = 0;
			int pos;
			int pos2;
			int j;
			int k;
			bool outcome;
			bool HasConstAndVarSection;
			bool NeedsClosingBracket;
			string returnType = "";
			string methodName = "";
			string leftSide = "";
			string rightSide = "";
			string aDirection;
			string propertyName;
			string propertyType;
			string propertyRead;
			string propertyWrite;
			string[] paramArray;
			char[] allChars;
			string trailingComment = "";
			string allText = tb1.Text.ToLower();
			string ConstAndVarSection;
			allText = DeleteExtraSpaces(allText);
			
			tb1.Text = ChangeNonBracketedSyntax(tb1.Text);
			tb1.Text = tb1.Text.Trim();
			tb1.Lines = MakeLinesWhole(tb1.Lines);
			tb1.Text = tb1.Text.Trim();
			

			HasConstAndVarSection = false;
			NeedsClosingBracket = false;
			ConstAndVarSection = "";
			int textLines = tb1.Lines.GetUpperBound(0);
			string[] newCode = new string[textLines + 1];
			for (i = 0; i <= textLines; i++)
			{
				//aLine = tb1.Lines[i];
				label1.Text = "line " + i.ToString() + ":";
				Application.DoEvents();
				
				//  set vars
				aLine.Length = 0;
				aLine.Append(tb1.Lines[i]);
				trailingComment = "";
				ExtractComments(ref aLine, ref trailingComment); 
				label2.Text = aLine.ToString();

	
				//  Convert procedure
				x = aLine.ToString().TrimStart().ToLower();
				pos = x.IndexOf("procedure ");
				if (pos > -1)
				{
					if (HasConstAndVarSection) 
						AddLine(ref aLine, "}\r\n", "");

					ConstAndVarSection = "";
					HasConstAndVarSection = false;
					
					//  Get params
					x = aLine.ToString();
					pos = x.IndexOf("(");
					pos2 = x.IndexOf(")");
					if ((pos > -1) & (pos2 > -1))
					{
						y = x.Substring(pos + 1, pos2 - pos - 1);
				
						//  Convert params
						paramArray = y.Split(new char[] {';', ')'});
						j = paramArray.Length;
						z = "";
						for (k = 0; k < j; k++)
						{
							z = z + ConvertMethodParamsForOneVarType(paramArray[k]);
						}
						z = z.TrimEnd();
						if (z.EndsWith(","))
						{
							z = z.Substring(0, z.Length - 1);
						}
					
						//  Get method name
						methodName = x.Substring(x.IndexOf("procedure ") + 10, x.IndexOf("(") - x.IndexOf("procedure ") - 10);
					
					}
					else
					{
						z = "";
						pos = x.IndexOf(" ");
						pos2 = x.IndexOf(";");
						//  Get method name
						methodName = x.Substring(pos, pos2 - pos); 
					}

					//  strip class qualifier from method name
					if (methodName.IndexOf(".") > -1)
					{
						paramArray = methodName.Split('.');
						methodName = paramArray[1];
					}

					//  build the C# method
					x =  x.Substring(0, x.IndexOf("procedure ")) + "public void " + methodName + "(" + z + ")";
					x = x.Replace("var ", "ref ");
					x = DeleteExtraSpaces(x);
					if (NeedsClosingBracket) 
					{
						x = "}\r\n\r\n" + x;
						NeedsClosingBracket = false;
					}
					AddLine(ref aLine, x.TrimStart(), trailingComment);
				}


				//  Convert function
				x = aLine.ToString().TrimStart().ToLower();
				pos = x.IndexOf("function ");
				if (pos > -1)
				{
					if (HasConstAndVarSection) 
						AddLine(ref aLine, "}\r\n", "");

					ConstAndVarSection = "";
					HasConstAndVarSection = false;

					x = aLine.ToString();

					//  Get params
					pos = x.IndexOf("(");
					pos2 = x.LastIndexOf(")");
					if ((pos > -1) & (pos2 > -1))
					{
						y = x.Substring(pos + 1, pos2 - pos - 1);
				
						//  Get function return type
						pos = x.IndexOf(":", pos2);
						returnType = x.Substring(pos + 1, x.Length - (pos + 1) - 1);

						//  Convert params
						paramArray = y.Split(new char[] {';', ')'});
						j = paramArray.Length;
						z = "";
						for (k = 0; k < j; k++)
						{
							z = z + ConvertMethodParamsForOneVarType(paramArray[k]);
						}
						z = z.TrimEnd();
						if (z.EndsWith(","))
						{
							z = z.Substring(0, z.Length - 1);
						}

						//  Get method name
						methodName = x.Substring(x.IndexOf("function ") + 9, x.IndexOf("(") - x.IndexOf("function ") - 9).TrimStart();
					}
					else
					{
						z = "";
						pos = x.IndexOf(" ");
						pos2 = x.IndexOf(":");
						//  Get method name
						methodName = x.Substring(pos, pos2 - pos); 
					}
					
					//  strip class qualifier from method name
					if (methodName.IndexOf(".") > -1)
					{
						paramArray = methodName.Split('.');
						methodName = paramArray[1];
					}

					//  build the C# method
					x =  x.Substring(0, x.IndexOf("function ")) + "public " + returnType + " " + methodName + "(" + z + ")";
					x = x.Replace("var ", "ref ");
					//  Remove double spaces
					x = DeleteExtraSpaces(x);
					if (NeedsClosingBracket) 
					{
						x = "}\r\n\r\n" + x;
						NeedsClosingBracket = false;
					}
					AddLine(ref aLine, x.TrimStart(), trailingComment);
				}


				//  Convert vars
				x = aLine.ToString().TrimStart().ToLower();
				if (x.StartsWith("var"))
				{
					y = x.Substring(4, x.Length - 4).Trim();
					paramArray = y.Split(new char[] {';', ')'});
					j = paramArray.Length;
					z = "";
					for (k = 0; k < j; k++)
					{
						z = z + ConvertMethodParamsForOneVarType(paramArray[k]);
					}
					z = z.TrimEnd();
					//  Convert commas to semicolons
					if (HasConstAndVarSection)
					{
						z = z.Replace(",", ";\n"); 
					}
					else
					{
						z = "{\n" + z.Replace(",", ";\n"); 
					}
					
					HasConstAndVarSection = true;
					ConstAndVarSection = ConstAndVarSection + z + "\r\n";
				}
				

				//  Convert const
				x = aLine.ToString().TrimStart().ToLower();
				if (x.StartsWith("const"))
				{
					y = x.Substring(4, x.Length - 4).Trim();
					paramArray = y.Split(new char[] {';', ')'});
					j = paramArray.Length;
					z = "";
					for (k = 0; k < j; k++)
					{
						z = z + ConvertMethodParamsForOneVarType(paramArray[k]);
					}
					z = z.TrimEnd();
					//  Convert commas to semicolons
					if (!HasConstAndVarSection)
					{
						z = "{\n" + z; 
					}
					
					HasConstAndVarSection = true;
					ConstAndVarSection = ConstAndVarSection + z + "\r\n";
				}
				if (HasConstAndVarSection)
				{
					AddLine(ref aLine, ConstAndVarSection, trailingComment);
					HasConstAndVarSection = false;
					NeedsClosingBracket = true;
				}

				//  look for Initialization
				x = aLine.ToString().TrimStart().ToLower();
				if (x.StartsWith("initialization"))
				{
					if (NeedsClosingBracket) 
					{
						x = "}\r\n\r\n";
						NeedsClosingBracket = false;
						AddLine(ref aLine, x, trailingComment);
						//  add it to new code array
						newCode[lineNbr++] = aLine.ToString();
					}
					continue;
				}

				//  look for Finalization
				x = aLine.ToString().TrimStart().ToLower();
				if (x.StartsWith("finalization"))
				{
					if (NeedsClosingBracket) 
					{
						x = "}\r\n\r\n";
						NeedsClosingBracket = false;
						AddLine(ref aLine, x, trailingComment);
						//  add it to new code array
						newCode[lineNbr++] = aLine.ToString();
					}
					continue;
				}

				//  Convert integer comparison
				x = aLine.ToString();
				x = x.Replace(" In ", " in ");
				x = x.Replace(" IN ", " in ");
				x = DeleteExtraSpaces(x);
				pos = x.IndexOf(" in ['0'..'9']");
				if (pos > -1)
				{
					//  Find previous word
					y = "";
					for (j = pos - 1; j >= 0; j--)
					{
						if ((x[j] != ' ') && (x[j] != '('))
						{
							y = x[j] + y;	
						}
						else
						{
							break;
						}
					}
					GetBothSides(aLine.ToString(), "['0'..'9']", ref leftSide, ref rightSide);
					leftSide = aLine.ToString().Substring(0, aLine.ToString().IndexOf(y));   //
					x = leftSide + "Char.IsNumber(" + y + ")" + rightSide;
					AddLine(ref aLine, x, trailingComment);
				}

				x = aLine.ToString();
				//  Convert POS
				while (ConvertPos(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}

				
				//  Convert COPY
				while (ConvertCopy(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert LENGTH
				while (ConvertLength(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert INC
				while (ConvertInc(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert DEC
				while (ConvertDec(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert IntToStr
				while (ConvertIntToStr(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert LOWERCASE
				while (ConvertLowercase(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert UPPERCASE
				while (ConvertUppercase(ref x))
				{
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert TRIM
				if (GetParamsForDelphiCommand("trim(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[0];
					x = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + x + ".Trim()";
					if (aLine.Length > ends)
						x = x +	aLine.ToString().Substring(ends + 1, aLine.ToString().Length - ends - 1);
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert LOW
				if (GetParamsForDelphiCommand("low(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[0];
					x = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + x + ".GetLowerBound(0) " +
						aLine.ToString().Substring(ends + 1, aLine.ToString().Length - ends - 1);
					AddLine(ref aLine, x, trailingComment);
				}


				// Convert HIGH
				if (GetParamsForDelphiCommand("high(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[0];
					x = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + x + ".GetUpperBound(0) " +
						aLine.ToString().Substring(ends + 1, aLine.ToString().Length - ends - 1);
					AddLine(ref aLine, x, trailingComment);
				}

				//  Convert PRED
				if (GetParamsForDelphiCommand("pred(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[0];
					x = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + x + " - 1" +
						aLine.ToString().Substring(ends + 1, aLine.ToString().Length - ends - 1);
					AddLine(ref aLine, x, trailingComment);
				}
			
				//  Convert SUCC
				if (GetParamsForDelphiCommand("succ(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[0];
					x = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + x + " + 1" +
						aLine.ToString().Substring(ends + 1, aLine.ToString().Length - ends - 1);
					AddLine(ref aLine, x, trailingComment);
				}
			
								
/*                //  Convert StringReplace
				x = aLine.ToString();
				while (ConvertStringReplace(ref x)) 
				{
					AddLine(ref aLine, x, trailingComment);
				}
*/
				//  Convert String.Delete
				if (GetParamsForDelphiCommand("delete(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[2];
					y = paramArray[1];
					z = paramArray[0];
					w = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + x + ".Remove(" + y + ", " + z + ") " +
						aLine.ToString().Substring(ends, aLine.ToString().Length - ends);
					AddLine(ref aLine, w, trailingComment);
				}


				//  Convert String.Insert
				if (GetParamsForDelphiCommand("insert(", aLine.ToString(), out paramArray, out starts, out ends))
				{
					x = paramArray[2];
					y = paramArray[1];
					z = paramArray[0];
					w = aLine.ToString().Substring(0, Math.Max(0, starts)) + " " + y + ".Insert(" + z + ", " + x + ") " +
						aLine.ToString().Substring(ends, aLine.ToString().Length - ends);
					AddLine(ref aLine, w, trailingComment);
				}


				//  Convert .Create w/no params;
				pos = aLine.ToString().ToLower().IndexOf(".create;");
				if (pos > -1)
				{
					for (pos2 = pos; pos2 > 0; pos2--)
					{
						allChars = aLine.ToString().ToCharArray();
						if (allChars[pos2] == ' ')
						{
							break;
						}
					}
					x = aLine.ToString().Substring(0, pos2) + " new " + aLine.ToString().Substring(pos2, pos - pos2);
					if (!(x.TrimEnd().EndsWith(";")))
					{
						x = x + ";";
					}
					AddLine(ref aLine, x, trailingComment);
				}


				//  Drop .Free;
				pos = aLine.ToString().ToLower().IndexOf(".free;");
				if (pos > -1)
				{
					aLine.Length = 0;
				}


				//  Convert .Create w/no params;
				pos = aLine.ToString().ToLower().IndexOf(".create;");
				if (pos > -1)
				{
					for (pos2 = pos; pos2 > 0; pos2--)
					{
						allChars = aLine.ToString().ToCharArray();
						if (allChars[pos2] == ' ')
						{
							break;
						}
					}
					x = aLine.ToString().Substring(0, pos2) + " new " + aLine.ToString().Substring(pos2, pos - pos2);
					AddLine(ref aLine, x, trailingComment);
				}


				//  Convert ''
				x = aLine.ToString();
				while (x.IndexOf(@"''") > -1)
				{
					x = x.Replace(@"''", "\"\"");
					AddLine(ref aLine, x, trailingComment);
				}


				//  Convert 'strings like this' to "strings like this"
				x = aLine.ToString();
				pos = x.IndexOf(@"'");
				pos2 = x.IndexOf(@"'", pos + 1);
				if ((pos > -1) & (pos2 > -1) & ((pos2 - pos) > 1))
				{
					x = x.Remove(pos, 1);
					x = x.Insert(pos, "\"");
					x = x.Remove(pos2, 1);
					x = x.Insert(pos2, "\"");
					AddLine(ref aLine, x, trailingComment);
				}


				//  Convert properties
				outcome = aLine.ToString().TrimStart().ToLower().StartsWith("property ");
				if (outcome == true)
				{
					// strip punctuation
					x = aLine.ToString().Trim().Replace(':', ' ');
					x = x.Replace(';', ' ');
					while (x.IndexOf("  ") > -1)
					  x = DeleteExtraSpaces(x);
					
					//  put words into string array
					paramArray = x.Split();
					
					// get property attributes
					propertyWrite = "";
					propertyRead = "";
					propertyName = "";
					propertyType = "";
					if (paramArray.Length > 1 ) propertyName = paramArray[1];
					if (paramArray.Length > 2 ) propertyType = paramArray[2];
					if (paramArray.Length > 3 ) 
					{
						if (paramArray[3] == "read")
						{
							propertyRead = "F" + paramArray[4].Trim();
						}
						else
						{
							propertyRead = "";
						};
					};
					
					if (paramArray.Length > 4 )
					{
						if (paramArray[3] == "write")
						{
							propertyWrite = paramArray[4];
						};
					};	
					
					if (paramArray.Length > 5 ) 
					{
						if (paramArray[5] == "write")
						{
							propertyWrite = "F" + paramArray[6].Trim();
						}
						else 
						{
							propertyWrite = "";
						};
					};

					//  Get write code
					if (propertyWrite.Length > 0)
					{
						pos = allText.LastIndexOf(propertyWrite.ToLower() + "(");
						if (pos > -1)
						{
							pos = allText.IndexOf("begin", pos) + 5;
							pos2 = allText.IndexOf("end;", pos);
							propertyWrite = allText.Substring(pos, pos2 - pos);
						}
						else
						{
							propertyWrite = "F" + propertyWrite.Substring(4, propertyWrite.Length - 4) + " := value;";
						};
					};

					//  Get read code
					if (propertyRead.Length > 0)
					{
						if (propertyRead.TrimStart().ToUpper().StartsWith("F"))
						{
							propertyRead = "return F" + propertyName;
						}
						else
						{
							pos = allText.IndexOf("read " + propertyRead.ToLower());
							pos2 = allText.LastIndexOf(propertyRead);
							if (pos2 > pos + 6)
							{
								pos = allText.LastIndexOf(propertyRead.ToLower());
								pos = allText.IndexOf("{", pos);
								pos2 = allText.IndexOf("}", pos);
								propertyRead = allText.Substring(pos, pos2 - pos);
							};
						};
					};

					x = "public " + propertyType + " " + propertyName + "{\n" + "    get\n{\n" + propertyRead + "\n}\n" + "    set\n{\n" + propertyWrite + "}\n}\n";
					AddLine(ref aLine, x, trailingComment);
				};

				//  Convert FOR loops
				x = aLine.ToString().TrimStart().ToLower();
				pos = x.IndexOf("for ");
				if (pos > -1)
				{
					w = aLine.ToString().Trim();
					x = DeleteExtraSpaces(x);
					while (w.IndexOf(") )") > -1)
					{
						w = w.Replace(") )", "))");
					}
					while (w.IndexOf("( (") > -1)
					{
						w = w.Replace("( (", "((");
					}
					words = w.Split();  // defaults to any white space char as delimiter
					nbrWords = words.GetUpperBound(0);
				
					if ( (words[0].ToLower() == "for") & (nbrWords >= 6 ) ) 
					{
						if (aLine.ToString().ToLower().IndexOf(" to ") > -1) 
							aDirection = "to";
						else
							aDirection = "downto";
						cnt = 1;
						x = "";
						y = "";
						z = "";
						while ((cnt <= nbrWords) && (words[cnt] != ":="))
						{
							x = x + " " + words[cnt];
							cnt++;
						}

						cnt++;
						while ((cnt <= nbrWords) && (words[cnt] != aDirection))
						{
							y = y + " " + words[cnt];
							cnt++;
						}

						cnt++;
						while ((cnt <= nbrWords) && (words[cnt] != "do"))
						{
							z = z + " " + words[cnt];
							cnt++;
						}

						string Incrementor;
						if (aDirection == "to") 
						{
							aDirection = " <= ";
							Incrementor = "++";
						}
						else
						{	
							aDirection = " >= ";
							Incrementor = "--";
						}
						//  assemble for loop
						x = "for (" + x + " = " + y + "; " + x + aDirection + z + "; " + x + Incrementor + ")" + "\n";
						//  add back any trailing text
						pos = aLine.ToString().ToLower().IndexOf(" do ");
						if (pos > -1)
						{
							rightSide = aLine.ToString().Substring(pos + 4, aLine.ToString().Length - pos - 4);
							x = x + " " + rightSide;
						}

						AddLine(ref aLine, x, trailingComment);
					}
				}

				
				//  Convert =
				x = aLine.ToString();
				pos = x.IndexOf(" = ");
				if (pos > -1)
				{
					if (x.IndexOf("const") == -1)
					{
						x = x.Replace(" = ", " == ");
						AddLine(ref aLine, x, trailingComment);
					}
				}


				//  Convert :=
				x = aLine.ToString();
				pos = x.IndexOf(":=");
				if (pos > -1)
				{
					x = x.Replace(":=", "=");
					AddLine(ref aLine, x, trailingComment);
				}



				
				//  add it to new code array
				newCode[lineNbr++] = aLine.ToString();
			}
			
			tb1.Lines = newCode;
			tb1.Text = tb1.Text.Trim();
			StringBuilder newCode2 = new StringBuilder();
			
			for (i=tb1.Lines.Length;i>0;i--)
			{
				label1.Text = "line " + i.ToString() + ":";
				Application.DoEvents();
				
				if (tb1.Lines[i-1].TrimStart() != "")
				{
					newCode2.Insert(0, tb1.Lines[i-1] + "\n");
				}
				else
				{
					continue;
				}

				Regex replacer = new Regex("\bend\n", RegexOptions.IgnoreCase);
				replacer.Replace(tb1.Text, "}");

				if (tb1.Lines[i-1].TrimStart().StartsWith("public "))
				{
					newCode2.Insert(0, "\n//--------------------------------------------\n");
				}
				if (tb1.Lines[i-1].TrimStart().StartsWith("private "))
				{
					newCode2.Insert(0, "\n//--------------------------------------------\n");
				}
			}
			tb1.Text = newCode2.ToString();
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			label1.Text = "";
			label2.Text = "Cut and paste Delphi unit into the text box, then Convert.";
		}
		
/*???*/

		
		
	}
}
