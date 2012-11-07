using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCXCODE
{
    public class Parser
    {
        Token[] tokens;
        int i;

        internal static int labelcounter;

        List<ISTMT> result;

        Token Current
        {
            get { return tokens[i]; }
        }

        bool isfunction;
        bool isClass;
        DefClass cref;
        List<DefineFunc> cfs;
        List<DefVar> cvs;

        internal ISTMT[] Result
        {
            get
            {
                return result.ToArray();
            }
        }

        public Parser(Token[] tokens)
        {
            this.tokens = tokens;
        }

        public void Parse()
        {
            result = new List<ISTMT>();
            for (i = 0; i < tokens.Length; )
            {
                ISTMT s = ParseStmt();
                if (s != null)
                {
                    result.Add(s);
                }
            }
        }

        private ISTMT ParseStmt()
        {
            try
            {
                if (i + 1 < tokens.Length && tokens[i + 1].val.ToString() == "(")
                {
                    if (isfunction)
                    {
                        string name = Current.val.ToString();
                        i++;
                        i++;
                        FuncCall f = new FuncCall() { method = name, args = ParseArgs() };
                        i++;
                        return f;
                    }
                    else
                    {
                        string name = Current.val.ToString();
                        i++;
                        i++;
                        DefineFunc f = new DefineFunc() { name = name, Args = ParseFuncArgs() };
                        isfunction = true;
                        i++;
                        f.body = ParseBody();
                        isfunction = false;
                        if (isClass)
                        {
                            cfs.Add(f);
                        }
                        return f;
                    }
                }
                else if (i < tokens.Length && Current.type == TokenType.Base)
                {
                    BaseCode bc = new BaseCode() { val = Current.val.ToString() };
                    i++;
                    return bc;
                }
                else if (i < tokens.Length && Current.val.ToString() == "open")
                {
                    i++;
                    Open o = new Open() { name = Current.val.ToString() };
                    i++;
                    return o;
                }
                else if (i + 1 < tokens.Length && Current.val.ToString() == "xmlgen")
                {
                    i++;
                    string XML = Current.val.ToString();
                    Xml x = new Xml() { text = XML };
                    i++;
                    return x;
                }
                else if (i + 1 < tokens.Length && tokens[i + 1].val.ToString() == "=")
                {
                    string name = Current.val.ToString();
                    i++;
                    i++;
                    SetVar s = new SetVar() { name = name, value = ParseExpr(false) };
                    i++;
                    return s;
                }
                else if (i + 1 < tokens.Length && Current.val.ToString() == "if")
                {
                    If f = new If();
                    i++;
                    f.eval = ParseExpr(false);
                    i++;
                    f.truebody = ParseBody();
                    i--;
                    if (Current.val.ToString() == "else")
                    {
                        i++;
                        f.falsebody = ParseBody();
                    }
                    else
                    {
                        i++;
                    }
                    return f;
                }
                else if (i < tokens.Length && Current.val.ToString() == "from")
                {
                    i++;
                    string inc = Current.val.ToString();
                    i++; //name
                    i++; //,
                    IEXPR to = ParseExpr(false);
                    i++; //expr
                    i++; //,
                    IEXPR by = ParseExpr(false);
                    i++; //by

                    ISTMT[] body = ParseBody();
                    Loop l = new Loop() { from = inc, to = to, by = by, body = body };
                    return l;
                }
                else if (i < tokens.Length && Current.val.ToString() == "while")
                {
                    i++;
                    IEXPR expr = ParseExpr(false);
                    i++;
                    ISTMT[] body = ParseBody();
                    While w = new While() { expr = expr, body = body };
                    return w;
                }
                else if (i < tokens.Length && Current.val.ToString() == "return")
                {
                    i++;
                    IEXPR v = ParseExpr(false);
                    i++;
                    Return r = new Return() { value = v };
                    return r;
                }
                else if (i < tokens.Length && Current.val.ToString() == "class")
                {
                    i++;
                    string name = Current.val.ToString();
                    i++;
                    DefClass d = new DefClass();
                    d.Name = name;
                    isClass = true;
                    cref = d;
                    cfs = new List<DefineFunc>();
                    cvs = new List<DefVar>();
                    ParseBody();
                    d.funcs = cfs.ToArray();
                    d.vars = cvs.ToArray();
                    isClass = false;
                    return d;
                }
                else if (i < tokens.Length && Current.val.ToString() == "public")
                {
                    i++;
                    string name = Current.val.ToString();
                    i++;
                    DefVar v = new DefVar() { access = "public", name = name };
                    if (isClass)
                    {
                        cvs.Add(v);
                    }
                    return v;
                }
                else if (i < tokens.Length && Current.val.ToString() == "private")
                {
                    i++;
                    string name = Current.val.ToString();
                    i++;
                    DefVar v = new DefVar() { access = "private", name = name };
                    if (isClass)
                    {
                        cvs.Add(v);
                    }
                    return v;
                }
                else if (i < tokens.Length && Current.val.ToString() == "\n")
                {
                    i++;
                    return null;
                }
                else if (i < tokens.Length && Current.val.ToString() == "end")
                {
                    i++;
                    return new End();
                }
                else if (i < tokens.Length && Current.val.ToString() == "else")
                {
                    i++;
                    return new Else();
                }
                else
                {
                    throw new Exception("Unkown token");
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Unexpected end of file","Error");
                return null;
            }
        }

        private ISTMT[] ParseBody()
        {
            List<ISTMT> result = new List<ISTMT>();
            ISTMT s;
            while (!((s = ParseStmt()) is End) && (!(s is Else)))
            {
                if (s != null)
                {
                    result.Add(s);
                }
            }
            return result.ToArray();
        }

        private string[] ParseFuncArgs()
        {
            List<string> r = new List<string>();
            while (Current.val.ToString() != ")")
            {
                r.Add(Current.val.ToString());
                i++; //string
                if (Current.val.ToString() == ")")
                {
                    break;
                }
                else
                {
                    i++; //,
                }
            }
            return r.ToArray();
        }

        private IEXPR[] ParseArgs()
        {
            List<IEXPR> el = new List<IEXPR>();
            while ((Current.val.ToString()) != ")")
            {
                IEXPR e = ParseExpr(false); 
                if (e != null)
                {
                    el.Add(e);
                }
                i++;
                if ((Current.val.ToString()) == ")")
                {
                    break;
                }
                else
                {
                    i++; //,
                }
            }
            return el.ToArray();
            
        }

        private IEXPR ParseExpr(bool isArith)
        {
            IEXPR r = null;
            if (Current.type == TokenType.String_Literal)
            {
                string val = Current.val.ToString();
                StringLit s = new StringLit() { val = val };
                r = s;
            }
            else if (Current.type == TokenType.Int_Literal)
            {
                int val = (int)Current.val;
                IntLit i = new IntLit() { val = val };
                 r = i;

            }
            else if (Current.type == TokenType.Double_Literal)
            {
                double val = (double)Current.val;
                DoubleLit d = new DoubleLit() { val = val };
                 r = d;
            }
            else if (Current.type == TokenType.Word && tokens[this.i + 1].val.ToString() == "(")
            {
                 string name = Current.val.ToString();
                 i++;
                 i++;
                 FuncCall f = new FuncCall() { method = name, args = ParseArgs() };
                 r = f;
            }
            else if (Current.val.ToString() == "new")
            {
                i++;
                New n = new New() { from = Current.val.ToString() };
                r = n;
            }
            else if (Current.type == TokenType.Bool_Literal)
            {
                BoolLit b = new BoolLit() { val = (bool)Current.val };
                r = b;
            }
            else if (Current.type == TokenType.Word)
            {
                string name = Current.val as string;
                LoadVar v = new LoadVar() { name = name };
                r = v;
            }
            

            if ((this.i + 1 < tokens.Length) && (isArith == false) && (tokens[this.i + 1].val.ToString() == "+" || tokens[this.i + 1].val.ToString() == "-" || tokens[this.i + 1].val.ToString() == "*" || tokens[this.i + 1].val.ToString() == "/" || tokens[this.i + 1].val.ToString() == "%" || tokens[this.i + 1].val.ToString() == "<" || tokens[this.i + 1].val.ToString() == "=" || tokens[this.i + 1].val.ToString() == ">" || Current.val.ToString() == "("))
            {
                r = ParseAtrithExpr(r);
                i++;
                return r;
            }
            else
            {
                return r;
            }

            throw new Exception();
        }

        private bool IsWord(string p)
        {
            bool t = true;
            foreach (char c in p)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    t = false;
                }
            }
            return t;
        }

        IEXPR ParseAtrithExpr(IEXPR exp)
        {
            Queue<IEXPR> que = new Queue<IEXPR>();
            Stack<IEXPR> ops = new Stack<IEXPR>();
            bool isop = true;
            int sub = 0;

            if (exp != null)
            {
                que.Enqueue(exp);
                i++;
            }

            if (Current.val.ToString() == "(")
            {
                i++;
            }

            while (true)
            {
                if (Current.val.ToString() == "+")
                {
                    ArithPushStack(que, ops, new Add());
                    isop = true;
                }
                else if (Current.val.ToString() == "-")
                {
                    ArithPushStack(que, ops, new Sub());
                    isop = true;
                }
                else if (Current.val.ToString() == "*")
                {
                    ArithPushStack(que, ops, new Mul());
                    isop = true;
                }
                else if (Current.val.ToString() == "/")
                {
                    ArithPushStack(que, ops, new Div());
                    isop = true;
                }
                else if (Current.val.ToString() == "%")
                {
                    ArithPushStack(que, ops, new Mod());
                    isop = true;
                }
                else if (Current.val.ToString() == "(")
                {
                    ArithPushStack(que, ops, new OpenSub());
                    sub++;
                }
                else if (Current.val.ToString() == ")")
                {
                    if (sub < 0)
                    {
                        i++;
                        i++;
                        break;
                    }
                    else
                    {
                        ArithPushStack(que, ops, new CloseSub());
                        sub--;
                    }
                }
                else if (Current.val.ToString() == "<")
                {
                    ArithPushStack(que, ops, new CompareLess());
                    isop = true;
                }
                else if (Current.val.ToString() == "=")
                {
                    ArithPushStack(que, ops, new CompareEqu());
                    isop = true;
                }
                else if (Current.val.ToString() == ">")
                {
                    ArithPushStack(que, ops, new CompareGrt());
                    isop = true;
                }
                else
                {
                    if (isop == false)
                    {
                        i--;
                        break;
                    }
                    else
                    {
                        IEXPR ex = ParseExpr(true);
                        que.Enqueue(ex);
                        isop = false;
                    }
                }
                i++;
            }

            while (ops.Count > 0)
            {
                que.Enqueue(ops.Pop());
            }

            AritemeticExpr e = new AritemeticExpr();
            e.code = que.ToArray();
            return e;
        }

        private void ArithPushStack(Queue<IEXPR> que, Stack<IEXPR> ops, IEXPR val)
        {
            if (val is CloseSub)
            {
                while (ops.Count > 0 && !(ops.Peek() is OpenSub))
                {
                    que.Enqueue(ops.Pop());
                }
            }
            else if (ops.Count > 0)
            {
                if (ops.Peek() is OpenSub)
                {
                    ops.Push(val);
                }
                else if (ArithLevel(ops.Peek()) > ArithLevel(val))
                {
                    que.Enqueue(ops.Pop());
                    ops.Push(val);
                }
                else
                {
                    ops.Push(val);
                }
            }
            else
            {
                ops.Push(val);
            }
            
        }

        private int ArithLevel(IEXPR iEXPR)
        {
            if (iEXPR is Add || iEXPR is Sub)
            {
                return 2;
            }
            else if (iEXPR is Mul || iEXPR is Div || iEXPR is Mod)
            {
                return 3;
            }
            else if (iEXPR is CompareLess || iEXPR is CompareEqu || iEXPR is CompareGrt)
            {
                return 4;
            }
            else if (iEXPR is OpenSub)
            {
                return 5;
            }
            else
            {
                return 1;
            }
        }

    }
}
