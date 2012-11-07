using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCXCODE
{
    public enum TokenType
    {
        String_Literal,
        Char_Literal,
        Int_Literal,
        Double_Literal,
        Bool_Literal,
        Byte_Literal,
        NewLine,
        TAB,
        Word,
        Text,
        NWord,
        Symbol,
        Base
    }

    public class Token
    {
        public object val;
        public TokenType type;

        public override string ToString()
        {
            return type.ToString() + " - " + val.ToString();
        }
    }

    class Scanner
    {
        string code;
        List<Token> tokens;

        int i;
        bool isEscape;

        char c
        {
            get
            {
                return code[i];
            }
        }

        public Scanner(string code)
        {
            this.code = code;
            tokens = new List<Token>();
        }

        public void Scan()
        {
            isEscape = false;
            for (i = 0; i < code.Length; i++)
            {
                if (c == '\n')
                {
                    tokens.Add(new Token() { type = TokenType.NewLine, val = '\n' });
                }
                else if (c == '#')
                {
                    i++;
                    while (c != '#')
                    {
                        i++;
                    }
                }
                else if (c == '\\')
                {
                    isEscape = true;
                }
                else if (c == '{')
                {
                    StringBuilder sb = new StringBuilder();
                    i++;
                    while (c != '}')
                    {
                        sb.Append(c);
                        i++;
                    }
                    tokens.Add(new Token() { type = TokenType.Base, val = sb.ToString() });
                }
                else if (c == '\r')
                {
                }
                else if (c == '\t')
                {
                }
                else if (c == '"')
                {
                    StringBuilder b = new StringBuilder();
                    i++;
                    while (i < code.Length && c != '"')
                    {
                        if (c == '\\')
                        {
                            i++;
                        }
                        b.Append(c);
                        i++;
                    }
                    tokens.Add(new Token() { type = TokenType.String_Literal, val = b.ToString() });
                }
                else if (c == '\'')
                {
                    i++;
                    tokens.Add(new Token() { type = TokenType.Char_Literal, val = c });
                    i++;
                }
                else if (char.IsLetter(c))
                {
                    StringBuilder b = new StringBuilder("");
                    b.Append(c);
                    i++;
                    while (i < code.Length && (char.IsLetterOrDigit(c) || c == '.'))
                    {
                        b.Append(c);
                        i++;
                    }
                    i--;

                    if (b.ToString() == "true")
                    {
                        tokens.Add(new Token() { type = TokenType.Bool_Literal, val = true });
                    }
                    else if (b.ToString() == "false")
                    {
                        tokens.Add(new Token() {type = TokenType.Bool_Literal, val = false });
                    }
                    else
                    {
                        tokens.Add(new Token() { type = TokenType.Word, val = b.ToString() });
                    }
                }
                else if (char.IsDigit(c))
                {
                    StringBuilder b = new StringBuilder();
                    while (i < code.Length && char.IsDigit(c))
                    {
                        b.Append(c);
                        i++;
                    }
                    if (i < code.Length && c == '.')
                    {
                        i++;
                        b.Append(',');
                        while (i < code.Length && char.IsDigit(c))
                        {
                            b.Append(c);
                            i++;
                        }
                        i--;
                        tokens.Add(new Token() { type = TokenType.Double_Literal, val = Convert.ToDouble(b.ToString()) });
                    }
                    else
                    {
                        i--;
                        tokens.Add(new Token() { type = TokenType.Int_Literal, val = Convert.ToInt32(b.ToString()) });
                    }
                }
                else if (char.IsWhiteSpace(c))
                {
                    
                }
                else if (c == '-')
                {
                    if (i < code.Length + 1 && char.IsDigit(code[i + 1]))
                    {
                        i++;
                        StringBuilder b = new StringBuilder();
                        while (i < code.Length && char.IsDigit(c))
                        {
                            b.Append(c);
                            i++;
                        }
                        if (i < code.Length && c == '.')
                        {
                            i++;
                            b.Append(',');
                            while (i < code.Length && char.IsDigit(c))
                            {
                                b.Append(c);
                                i++;
                            }
                            i--;
                            tokens.Add(new Token() { type = TokenType.Double_Literal, val = (0 - Convert.ToDouble(b.ToString())) });
                        }
                        else
                        {
                            i--;
                            tokens.Add(new Token() { type = TokenType.Int_Literal, val = (0 - Convert.ToInt32(b.ToString())) });
                        }
                    }
                    else
                    {
                        tokens.Add(new Token() { type = TokenType.Symbol, val = c });
                    }
                }
                else
                {
                    tokens.Add(new Token() { type = TokenType.Symbol, val = c });
                }
            }
        }

        public IEnumerable<Token> Tokens { get { return tokens; } }
    }
}
