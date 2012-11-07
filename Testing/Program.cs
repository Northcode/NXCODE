using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MethodInfo[] mi = typeof(LOL).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo m in mi)
            {
                Console.WriteLine(m.Name);
            }
            Console.ReadKey();
        }
    }

    internal class LOL
    {
        public static void TROLOLO()
        {
        }

        public static void LALALA()
        {
        }

        public static void FUFUFUFU()
        {
        }
    }
}