using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace n32
{
    public class Assembler
    {
        private CodeBuilder cb;
        private string code;

        public Assembler(string Code)
        {
            code = Code;
        }

        public void Assemble()
        {
            cb = new CodeBuilder(code.Split('\n').Length * 8 * 64);

            foreach (string l in code.Split('\n'))
            {
                string line = l.Replace("\r", "").Replace("\t", "");

                if (line.StartsWith("mov "))
                {
                    line = ParseMov(line);
                }
                else if (line.EndsWith(":"))
                {
                    cb.AddLabel(line.Substring(0, line.Length - 2), (int)cb.GetAddress());
                }
            }
        }

        private string ParseMov(string line)
        {
            line = line.Substring(4);

            string[] linec = line.Split(',');
            linec[0] = linec[0].Trim();

            if (linec[0].StartsWith("[") && linec[0].EndsWith("]")) // mov [something], something
            {
                int addr = 0;
                if (int.TryParse(linec[0].Substring(1, linec[0].IndexOf(']') - 2), out addr)) // mov [int], something
                {
                    if (CheckRegisters(linec[1])) //mov [int],reg
                    {
                        cb.WriteBytes((byte)VM.ops.str);
                        WriteRegisters(linec[1]);
                        cb.WriteBytes((int)addr);
                    }
                }
                else if (CheckRegisters(linec[0].Substring(1, linec[0].IndexOf(']') - 2))) // mov [reg],something
                {
                    if (linec[1].StartsWith("[") && linec[1].EndsWith("]")) //mov [reg],[something]
                    {
                        if (CheckRegisters(linec[1].Substring(1, linec[1].IndexOf(']') - 2))) //mov [reg],[reg]
                        {
                            cb.WriteBytes((byte)VM.ops.movar);
                            WriteRegisters(linec[0].Substring(1, linec[0].IndexOf(']') - 2));
                            WriteRegisters(linec[1].Substring(1, linec[1].IndexOf(']') - 2));
                        }
                    }
                    else if (int.TryParse(linec[1], out addr)) //mov [reg],int
                    {
                        cb.WriteBytes((byte)VM.ops.lod);
                        WriteRegisters(linec[0].Substring(1, linec[0].IndexOf(']') - 2));
                        cb.WriteBytes(addr);
                    }
                }
            }
            else if (CheckRegisters(linec[0]))
            {
                if (true)
                {
                }
            }
            return line;
        }

        public byte[] GetData() { return cb.GetCode(); }

        private bool CheckRegisters(string linec)
        {
            if (linec == "al")
            {
                return true;
            }
            else if (linec == "ah")
            {
                return true;
            }
            else if (linec == "ax")
            {
                return true;
            }
            else if (linec == "bl")
            {
                return true;
            }
            else if (linec == "bx")
            {
                return true;
            }
            else if (linec == "ch")
            {
                return true;
            }
            else if (linec == "cl")
            {
                return true;
            }
            else if (linec == "ex")
            {
                return true;
            }
            else if (linec == "eh")
            {
                return true;
            }
            else if (linec == "si")
            {
                return true;
            }
            else if (linec == "sp")
            {
                return true;
            }
            else if (linec == "ss")
            {
                return true;
            }
            else if (linec == "di")
            {
                return true;
            }
            else if (linec == "dp")
            {
                return true;
            }
            else if (linec == "ds")
            {
                return true;
            }
            else if (linec == "eax")
            {
                return true;
            }
            else if (linec == "eah")
            {
                return true;
            }
            else if (linec == "rex")
            {
                return true;
            }
            else if (linec == "reh")
            {
                return true;
            }
            else if (linec == "rax")
            {
                return true;
            }
            return false;
        }

        private void WriteRegisters(string linec)
        {
            if (linec == "al")
            {
                cb.WriteBytes((byte)VM.AL);
            }
            else if (linec == "ah")
            {
                cb.WriteBytes((byte)VM.AH);
            }
            else if (linec == "ax")
            {
                cb.WriteBytes((byte)VM.AX);
            }
            else if (linec == "bl")
            {
                cb.WriteBytes((byte)VM.BL);
            }
            else if (linec == "bx")
            {
                cb.WriteBytes((byte)VM.BX);
            }
            else if (linec == "ch")
            {
                cb.WriteBytes((byte)VM.CH);
            }
            else if (linec == "cl")
            {
                cb.WriteBytes((byte)VM.CL);
            }
            else if (linec == "ex")
            {
                cb.WriteBytes((byte)VM.EX);
            }
            else if (linec == "eh")
            {
                cb.WriteBytes((byte)VM.EH);
            }
            else if (linec == "si")
            {
                cb.WriteBytes((byte)VM.SI);
            }
            else if (linec == "sp")
            {
                cb.WriteBytes((byte)VM.SP);
            }
            else if (linec == "ss")
            {
                cb.WriteBytes((byte)VM.SS);
            }
            else if (linec == "di")
            {
                cb.WriteBytes((byte)VM.DI);
            }
            else if (linec == "dp")
            {
                cb.WriteBytes((byte)VM.DP);
            }
            else if (linec == "ds")
            {
                cb.WriteBytes((byte)VM.DS);
            }
            else if (linec == "eax")
            {
                cb.WriteBytes((byte)VM.EAX);
            }
            else if (linec == "eah")
            {
                cb.WriteBytes((byte)VM.EAH);
            }
            else if (linec == "rex")
            {
                cb.WriteBytes((byte)VM.REX);
            }
            else if (linec == "reh")
            {
                cb.WriteBytes((byte)VM.REH);
            }
            else if (linec == "rax")
            {
                cb.WriteBytes((byte)VM.RAX);
            }
        }
    }
}