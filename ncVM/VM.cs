using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ncVM
{
    public class VM //The virtual machine class, used to create a machine.
    {
        private Stack<object> stack; //The object stack
        private Stack<call> callstack; //The callstack

        private Memory memory; //virtual ram
        private uint AllocAddr = 0; //Address for new objects, automatically set to 10 bytes after end of program in RAM
        private uint IP; //instruction pointer
        private bool RUN; //True if running
        private bool JUMP; //Jump if true
        private Local[] locals; //List of locals
        private Dictionary<string, Class> classes; //The list of loaded classes
        private List<Instance> instances; //Instances

        private delegate void interupt();

        private interupt[] interupts; //interupt table

        private delegate void instruction(VM machine);

        private instruction[] instructions; //instruction set

        private string name; //name of the machine

        internal static Dictionary<string, int> opcodes;

        internal static void InitOpCodes()
        {
            opcodes = new Dictionary<string, int>();
            Type it = typeof(Instructions);
            MethodInfo[] mi = it.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < mi.Length; i++)
            {
                opcodes.Add(mi[i].Name, i);
            }
        }

        public VM(string name)
        {
            this.name = name;
            stack = new Stack<object>();
            callstack = new Stack<call>();
            memory = new Memory(512);
            AllocAddr = 0;
            IP = 0;
            RUN = true;
            JUMP = false;
            locals = new Local[1000];
            classes = new Dictionary<string, Class>();
            instances = new List<Instance>();
            interupts = new interupt[] { interupt0 };
            Type instructiontype = typeof(Instructions);
            MethodInfo[] instructionmethods = instructiontype.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
            instructions = new instruction[instructionmethods.Length];
            for (int i = 0; i < instructionmethods.Length; i++)
            {
                instructions[i] = instruction.CreateDelegate(typeof(instruction), instructionmethods[i]) as instruction;
            }
        }

        private static Local allocateObject(VM m, object val)
        {
            if (val is byte)
            {
                Local l = new Local(localtype.bytepointer, m.AllocAddr);
                m.memory.Write(m.AllocAddr, (byte)val);
                m.AllocAddr++;
                return l;
            }
            else if (val is int)
            {
                Local l = new Local(localtype.intpointer, m.AllocAddr);
                m.memory.WriteDoubleWord(m.AllocAddr, (int)val);
                m.AllocAddr += 4;
                return l;
            }
            else if (val is long)
            {
                Local l = new Local(localtype.longpointer, m.AllocAddr);
                m.memory.WriteQuadWord(m.AllocAddr, (long)val);
                m.AllocAddr += 8;
                return l;
            }
            else if (val is string)
            {
                Local l = new Local(localtype.stringpointer, m.AllocAddr);
                m.memory.WriteString(m.AllocAddr, ((val as string) + (char)0x00));
                m.AllocAddr += (uint)((val as string).Length + 1);
                return l;
            }
            else if (val is Instance)
            {
                Local l = new Local(localtype.instancepointer, (uint)(val as Instance).address);
                return l;
            }
            else
            {
                throw new Exception("Unknown type " + val.GetType().ToString() + ", cannot allocate space in memory");
            }
        }

        public void DoInstruction()
        {
            byte instruction = memory.Read(IP);
            IP++;
            instructions[instruction](this);
        }

        public void ClockCycle()
        {
            if (IP < memory.Length)
            {
                DoInstruction();
            }
        }

        public void Run()
        {
            for (IP = 0; IP < memory.Length && RUN; )
            {
                DoInstruction();
            }
        }

        public void LoadProgram(byte[] contents, uint RAM)
        {
            memory = new Memory((int)(RAM + contents.Length), contents);
            AllocAddr = (uint)contents.Length + 10;
        }

        private void interupt0()
        {
            int action = (int)stack.Pop();
            if (action == 0)
            {
                string s = (string)stack.Pop();
                Console.WriteLine(s);
            }
        }

        internal class Class
        {
            internal Dictionary<string, int> fields;
            internal Dictionary<string, uint> functions;
            internal string name;

            internal int GetField(string name)
            {
                return fields[name];
            }
        }

        internal class Local
        {
            private localtype type;
            private uint addr;

            internal Local(localtype type, uint address)
            {
                this.type = type;
                this.addr = address;
            }

            internal object GetVal(VM m)
            {
                if (type == localtype.bytepointer)
                {
                    return m.memory.Read(addr);
                }
                else if (type == localtype.intpointer)
                {
                    return m.memory.ReadDoubleWord(addr);
                }
                else if (type == localtype.longpointer)
                {
                    return m.memory.ReadQuadWord(addr);
                }
                else if (type == localtype.stringpointer)
                {
                    uint tmp = 0;
                    return m.memory.ReadString(addr, out tmp);
                }
                else if (type == localtype.instancepointer)
                {
                    return m.instances[(int)addr];
                }
                else
                {
                    throw new Exception("Unknown type: " + type.ToString());
                }
            }

            internal uint GetAddress()
            {
                return addr;
            }

            internal void SetAddress(uint Addr)
            {
                addr = Addr;
            }
        }

        internal class Instance
        {
            internal Class classname;
            internal Local[] locals;
            internal int address;

            internal uint GetSize()
            {
                return (uint)(locals.Length);
            }

            internal void StoreField(VM m, string name, object value)
            {
                int pos = classname.GetField(name);
                Local l = VM.allocateObject(m, value);
                locals[pos] = l;
            }
        }

        internal class call
        {
            internal uint calladdr;
            internal int instaddr;
            internal uint retaddr;
            internal bool isIcall;

            internal call(uint calladdr, uint retaddr)
            {
                this.calladdr = calladdr;
                this.retaddr = retaddr;
                isIcall = false;
            }

            internal call(uint calladdr, int instaddr, uint retaddr)
            {
                this.calladdr = calladdr;
                this.instaddr = instaddr;
                this.retaddr = retaddr;
                isIcall = true;
            }
        }

        internal enum localtype
        {
            bytepointer,
            intpointer,
            longpointer,
            stringpointer,
            instancepointer
        }

        public static class Instructions
        {
            public static void pushbyte(VM machine) //Instruction for push byte
            {
                byte b = machine.memory.Read(machine.IP); //Read a byte from next position in memory
                machine.IP++; //increment the instuction pointer
                machine.stack.Push(b); //push the byte on the stack
            }

            public static void pushdword(VM machine) //Instuctions for push int
            {
                int i = machine.memory.ReadDoubleWord(machine.IP); //Read an int from the next position in memory
                machine.IP += 4; //increment the instruction pointer
                machine.stack.Push(i); //push the int on the stack
            }

            public static void pushqword(VM machine)
            {
                long l = machine.memory.ReadQuadWord(machine.IP); //Read a long from the next position in memory
                machine.IP += 8; //increment the instruction pointer
                machine.stack.Push(l); //push the long on the stack
            }

            public static void pushstring(VM m)
            {
                uint len = 0; //for storing length of string
                string s = m.memory.ReadString(m.IP, out len); //Reads a string from memory and stores length in len
                m.IP += len + 1; //increment the instruction pointer with length of string plus the nullterminator
                m.stack.Push(s); //Push string to the stack
            }

            public static void pop(VM machine)
            {
                machine.stack.Pop();
            }

            public static void end(VM m) //Code for terminating program
            {
                m.RUN = false; //Sets run flag to false, ending main loop
            }

            public static void interupt(VM m)
            {
                int i = m.memory.ReadDoubleWord(m.IP); //Read interupt index
                m.IP += 4;
                m.interupts[i](); //Execute interupt
            }

            public static void jmp(VM m)
            {
                uint address = (uint)m.memory.ReadDoubleWord(m.IP); //Read address
                m.IP = address;
            }

            public static void je(VM m)
            {
                uint address = (uint)m.memory.ReadDoubleWord(m.IP); //Read address
                m.IP += 4;
                if (m.JUMP)
                {
                    m.IP = address;
                }
            }

            public static void jne(VM m)
            {
                uint address = (uint)m.memory.ReadDoubleWord(m.IP); //Read address
                m.IP += 4;
                if (!m.JUMP)
                {
                    m.IP = address;
                }
            }

            public static void call(VM m)
            {
                int inst = m.memory.ReadDoubleWord(m.IP);
                m.IP += 4;
                uint address = (uint)m.memory.ReadDoubleWord(m.IP); //Read address
                m.IP += 4;
                uint caddr = m.IP; //Save current address

                if (inst == -1)
                {
                    m.callstack.Push(new call(address, caddr));//Push call to callstack
                }
                else
                {
                    m.callstack.Push(new call(address, inst, caddr));//Push call to callstack
                }

                m.IP = address; //Transfer control
            }

            public static void ret(VM m)
            {
                call prev = m.callstack.Pop(); //Load call and remove from stack
                m.IP = prev.retaddr; //Transfer control
            }

            public static void newobj(VM m)
            {
                uint tmp = 0;
                string lclass = m.memory.ReadString(m.IP, out tmp); //Read class name
                m.IP += tmp + 1;
                if (m.classes.ContainsKey(lclass))
                {
                    Instance i = new Instance();
                    i.classname = m.classes[lclass];
                    i.address = m.instances.Count;
                    i.locals = new Local[i.classname.fields.Count];
                    m.instances.Add(i);
                    m.stack.Push(i);
                }
                else
                {
                    throw new Exception("Class " + lclass + " not found in loaded classes");
                }
            }

            public static void stloc(VM m)
            {
                int pos = m.memory.ReadDoubleWord(m.IP);
                m.IP += 4;
                object val = m.stack.Pop();
                Local l = VM.allocateObject(m, val);
                m.locals[pos] = l;
            }

            public static void ldloc(VM m)
            {
                int pos = m.memory.ReadDoubleWord(m.IP);
                m.IP += 4;
                Local l = m.locals[pos];
                m.stack.Push(l.GetVal(m));
            }

            public static void ldref(VM m)
            {
                int pos = m.memory.ReadDoubleWord(m.IP);
                m.IP += 4;
                Local l = m.locals[pos];
                m.stack.Push((int)l.GetAddress());
            }

            public static void stref(VM m)
            {
                int pos = m.memory.ReadDoubleWord(m.IP);
                m.IP += 4;
                int addr = (int)m.stack.Pop();
                m.locals[pos].SetAddress((uint)addr);
            }

            public static void stfld(VM m)
            {
                object val = m.stack.Pop();
                Instance i = m.stack.Peek() as Instance;
                uint tmp = 0;
                string fld = m.memory.ReadString(m.IP, out tmp);
                m.IP += tmp + 1;
                i.StoreField(m, fld, val);
            }

            public static void ldfld(VM m)
            {
                Instance i = m.stack.Peek() as Instance;
                uint tmp = 0;
                string fld = m.memory.ReadString(m.IP, out tmp);
                m.IP += tmp + 1;
            }

            public static void ldthis(VM m)
            {
                if (m.callstack.Count > 0 && m.callstack.Peek().isIcall)
                {
                    Instance i = m.instances[m.callstack.Peek().instaddr];
                    m.stack.Push(i);
                }
            }
        }

        internal void AddClass(Class c)
        {
            classes.Add(c.name, c);
        }
    }

    public class CodeBuilder
    {
        private Memory buffer;
        private uint pos;
        private Dictionary<string, int> labels;

        public CodeBuilder(int size)
        {
            buffer = new Memory(size);
            labels = new Dictionary<string, int>();
            pos = 0;
        }

        public void WriteBytes(params object[] args)
        {
            foreach (object arg in args)
            {
                if (arg is byte)
                {
                    buffer.Write(pos, (byte)arg);
                    pos++;
                }
                else if (arg is int)
                {
                    buffer.WriteDoubleWord(pos, (int)arg);
                    pos += 4;
                }
                else if (arg is long)
                {
                    buffer.WriteQuadWord(pos, (long)arg);
                    pos += 8;
                }
                else if (arg is char)
                {
                    buffer.Write(pos, (byte)((char)arg));
                    pos++;
                }
                else if (arg is string)
                {
                    foreach (char c in (arg as string))
                    {
                        WriteBytes(c);
                    }
                }
            }
        }

        public void Jump(uint newaddress)
        {
            pos = newaddress;
        }

        public uint GetAddress()
        {
            return pos;
        }

        public byte[] GetCode()
        {
            return buffer.GetBytes();
        }

        public void AddLabel(string name, int address)
        {
            labels.Add(name, address);
        }

        public int Label(string name)
        {
            return labels[name];
        }
    }
}