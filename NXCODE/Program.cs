using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using NXCODE.Virtual_Machine;

namespace NXCODE
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "/c")
                {
                    Assembler a = new Assembler(args[2], args[3], File.ReadAllText(args[1]));
                    a.Assemble();
                    a.Save(args[4]);
                }
                else
                {
                    Assembler.PopulateByteCodes();

                    VM v = new VM();
                    v.LoadProgram(args[0]);
                    v.NewThread("Main");
                    v.Run(0);
                }
            }
            else if (Console.ReadLine() == "c")
            {
                Console.Write("File to compile: ");
                string filename = Console.ReadLine();
                Console.WriteLine("-------------------------");
                Console.Write("Application name: ");
                string Aname = Console.ReadLine();
                Console.Write("Author: ");
                string Aauth = Console.ReadLine();

                string code = File.ReadAllText(filename);

                Assembler a = new Assembler(Aname, Aauth, code);
                a.Assemble();
                a.Save(Aname + ".nxe");
            }
            else
            {
                Assembler.PopulateByteCodes();

                Console.Write("File to run: ");
                string Aname = Console.ReadLine();

                VM v = new VM();
                v.LoadProgram(Aname);
                v.NewThread("Main");
                v.Run(0);
            }
            
        }
    }
}
