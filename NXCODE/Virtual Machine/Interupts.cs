using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NXCODE.Virtual_Machine
{
    class Interupt1 : Interupt
    {
        public void Do(Stack<object> stack, Dictionary<string, object> vars, object[] callargs)
        {
            int option = (int)stack.Pop();
            switch (option)
            {
                case 0:
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), callargs[0] as string);
                    break;
                default:
                    break;
            }
        }
    }

    class Interupt2 : Interupt
    {
        public void Do(Stack<object> stack, Dictionary<string, object> vars, object[] callargs)
        {
            int option = (int)stack.Pop();
            if (option == 0)
	        {
                string mode = callargs[0] as string;
                FileStream s = new FileStream(callargs[1] as string, (mode == "write" ? FileMode.OpenOrCreate : FileMode.Open));
                if (mode == "write")
                {
                    BinaryWriter w = new BinaryWriter(s);
                    stack.Push(w);
                }
                else
                {
                    BinaryReader r = new BinaryReader(s);
                    stack.Push(r);
                }
	        }
            else if (option == 1)
            {
                BinaryReader r = callargs[0] as BinaryReader;
                int mode = (int)callargs[1];
                if (mode == 0)
                {
                    stack.Push(r.ReadString());
                }
                else if (mode == 1)
                {
                    stack.Push(r.ReadInt32());
                }
                else if (mode == 2)
                {
                    stack.Push(r.ReadDouble());
                }
                else if (mode == 3)
                {
                    stack.Push(r.ReadBoolean());
                }
            }
            else if (option == 2)
            {
                BinaryWriter w = callargs[0] as BinaryWriter;
                object val = callargs[1];
                if (val is string)
                {
                    w.Write(val as string);
                }
                else if (val is int)
                {
                    w.Write((int)val);
                }
                else if (val is double)
                {
                    w.Write((double)val);
                }
                else if (val is bool)
                {
                    w.Write((bool)val);
                }
            }
            else if (option == 3)
            {
                int mode = (int)callargs[0];
                if (mode == 0)
                {
                    BinaryReader b = callargs[1] as BinaryReader;
                    b.Close();
                }
                else
                {
                    BinaryWriter w = callargs[1] as BinaryWriter;
                    w.Close();
                }
            }
            else if (option == 4)
            {
                string file = callargs[0] as string;
                string text = callargs[1] as string;
                File.WriteAllText(file, text);
            }
            else if (option == 5)
            {
                string file = callargs[0] as string;
                stack.Push(File.ReadAllText(file));
            }
            else if (option == 6)
            {
                string file = callargs[0] as string;
                StreamWriter sw = new StreamWriter(new FileStream(file, FileMode.OpenOrCreate));
                stack.Push(sw);
            }
            else if (option == 7)
            {
                StreamWriter sw = callargs[0] as StreamWriter;
                if (sw != null)
                {
                    sw.Close();
                }
            }
            else if (option == 8)
            {
                StreamWriter sw = callargs[0] as StreamWriter;
                string value = callargs[1] as string;
                if (sw != null && value != null)
                {
                    sw.Write(value);
                }
            }
            else if (option == 9)
            {
                string file = callargs[0] as string;
                StreamReader sw = new StreamReader(new FileStream(file, FileMode.OpenOrCreate));
                stack.Push(sw);
            }
            else if (option == 10)
            {
                StreamReader sw = callargs[0] as StreamReader;
                if (sw != null)
                {
                    sw.Close();
                }
            }
            else if (option == 11)
            {
                StreamReader sw = callargs[0] as StreamReader;
                if (sw != null)
                {
                    string value = sw.ReadLine();
                    stack.Push(value);
                }
            }
        }
    }

    class Interupt3 : Interupt
    {
        public void Do(Stack<object> stack, Dictionary<string, object> vars, object[] callargs)
        {
            int opcode = (int)stack.Pop();
            if (opcode == 0)
            {
                string name = callargs[0] as string;
                Form f = new Form();
                f.Name = name;
                stack.Push(f);
            }
            else if (opcode == 1)
            {
                Form f = callargs[0] as Form;
                int w = (int)callargs[1];
                int h = (int)callargs[2];

                f.Width = w;
                f.Height = h;
            }
            else if (opcode == 2)
            {
                Form f = callargs[0] as Form;
                string title = callargs[1] as string;

                f.Text = title;
            }
            else if (opcode == 3)
            {
                Form f = callargs[0] as Form;
                f.Show();
            }
            else if (opcode == 4)
            {
                Form f = callargs[0] as Form;
                f.Close();
            }
            else if (opcode == 5)
            {
                Form f = callargs[0] as Form;
                Application.Run(f);
            }
        }
    }
}
