using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace NCXCODE
{
    interface ISTMT
    {
        string ToAssembly();

        System.Windows.Forms.TreeNode ToNode();
    }

    interface IEXPR : ISTMT
    {
    }

    class StringLit : IEXPR
    {
        public string val;

        public string ToAssembly()
        {
            return "push '" + val + "'\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "string: " + val;
            return t;
        }
    }

    class IntLit : IEXPR
    {
        public int val;

        public string ToAssembly()
        {
            return "push " + val.ToString() + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "int: " + val.ToString();
            return t;
        }
    }

    class DoubleLit : IEXPR
    {
        public double val;

        public string ToAssembly()
        {
            return "push d" + val.ToString() + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "double: " + val.ToString();
            return t;
        }
    }

    class BoolLit : IEXPR
    {
        public bool val;

        public string ToAssembly()
        {
            return "push " + val.ToString().ToLower() + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "bool: " + val.ToString();
            return t;
        }
    }

    class CharLit : IEXPR
    {
        public char val;

        public string ToAssembly()
        {
            return "push c" + val.ToString() + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "char: " + val.ToString();
            return t;
        }
    }

    class FuncCall : IEXPR
    {
        public string method;
        public IEXPR[] args;

        public string ToAssembly()
        {
            StringBuilder asm = new StringBuilder();
            foreach (IEXPR a in args)
            {
                asm.Append(a.ToAssembly());
            }
            asm.AppendLine("callargs " + args.Length);
            for (int i = args.Length - 1; i < args.Length && i >= 0; i--)
            {
                asm.AppendLine("starg " + i);
            }
            asm.AppendLine("call " + method);
            return asm.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "func call: " + method;
            foreach (IEXPR arg in args)
            {
                t.Nodes.Add(arg.ToNode());
            }
            return t;
        }
    }

    class LoadVar : IEXPR
    {
        public string name;


        public string ToAssembly()
        {
            StringBuilder asm = new StringBuilder();
            asm.AppendLine("ldloc " + name);
            return asm.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "load var: " + name;
            return t;
        }
    }

    class Else : ISTMT
    {

        public string ToAssembly()
        {
            return "";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "else";
            return t;
        }
    }

    class BoolExpr : IEXPR
    {
        internal IEXPR first;
        internal IEXPR last;
        internal char op;

        public string ToAssembly()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(first.ToAssembly());
            sb.Append(last.ToAssembly());
            if (op == '<')
            {
                sb.AppendLine("cls");
            }
            else if (op == '=')
            {
                sb.AppendLine("cmp");
            }
            else if (op == '>')
            {
                sb.AppendLine("cgt");
            }
            return sb.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "bool expr";
            t.Nodes.Add(first.ToNode());
            t.Nodes.Add(op.ToString());
            t.Nodes.Add(last.ToNode());
            return t;
        }
    }

    class AritemeticExpr : IEXPR
    {
        internal IEXPR[] code;

        public string ToAssembly()
        {
            StringBuilder asmb = new StringBuilder();
            foreach (IEXPR e in code)
            {
                asmb.Append(e.ToAssembly());
            }
            return asmb.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode tn = new TreeNode("Arithmetic expr");
            foreach (IEXPR e in code)
            {
                tn.Nodes.Add(e.ToNode());
            }
            return tn;
        }
    }

    class Add : IEXPR
    {
        public string ToAssembly()
        {
            return "add\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("add");
        }
    }

    class Sub : IEXPR
    {
        public string ToAssembly()
        {
            return "sub\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("sub");
        }
    }

    class Mul : IEXPR
    {
        public string ToAssembly()
        {
            return "mul\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("mul");
        }
    }

    class Div : IEXPR
    {
        public string ToAssembly()
        {
            return "div\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("div");
        }
    }

    class Mod : IEXPR
    {
        public string ToAssembly()
        {
            return "mod\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("mod");
        }
    }

    class OpenSub : IEXPR
    {
        public string ToAssembly()
        {
            return "";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("open sub expr");
        }
    }

    class CloseSub : IEXPR
    {
        public string ToAssembly()
        {
            return "";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("close sub expr");
        }
    }

    class SetVar : ISTMT
    {
        public string name;
        public IEXPR value;

        public string ToAssembly()
        {
            StringBuilder asm = new StringBuilder();
            asm.Append(value.ToAssembly());
            asm.AppendLine("stloc " + name);
            return asm.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "setvar: " + name;
            t.Nodes.Add(value.ToNode());
            return t;
        }
    }

    class End : ISTMT
    {
        public string ToAssembly()
        {
            return "";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            throw new NotImplementedException();
        }
    }

    class DefineFunc : ISTMT
    {
        public string name;
        public string[] Args;
        public ISTMT[] body;


        public string ToAssembly()
        {
            StringBuilder asm = new StringBuilder();
            asm.AppendLine("func " + name);
            for (int i = 0; i < Args.Length; i++)
            {
                asm.AppendLine("ldarg " + i);
                asm.AppendLine("stloc " + Args[i]);
            }
            foreach (ISTMT s in body)
            {
                asm.Append(s.ToAssembly());
            }
            asm.AppendLine("ret");
            return asm.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "function: " + name;
            TreeNode args = new TreeNode("args:");
            foreach (string str in Args)
            {
                args.Nodes.Add(str);
            }
            TreeNode body = new TreeNode("code body:");
            foreach (ISTMT st in this.body)
            {
                body.Nodes.Add(st.ToNode());
            }
            t.Nodes.Add(args);
            t.Nodes.Add(body);
            return t;
        }
    }

    class DefVar : ISTMT
    {
        public string name;
        public string access;

        public string ToAssembly()
        {
            return "var " + access + " " + name + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "var: " + name + " | access: " + access;
            return t;
        }
    }

    class DefClass : ISTMT
    {
        public string Name;
        public DefVar[] vars;
        public DefineFunc[] funcs;

        public string ToAssembly()
        {
            StringBuilder asm = new StringBuilder();
            asm.AppendLine("class " + Name);
            foreach (DefVar v in vars)
            {
                asm.Append(v.ToAssembly());
            }
            foreach (DefineFunc f in funcs)
            {
                asm.Append(f.ToAssembly());
            }
            asm.AppendLine("end");
            return asm.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "class: " + Name;
            TreeNode vs = new TreeNode("variables:");
            foreach (DefVar d in vars)
            {
                vs.Nodes.Add(d.ToNode());
            }
            TreeNode fs = new TreeNode("functions:");
            foreach (DefineFunc d in funcs)
            {
                fs.Nodes.Add(d.ToNode());
            }
            t.Nodes.Add(vs);
            t.Nodes.Add(fs);
            return t;
        }
    }

    class BaseCode : ISTMT
    {
        internal string val;

        public string ToAssembly()
        {
            return val;
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "base code:";
            t.Nodes.Add(val);
            return t;
        }
    }

    class Open : ISTMT
    {
        internal string name;

        public string ToAssembly()
        {
            return "open " + name + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "open: " + name;
            return t;
        }
    }

    class Return : ISTMT
    {
        internal IEXPR value;

        public string ToAssembly()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(value.ToAssembly());
            sb.AppendLine("ret");
            return sb.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "return";
            return t;
        }
    }

    class New : IEXPR
    {
        internal string from;

        public string ToAssembly()
        {
            return "newobj " + from + "\r\n";
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode();
            t.Text = "new: " + from;
            return t;
        }
    }

    class If : ISTMT
    {
        internal IEXPR eval;
        internal ISTMT[] truebody;
        internal ISTMT[] falsebody;

        public string ToAssembly()
        {
            StringBuilder sb = new StringBuilder();
            int lb1 = Parser.labelcounter++;
            int lb2 = Parser.labelcounter++;

            sb.Append(eval.ToAssembly());
            sb.AppendLine("jn " + lb1.ToString());
            foreach (ISTMT s in truebody)
            {
                sb.Append(s.ToAssembly());
            }
            sb.AppendLine("jmp " + lb2.ToString());

            sb.AppendLine(":" + lb1.ToString());
            if (falsebody != null)
            {
                foreach (ISTMT s in falsebody)
                {
                    sb.Append(s.ToAssembly());
                }
            }
            sb.AppendLine(":" + lb2.ToString());
            return sb.ToString();
        }


        public System.Windows.Forms.TreeNode ToNode()
        {
            TreeNode t = new TreeNode("If");
            TreeNode et = new TreeNode("Expression:");
            et.Nodes.Add(eval.ToNode());
            TreeNode tt = new TreeNode("If true:");
            foreach (ISTMT s in truebody)
            {
                tt.Nodes.Add(s.ToNode());
            }


            t.Nodes.Add(et);
            t.Nodes.Add(tt);

            if(falsebody != null)
            {
            TreeNode ft = new TreeNode("If false:");
            foreach (ISTMT s in falsebody)
            {
                ft.Nodes.Add(s.ToNode());
            }

            t.Nodes.Add(ft);

            }
            return t;
        }
    }

    class Loop : ISTMT
    {
        internal string from;
        internal IEXPR to, by;
        internal ISTMT[] body;

        public string ToAssembly()
        {
            StringBuilder asmb = new StringBuilder();
            int lb1 = Parser.labelcounter++;
            int lb2 = Parser.labelcounter++;
            asmb.AppendLine(":" + lb1);
            //Checker
            asmb.AppendLine("ldloc " + from);
            asmb.Append(to.ToAssembly());
            asmb.AppendLine("cls");
            asmb.AppendLine("jn " + lb2);

            //Increment
            asmb.AppendLine("ldloc " + from);
            asmb.Append(by.ToAssembly());
            asmb.AppendLine("add");
            asmb.AppendLine("stloc " + from);

            //Body
            foreach (ISTMT s in body)
            {
                asmb.Append(s.ToAssembly());
            }
            asmb.AppendLine("jmp " + lb1);
            asmb.AppendLine(":" + lb2);
            return asmb.ToString();
        }

        public TreeNode ToNode()
        {
            TreeNode tn = new TreeNode("loop");
            tn.Nodes.Add("from: " + from);
            TreeNode to = new TreeNode("to:");
            to.Nodes.Add(this.to.ToNode());
            tn.Nodes.Add(to);
            TreeNode by = new TreeNode("by:");
            by.Nodes.Add(this.by.ToNode());
            tn.Nodes.Add(by);
            TreeNode bn = new TreeNode("body:");
            foreach (ISTMT s in body)
            {
                bn.Nodes.Add(s.ToNode());
            }
            tn.Nodes.Add(bn);
            return tn;
        }
    }

    class While : ISTMT
    {
        internal IEXPR expr;
        internal ISTMT[] body;

        public string ToAssembly()
        {
            StringBuilder asmb = new StringBuilder();
            int lbl1 = Parser.labelcounter++;
            int lbl2 = Parser.labelcounter++;
            asmb.AppendLine(":" + lbl1);
            asmb.Append(expr.ToAssembly());
            asmb.AppendLine("jn " + lbl2);
            foreach (ISTMT s in body)
            {
                asmb.Append(s.ToAssembly());
            }
            asmb.AppendLine("jmp " + lbl1);
            asmb.AppendLine(":" + lbl2);
            return asmb.ToString();
        }

        public TreeNode ToNode()
        {
            TreeNode tn = new TreeNode("while");
            TreeNode exprn = new TreeNode("expr");
            exprn.Nodes.Add(expr.ToNode());
            TreeNode bodyn = new TreeNode("body:");
            foreach (ISTMT i in body)
            {
                bodyn.Nodes.Add(i.ToNode());
            }
            tn.Nodes.Add(exprn);
            tn.Nodes.Add(bodyn);
            return tn;
        }
    }

    class Xml : ISTMT
    {
        internal string text;

        public string ToAssembly()
        {
            StringReader tr = new StringReader(text);
            XmlReader xr = XmlReader.Create(tr);
            StringBuilder asmb = new StringBuilder();

            while (xr.Read())
            {
                if (xr.Name == "obj")
                {
                    string type = xr.GetAttribute("type");
                    string name = xr.GetAttribute("name");
                    List<Tuple<string, string, bool>> fields = new List<Tuple<string, string, bool>>();
                    while (xr.Read() && xr.NodeType != XmlNodeType.EndElement)
                    {
                        string n = xr.Name;
                        string val = xr.GetAttribute("value");
                        bool t = (xr.GetAttribute("string") != null);
                        fields.Add(new Tuple<string, string, bool>(n, val, t));
                    }

                    asmb.AppendLine("newobj " + type);
                    asmb.AppendLine("stloc " + name);
                    foreach (Tuple<string, string, bool> fld in fields)
                    {
                        if (fld.Item1 != "")
                        {
                            asmb.AppendLine("push " +(fld.Item3 ? "'" : "") + fld.Item2+ (fld.Item3 ? "'" : ""));
                            asmb.AppendLine("stloc " + name + "." +  fld.Item1 );
                        }
                    }
                }
            }

            return asmb.ToString();
        }

        public TreeNode ToNode()
        {
            TreeNode tn = new TreeNode("xmlgen");
            foreach (string str in text.Split('\n'))
            {
                tn.Nodes.Add(str);
            }
            return tn;
        }



    }


    class CompareLess : IEXPR
    {
        public string ToAssembly()
        {
            return "cls\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("cls");
        }
    }

    class CompareEqu : IEXPR
    {
        public string ToAssembly()
        {
            return "cmp\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("cmp");
        }
    }

    class CompareGrt : IEXPR
    {
        public string ToAssembly()
        {
            return "cgt\r\n";
        }

        public TreeNode ToNode()
        {
            return new TreeNode("cgt");
        }
    }
}
