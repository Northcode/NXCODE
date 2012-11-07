using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace n32
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CodeBuilder cb = new CodeBuilder(600);

            //cb.AddLabel("printstring", 200);
            //cb.AddLabel("readstring", 300);
            //cb.AddLabel("return", 290);
            //cb.AddLabel("buffer", 500);

            //cb.WriteBytes(VM.ops.movdw, VM.SI, cb.Label("buffer"));
            //cb.WriteBytes(VM.ops.call, cb.Label("readstring"));
            //cb.WriteBytes(VM.ops.movb, VM.CL, (byte)0x00,
            //    VM.ops.movb, VM.CH, (byte)0x01,
            //    VM.ops.movb, VM.AH, (byte)0x03,
            //    VM.ops.doint, (byte)0x01);
            //cb.WriteBytes(VM.ops.movdw, VM.SI, cb.Label("buffer"));
            //cb.WriteBytes(VM.ops.call, cb.Label("printstring"));
            //cb.WriteBytes(VM.ops.end);
            //cb.Jump((uint)cb.Label("printstring"));
            //cb.WriteBytes(VM.ops.mova, VM.AL, VM.SI);
            //cb.WriteBytes(VM.ops.cmp, VM.AL, (byte)0x01, (byte)0x00);
            //cb.WriteBytes(VM.ops.je, cb.Label("return"));
            //cb.WriteBytes(VM.ops.movb, VM.AH, (byte)0x01);
            //cb.WriteBytes(VM.ops.doint, (byte)0x01);
            //cb.WriteBytes(VM.ops.inc, VM.SI);
            //cb.WriteBytes(VM.ops.jmp, cb.Label("printstring"));
            //cb.Jump((uint)cb.Label("return"));
            //cb.WriteBytes(VM.ops.ret);
            //cb.Jump((uint)cb.Label("readstring"));
            //cb.WriteBytes(
            //        VM.ops.movb, VM.AH, (byte)0x02,
            //        VM.ops.doint, (byte)0x01,
            //        VM.ops.cmp, VM.AL, (byte)0x01, '\r',
            //        VM.ops.je, cb.Label("return"),
            //        VM.ops.movar, VM.SI, VM.AL,
            //        VM.ops.inc, VM.SI,
            //        VM.ops.jmp, cb.Label("readstring")
            //    );

            string line = "";

            StringBuilder sb = new StringBuilder();

            while ((line = Console.ReadLine()) != "!end")
            {
                sb.AppendLine(line);
            }

            Assembler a = new Assembler(sb.ToString());

            a.Assemble();

            byte[] program = a.GetData();

            VM v = new VM();
            v.LoadProgram(program);
            v.Run();
            Console.ReadKey();
        }
    }
}