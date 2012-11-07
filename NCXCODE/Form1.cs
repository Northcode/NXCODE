using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCXCODE
{
    public partial class Form1 : Form
    {
        Token[] tokens;
        ISTMT[] stmts;

        string code;

        string asmcode;
        string name;

        public Form1()
        {
            InitializeComponent();
        }

        private void tokenizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetCode();
            Scanner s = new Scanner(code);
            s.Scan();
            tokens = s.Tokens.ToArray();
            TokenList t = new TokenList();
            foreach (Token tk in tokens)
            {
                t.listBox1.Items.Add(tk.type.ToString() + " : " + tk.val.ToString());
            }
            t.MdiParent = this;
            t.Show();
        }

        private void GetCode()
        {
            StringBuilder codeb = new StringBuilder();
            foreach (Form f in this.MdiChildren)
            {
                if (f is TextEditWindow)
                {
                    codeb.AppendLine(((TextEditWindow)(f)).textBox1.Text);
                }
            }
            code = codeb.ToString();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditWindow t = new TextEditWindow();
            t.MdiParent = this;
            t.Show();
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tokens != null)
            {
                Parser p = new Parser(tokens);
                p.Parse();
                stmts = p.Result;

                AST_Viewer a = new AST_Viewer();

                foreach (ISTMT s in stmts)
                {
                    a.treeView1.Nodes.Add(s.ToNode());
                }

                a.MdiParent = this;
                a.Show();
            }
        }

        private void generateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder asmb = new StringBuilder();
            asmb.AppendLine("call main\n\r");
            asmb.AppendLine("terminate\n\r");
            foreach (ISTMT s in stmts)
            {
                asmb.Append(s.ToAssembly());
            }
            asmcode = asmb.ToString();
            CodeViewer v = new CodeViewer();
            v.textBox1.Text = asmcode;
            v.MdiParent = this;
            v.Show();
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (name != null)
            {
                NXCODE.Assembler a = new NXCODE.Assembler(name, Environment.UserName, asmcode);
                a.Assemble();
                a.Save(name + ".nxe");
            }
            
        }

        private void setAppNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            name = toolStripTextBox1.Text;
            MessageBox.Show("Set name to: " + name, "Name");
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (name != null)
	        {
                GetCode();
                Scanner s = new Scanner(code);
                s.Scan();
                tokens = s.Tokens.ToArray();
                TokenList t = new TokenList();
                foreach (Token tk in tokens)
                {
                    t.listBox1.Items.Add(tk.type.ToString() + " : " + tk.val.ToString());
                }
                t.MdiParent = this;
                t.Show();
                if (tokens != null)
                {
                    Parser p = new Parser(tokens);
                    p.Parse();
                    stmts = p.Result;

                    AST_Viewer a = new AST_Viewer();

                    foreach (ISTMT st in stmts)
                    {
                        a.treeView1.Nodes.Add(st.ToNode());
                    }

                    a.MdiParent = this;
                    a.Show();
                }
                StringBuilder asmb = new StringBuilder();
                asmb.AppendLine("call main\n\r");
                asmb.AppendLine("terminate\n\r");
                foreach (ISTMT st in stmts)
                {
                    asmb.Append(st.ToAssembly());
                }
                asmcode = asmb.ToString();
                CodeViewer v = new CodeViewer();
                v.textBox1.Text = asmcode;
                v.MdiParent = this;
                v.Show();
                NXCODE.Assembler asm = new NXCODE.Assembler(name, Environment.UserName, asmcode);
                asm.Assemble();
                asm.Save(name + ".nxe");
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (name != null)
            {
                Process p = new Process();
                p.StartInfo.FileName = "NXCODE.exe";
                p.StartInfo.Arguments = name + ".nxe";
                p.Start();
            }
        }

    }
}
