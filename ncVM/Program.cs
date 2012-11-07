using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ncVM
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            VM.InitOpCodes();
            VM v = new VM("LOL");

            string line = "";
            StringBuilder sb = new StringBuilder();
            while ((line = Console.ReadLine()) != "-e")
            {
                sb.AppendLine(line);
            }

            Assembler a = new Assembler(sb.ToString());
            a.Assemble();
            v.LoadProgram(a.GetCode(), 128);
            VM.Class c = new VM.Class();
            c.name = "Test";
            c.fields = new Dictionary<string, int>();
            c.fields.Add("name", 0);
            c.functions = new Dictionary<string, uint>();
            v.AddClass(c);
            v.Run();
            Console.ReadKey();
        }
    }
}