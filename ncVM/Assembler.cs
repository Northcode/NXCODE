using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ncVM
{
    internal class Assembler
    {
        private string code;
        private CodeBuilder cb;

        internal Assembler(string code)
        {
            this.code = code;
            cb = new CodeBuilder(code.Length);
        }

        internal void Assemble()
        {
            code = code.Replace("\r", "");
            foreach (string line in code.Split('\n'))
            {
                if (line.StartsWith("-a"))
                {
                    int addr = Convert.ToInt32(line.Substring(3));
                    cb.Jump((uint)addr);
                }
                else if (line.EndsWith(":"))
                {
                    string label = line.Substring(0, line.IndexOf(':'));
                    cb.AddLabel(label, (int)cb.GetAddress());
                }
                else if (line.StartsWith("push"))
                {
                    string[] cmds = line.Split(' ');
                    if (cmds[1] == "byte")
                    {
                        byte b = byte.Parse(cmds[2], NumberStyles.HexNumber);
                        cb.WriteBytes((byte)VM.opcodes["pushbyte"], b);
                    }
                    else if (cmds[1] == "dword")
                    {
                        int i = Convert.ToInt32(cmds[2]);
                        cb.WriteBytes((byte)VM.opcodes["pushdword"], i);
                    }
                    else if (cmds[1] == "qword")
                    {
                        long l = Convert.ToInt64(cmds[2]);
                        cb.WriteBytes((byte)VM.opcodes["pushqword"], l);
                    }
                    else if (cmds[1] == "string")
                    {
                        string s = line.Substring(line.IndexOf("'") + 1, line.LastIndexOf("'") - line.IndexOf("'") - 1);
                        cb.WriteBytes((byte)VM.opcodes["pushstring"], s, (byte)0);
                    }
                }
                else if (line.Equals("end"))
                {
                    cb.WriteBytes((byte)VM.opcodes["end"]);
                }
                else if (line.StartsWith("stloc "))
                {
                    int index = Convert.ToInt32(line.Substring(6));
                    cb.WriteBytes((byte)VM.opcodes["stloc"], index);
                }
                else if (line.StartsWith("ldloc "))
                {
                    int index = Convert.ToInt32(line.Substring(6));
                    cb.WriteBytes((byte)VM.opcodes["ldloc"], index);
                }
                else if (line.StartsWith("int "))
                {
                    int index = Convert.ToInt32(line.Substring(4));
                    cb.WriteBytes((byte)VM.opcodes["interupt"], index);
                }
                else if (line.StartsWith("jmp "))
                {
                    string str = line.Substring(4);
                    int index = CheckLabel(str);
                    cb.WriteBytes((byte)VM.opcodes["jmp"], index);
                }
                else if (line.StartsWith("je "))
                {
                    string str = line.Substring(3);
                    int index = CheckLabel(str);
                    cb.WriteBytes((byte)VM.opcodes["je"], index);
                }
                else if (line.StartsWith("jne "))
                {
                    string str = line.Substring(4);
                    int index = CheckLabel(str);
                    cb.WriteBytes((byte)VM.opcodes["jne"], index);
                }
                else if (line.Equals("pop"))
                {
                    cb.WriteBytes((byte)VM.opcodes["pop"]);
                }
                else if (line.StartsWith("call "))
                {
                    string[] cmds = line.Split(' ');
                    int inst = Convert.ToInt32(cmds[1]);
                    int addr = CheckLabel(cmds[2]);
                    cb.WriteBytes((byte)VM.opcodes["call"], inst, addr);
                }
                else if (line.Equals("ret"))
                {
                    cb.WriteBytes((byte)VM.opcodes["ret"]);
                }
                else if (line.StartsWith("newobj "))
                {
                    string classname = line.Substring(7);
                    cb.WriteBytes((byte)VM.opcodes["newobj"], classname, (byte)0x00);
                }
                else if (line.StartsWith("stref "))
                {
                    int index = Convert.ToInt32(line.Substring(6));
                    cb.WriteBytes((byte)VM.opcodes["stref"], index);
                }
                else if (line.StartsWith("ldref "))
                {
                    int index = Convert.ToInt32(line.Substring(6));
                    cb.WriteBytes((byte)VM.opcodes["ldref"], index);
                }
                else if (line.StartsWith("stfld "))
                {
                    string classname = line.Substring(6);
                    cb.WriteBytes((byte)VM.opcodes["stfld"], classname, (byte)0x00);
                }
                else if (line.StartsWith("ldfld "))
                {
                    string classname = line.Substring(6);
                    cb.WriteBytes((byte)VM.opcodes["ldfld"], classname, (byte)0x00);
                }
                else if (line.Equals("ldthis"))
                {
                    cb.WriteBytes((byte)VM.opcodes["ldthis"]);
                }
            }
        }

        private int CheckLabel(string str)
        {
            int addr = -1;
            int index;
            if (int.TryParse(str, out addr))
            {
                index = addr;
            }
            else
            {
                index = cb.Label(str);
            }
            return index;
        }

        internal byte[] GetCode()
        {
            return cb.GetCode();
        }
    }
}