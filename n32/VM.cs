using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace n32
{
    public class VM
    {
        private Register[] registers; //Registers

        private Memory mem; //Virtual RAM

        private Stack<object> stack; //Virtual Stack
        private Stack<Tuple<uint, uint>> callstack; //holds information on call instructions, tells the ret function where to return to
        private uint IP; //Instruction pointer
        private bool RUN; //If false stops execution of the machine
        private bool JUMP; //Jumpflag

        //byte currentthread;

        private delegate void interupt();

        private interupt[] interupts;

        //Register names
        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte AL = 0;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte AX = 1;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte BX = 2;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte BL = 3;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte AH = 4;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte CL = 5;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte CH = 6;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte EX = 7;

        /// <summary>
        /// 8 bit register
        /// </summary>
        public const byte EH = 8;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte SI = 9;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte SP = 10;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte SS = 11;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte DI = 12;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte DP = 13;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte DS = 14;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte EAX = 15;

        /// <summary>
        /// 32 bit register
        /// </summary>
        public const byte EAH = 16;

        /// <summary>
        /// 64 bit register
        /// </summary>
        public const byte REX = 17;

        /// <summary>
        /// 64 bit register
        /// </summary>
        public const byte REH = 18;

        /// <summary>
        /// 64 bit register
        /// </summary>
        public const byte RAX = 19;

        //OPCODES
        public static class ops
        {
            /// <summary>
            /// Move register
            /// </summary>
            public const byte movr = 0x00;

            /// <summary>
            /// Move byte
            /// </summary>
            public const byte movb = 0x01;

            /// <summary>
            /// Move int32
            /// </summary>
            public const byte movdw = 0x02;

            /// <summary>
            /// Move int64
            /// </summary>
            public const byte movqw = 0x03;

            /// <summary>
            /// Move from memory
            /// </summary>
            public const byte mova = 0x04;

            /// <summary>
            /// Move to memory
            /// </summary>
            public const byte movar = 0x33;

            //Math functions, stores result in AX for 8 bit, EAX for 32 bit & RAX for 64 bit
            /// <summary>
            /// add two registers
            /// </summary>
            public const byte add = 0x05;

            /// <summary>
            /// subtract two registers
            /// </summary>
            public const byte sub = 0x06;

            /// <summary>
            /// multiply two registers
            /// </summary>
            public const byte mul = 0x07;

            /// <summary>
            /// divide two registers
            /// </summary>
            public const byte div = 0x09;

            /// <summary>
            /// modulus two registers
            /// </summary>
            public const byte mod = 0x0a;

            public const byte push = 0x0b; //Push a register on the stack

            //public const byte pushdw = 0x0a; //Push int32 on stack
            //public const byte pushqw = 0x0c; //Push int64 on stack

            public const byte pop = 0x0d; //Pop a value to a register from the stack

            //public const byte popdw = 0x0e; //Pop int32 off stack
            //public const byte popqw = 0x0f; //Pop int64 off stack

            public const byte str = 0x10; //stores a register in memory

            //public const byte stdw = 0x11; //store int32 in memory
            //public const byte stqw = 0x12; //store int64 in memory

            public const byte lod = 0x13; //load memory to a register

            //public const byte lddw = 0x14; //load int32 from memory
            //public const byte ldqw = 0x15; //load int64 from memory

            public const byte doint = 0x16; //execute an interupt

            public const byte call = 0x17; //call an address
            public const byte ret = 0x18; //return to caller
            public const byte hlt = 0x19; //pause for [AL] milliseconds

            public const byte sjf = 0x1a; //set jump flag
            public const byte cjf = 0x1c; //clear jump flag

            public const byte jmp = 0x1d; //jump to address
            public const byte je = 0x1e; //jump if flag is set
            public const byte jne = 0x1f; //jump if flag is cleared

            public const byte cmp = 0x20; //compare two registers
            public const byte cls = 0x21; //compare less than
            public const byte cle = 0x22; //compare less than or equal
            public const byte cgt = 0x23; //compare greater than
            public const byte cge = 0x24; //compare greater than or equal
            public const byte cne = 0x25; //compare not equal

            public const byte defclass = 0x26; //define class
            public const byte deffld = 0x27; //define field
            public const byte defmethod = 0x28; //define method
            public const byte deflabel = 0x29; //define label
            public const byte newobj = 0x2a; //create new instance of class
            public const byte ldloc = 0x2b; //load local
            public const byte stloc = 0x2c; //store local
            public const byte ldfld = 0x2d; //load field
            public const byte stfld = 0x2e; //store field
            public const byte delobj = 0x2f; //delete instance of class

            public const byte end = 0x30; //Stop the program

            /// <summary>
            /// increments a register by 1
            /// </summary>
            public const byte inc = 0x31;

            /// <summary>
            /// Decrements a register by 1
            /// </summary>
            public const byte dec = 0x32;
        }

        public VM()
        {
            registers = new Register[20];

            for (int i = 0; i < 9; i++)
            {
                registers[i] = new ByteRegister();
            }
            for (int i = 9; i < 17; i++)
            {
                registers[i] = new DoubleWordRegister();
            }
            for (int i = 17; i < 20; i++)
            {
                registers[i] = new QuadWordRegister();
            }

            mem = new Memory(512);

            RUN = false;
            JUMP = false;

            stack = new Stack<object>();
            callstack = new Stack<Tuple<uint, uint>>();

            interupts = new interupt[] { int1, int2, int3, int4 };
        }

        public void ClockCycle()
        {
            if (IP < mem.Length)
            {
                DoInstruction();
            }
        }

        public uint DoInstruction()
        {
            byte instruction = mem.Read(IP);
            if (instruction == ops.movr)
            {
                byte registerA = mem.Read(++IP);
                byte registerB = mem.Read(++IP);
                IP++;
                registers[registerA].setvalue(registers[registerB].GetValue());
            }
            else if (instruction == ops.movb)
            {
                byte register = mem.Read(++IP);
                byte value = mem.Read(++IP);
                IP++;
                registers[register].setvalue(value);
            }
            else if (instruction == ops.movdw)
            {
                byte register = mem.Read(++IP);
                int value = mem.ReadDoubleWord(++IP);
                IP += 4;
                registers[register].setvalue(value);
            }
            else if (instruction == ops.movqw)
            {
                byte register = mem.Read(++IP);
                long value = mem.ReadQuadWord(++IP);
                IP += 8;
                registers[register].setvalue(value);
            }
            else if (instruction == ops.mova)
            {
                byte register = mem.Read(++IP);
                byte aregister = mem.Read(++IP);
                IP++;

                uint addr = 0x00;
                if (registers[aregister] is ByteRegister)
                {
                    byte b = (byte)registers[aregister].GetValue();
                    addr = (uint)b;
                }
                else if (registers[aregister] is DoubleWordRegister)
                {
                    int b = (int)registers[aregister].GetValue();
                    addr = (uint)b;
                }
                else if (registers[aregister] is QuadWordRegister)
                {
                    long b = (long)registers[aregister].GetValue();
                    addr = (uint)b;
                }

                if (registers[register] is ByteRegister)
                {
                    registers[register].setvalue(mem.Read(addr));
                }
                else if (registers[register] is DoubleWordRegister)
                {
                    registers[register].setvalue(mem.ReadDoubleWord(addr));
                }
                else if (registers[register] is QuadWordRegister)
                {
                    registers[register].setvalue(mem.ReadQuadWord(addr));
                }
            }
            else if (instruction == ops.movar)
            {
                byte aregister = mem.Read(++IP);
                byte register = mem.Read(++IP);
                IP++;

                uint addr = 0x00;
                if (registers[aregister] is ByteRegister)
                {
                    byte b = (byte)registers[aregister].GetValue();
                    addr = (uint)b;
                }
                else if (registers[aregister] is DoubleWordRegister)
                {
                    int b = (int)registers[aregister].GetValue();
                    addr = (uint)b;
                }
                else if (registers[aregister] is QuadWordRegister)
                {
                    long b = (long)registers[aregister].GetValue();
                    addr = (uint)b;
                }

                if (registers[register] is ByteRegister)
                {
                    mem.Write(addr, (byte)registers[register].GetValue());
                }
                else if (registers[register] is DoubleWordRegister)
                {
                    mem.WriteDoubleWord(addr, (int)registers[register].GetValue());
                }
                else if (registers[register] is QuadWordRegister)
                {
                    mem.WriteQuadWord(addr, (long)registers[register].GetValue());
                }
            }
            else if (instruction == ops.add)
            {
                byte regA = mem.Read(++IP);
                byte regB = mem.Read(++IP);
                IP++;

                if (registers[regA] is ByteRegister)
                {
                    if (registers[regB] is ByteRegister)
                    {
                        byte result = (byte)(((byte)registers[regA].GetValue()) + ((byte)registers[regB].GetValue()));
                        registers[AX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot add a register of another size than 8 to a register of size 8");
                    }
                }
                else if (registers[regA] is DoubleWordRegister)
                {
                    if (registers[regB] is DoubleWordRegister)
                    {
                        int result = (((int)registers[regA].GetValue()) + ((int)registers[regB].GetValue()));
                        registers[EAX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot add a register of another size than 32 to a register of size 32");
                    }
                }
                else if (registers[regA] is QuadWordRegister)
                {
                    if (registers[regB] is QuadWordRegister)
                    {
                        long result = (long)(((long)registers[regA].GetValue()) + ((long)registers[regB].GetValue()));
                        registers[REX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot add a register of another size than 32 to a register of size 32");
                    }
                }
            }
            else if (instruction == ops.sub)
            {
                byte regA = mem.Read(++IP);
                byte regB = mem.Read(++IP);
                IP++;

                if (registers[regA] is ByteRegister)
                {
                    if (registers[regB] is ByteRegister)
                    {
                        byte result = (byte)(((byte)registers[regA].GetValue()) - ((byte)registers[regB].GetValue()));
                        registers[AX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot subract a register of another size than 8 to a register of size 8");
                    }
                }
                else if (registers[regA] is DoubleWordRegister)
                {
                    if (registers[regB] is DoubleWordRegister)
                    {
                        int result = (((int)registers[regA].GetValue()) - ((int)registers[regB].GetValue()));
                        registers[EAX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot subract a register of another size than 32 to a register of size 32");
                    }
                }
                else if (registers[regA] is QuadWordRegister)
                {
                    if (registers[regB] is QuadWordRegister)
                    {
                        long result = (long)(((long)registers[regA].GetValue()) - ((long)registers[regB].GetValue()));
                        registers[REX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot subract a register of another size than 32 to a register of size 32");
                    }
                }
            }
            else if (instruction == ops.mul)
            {
                byte regA = mem.Read(++IP);
                byte regB = mem.Read(++IP);
                IP++;

                if (registers[regA] is ByteRegister)
                {
                    if (registers[regB] is ByteRegister)
                    {
                        byte result = (byte)(((byte)registers[regA].GetValue()) * ((byte)registers[regB].GetValue()));
                        registers[AX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot multiply a register of another size than 8 to a register of size 8");
                    }
                }
                else if (registers[regA] is DoubleWordRegister)
                {
                    if (registers[regB] is DoubleWordRegister)
                    {
                        int result = (((int)registers[regA].GetValue()) * ((int)registers[regB].GetValue()));
                        registers[EAX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot multiply a register of another size than 32 to a register of size 32");
                    }
                }
                else if (registers[regA] is QuadWordRegister)
                {
                    if (registers[regB] is QuadWordRegister)
                    {
                        long result = (long)(((long)registers[regA].GetValue()) * ((long)registers[regB].GetValue()));
                        registers[REX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot multiply a register of another size than 32 to a register of size 32");
                    }
                }
            }
            else if (instruction == ops.div)
            {
                byte regA = mem.Read(++IP);
                byte regB = mem.Read(++IP);
                IP++;

                if (registers[regA] is ByteRegister)
                {
                    if (registers[regB] is ByteRegister)
                    {
                        byte result = (byte)(((byte)registers[regA].GetValue()) / ((byte)registers[regB].GetValue()));
                        registers[AX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot divide a register of another size than 8 to a register of size 8");
                    }
                }
                else if (registers[regA] is DoubleWordRegister)
                {
                    if (registers[regB] is DoubleWordRegister)
                    {
                        int result = (((int)registers[regA].GetValue()) / ((int)registers[regB].GetValue()));
                        registers[EAX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot divide a register of another size than 32 to a register of size 32");
                    }
                }
                else if (registers[regA] is QuadWordRegister)
                {
                    if (registers[regB] is QuadWordRegister)
                    {
                        long result = (long)(((long)registers[regA].GetValue()) / ((long)registers[regB].GetValue()));
                        registers[REX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot divide a register of another size than 32 to a register of size 32");
                    }
                }
            }
            else if (instruction == ops.mod)
            {
                byte regA = mem.Read(++IP);
                byte regB = mem.Read(++IP);
                IP++;

                if (registers[regA] is ByteRegister)
                {
                    if (registers[regB] is ByteRegister)
                    {
                        byte result = (byte)(((byte)registers[regA].GetValue()) % ((byte)registers[regB].GetValue()));
                        registers[AX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot modulus a register of another size than 8 to a register of size 8");
                    }
                }
                else if (registers[regA] is DoubleWordRegister)
                {
                    if (registers[regB] is DoubleWordRegister)
                    {
                        int result = (((int)registers[regA].GetValue()) % ((int)registers[regB].GetValue()));
                        registers[EAX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot modulus a register of another size than 32 to a register of size 32");
                    }
                }
                else if (registers[regA] is QuadWordRegister)
                {
                    if (registers[regB] is QuadWordRegister)
                    {
                        long result = (long)(((long)registers[regA].GetValue()) % ((long)registers[regB].GetValue()));
                        registers[REX].setvalue(result);
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot modulus a register of another size than 32 to a register of size 32");
                    }
                }
            }
            else if (instruction == ops.inc)
            {
                byte register = mem.Read(++IP);
                IP++;
                if (registers[register] is ByteRegister)
                {
                    byte b = (byte)registers[register].GetValue();
                    if (b + 1 <= 0xff)
                    {
                        b++;
                        registers[register].setvalue(b);
                    }
                }
                else if (registers[register] is DoubleWordRegister)
                {
                    int w = (int)registers[register].GetValue();
                    if (w + 1 <= int.MaxValue)
                    {
                        w++;
                        registers[register].setvalue(w);
                    }
                }
                else if (registers[register] is QuadWordRegister)
                {
                    long q = (long)registers[register].GetValue();
                    if (q + 1 <= long.MaxValue)
                    {
                        q++;
                        registers[register].setvalue(q);
                    }
                }
            }
            else if (instruction == ops.jmp)
            {
                uint address = (uint)mem.ReadDoubleWord(++IP);
                IP = address;
            }
            else if (instruction == ops.je)
            {
                uint address = (uint)mem.ReadDoubleWord(++IP);
                IP += 4;
                if (JUMP)
                {
                    IP = address;
                }
            }
            else if (instruction == ops.jne)
            {
                uint address = (uint)mem.ReadDoubleWord(++IP);
                IP += 4;
                if (!JUMP)
                {
                    IP = address;
                }
            }
            else if (instruction == ops.call)
            {
                uint calladdr = (uint)mem.ReadDoubleWord(++IP);
                IP += 4;
                uint retaddr = IP;
                callstack.Push(new Tuple<uint, uint>(calladdr, retaddr));
                IP = calladdr;
            }
            else if (instruction == ops.ret)
            {
                Tuple<uint, uint> top = callstack.Pop();
                IP = top.Item2;
            }
            else if (instruction == ops.push)
            {
                byte register = mem.Read(++IP);
                IP++;
                stack.Push(registers[register].GetValue());
            }
            else if (instruction == ops.pop)
            {
                byte register = mem.Read(++IP);
                IP++;
                if (registers[register] is ByteRegister)
                {
                    if (stack.Peek() is byte)
                    {
                        registers[register].setvalue((byte)stack.Pop());
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot set register of size 8 to any other size");
                    }
                }
                else if (registers[register] is DoubleWordRegister)
                {
                    if (stack.Peek() is int)
                    {
                        registers[register].setvalue((int)stack.Pop());
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot set register of size 32 to any other size");
                    }
                }
                else if (registers[register] is QuadWordRegister)
                {
                    if (stack.Peek() is long)
                    {
                        registers[register].setvalue((long)stack.Pop());
                    }
                    else
                    {
                        throw new InvalidRegisterSizeException("Cannot set register of size 64 to any other size");
                    }
                }
            }
            else if (instruction == ops.str)
            {
                byte register = mem.Read(++IP);
                uint address = (uint)mem.ReadDoubleWord(++IP);
                IP += 4;
                if (registers[register] is ByteRegister)
                {
                    mem.Write(address, (byte)registers[register].GetValue());
                }
                else if (registers[register] is DoubleWordRegister)
                {
                    mem.WriteDoubleWord(address, (int)registers[register].GetValue());
                }
                else if (registers[register] is QuadWordRegister)
                {
                    mem.WriteQuadWord(address, (int)registers[register].GetValue());
                }
            }
            else if (instruction == ops.lod)
            {
                byte register = mem.Read(++IP);
                uint address = (uint)mem.ReadDoubleWord(++IP);
                IP += 4;
                if (registers[register] is ByteRegister)
                {
                    registers[register].setvalue(mem.Read(address));
                }
                else if (registers[register] is DoubleWordRegister)
                {
                    registers[register].setvalue(mem.ReadDoubleWord(address));
                }
                else if (registers[register] is QuadWordRegister)
                {
                    registers[register].setvalue(mem.ReadQuadWord(address));
                }
            }
            else if (instruction == ops.doint)
            {
                byte interupt = mem.Read(++IP);
                IP++;
                interupts[interupt]();
            }
            else if (instruction == ops.cmp)
            {
                byte registera = mem.Read(++IP);
                byte towhat = mem.Read(++IP);
                if (towhat == 0x00)
                {
                    byte registerb = mem.Read(++IP);
                    IP++;
                    if (registers[registera] is ByteRegister && registers[registerb] is ByteRegister)
                    {
                        JUMP = (((byte)registers[registera].GetValue()) == ((byte)registers[registerb].GetValue()));
                    }
                    else if (registers[registera] is DoubleWordRegister && registers[registerb] is DoubleWordRegister)
                    {
                        JUMP = (((int)registers[registera].GetValue()) == ((int)registers[registerb].GetValue()));
                    }
                    else if (registers[registera] is QuadWordRegister && registers[registerb] is QuadWordRegister)
                    {
                        JUMP = (((long)registers[registera].GetValue()) == ((long)registers[registerb].GetValue()));
                    }
                }
                else if (towhat == 0x01)
                {
                    if (registers[registera] is ByteRegister)
                    {
                        byte val2 = mem.Read(++IP);
                        IP++;
                        JUMP = (((byte)registers[registera].GetValue()) == val2);
                    }
                    else if (registers[registera] is DoubleWordRegister)
                    {
                        int val2 = mem.ReadDoubleWord(++IP);
                        IP += 4;
                        JUMP = (((int)registers[registera].GetValue()) == val2);
                    }
                    else if (registers[registera] is QuadWordRegister)
                    {
                        long val2 = mem.ReadQuadWord(++IP);
                        IP += 8;
                        JUMP = (((long)registers[registera].GetValue()) == val2);
                    }
                }
            }
            else if (instruction == ops.end)
            {
                RUN = false;
            }
            return IP;
        }

        public void LoadProgram(byte[] contents)
        {
            mem = new Memory(2 * 1024 * 1024, contents);
        }

        public void AddProgram(byte[] contents, uint offset)
        {
            mem.WriteBytes(offset, contents);
        }

        internal void Run()
        {
            RUN = true;
            for (IP = 0; IP < mem.Length && RUN; )
            {
                DoInstruction();
            }
        }

        private void int1()
        {
        }

        private void int2()
        {
            if (((byte)registers[AH].GetValue()) == 0x01)
            {
                byte ch = (byte)registers[AL].GetValue();
                Console.Write((char)ch);
            }
            else if (((byte)registers[AH].GetValue()) == 0x02)
            {
                char ch = Console.ReadKey().KeyChar;
                registers[AL].setvalue((byte)ch);
            }
            else if (((byte)registers[AH].GetValue()) == 0x03)
            {
                byte x = (byte)registers[CH].GetValue();
                byte y = (byte)registers[CL].GetValue();
                Console.CursorTop = (int)x;
                Console.CursorLeft = (int)y;
            }
        }

        private void int3()
        {
        }

        private void int4()
        {
        }
    }

    //public class nThread
    //{
    //    Stack<int> stack;
    //    uint instructionpointer;

    //    internal void Cycle(VM vm)
    //    {
    //        instructionpointer++;
    //        instructionpointer = vm.DoInstruction(instructionpointer);
    //    }
    //}

    internal interface Register
    {
        object GetValue();

        void setvalue(object val);
    }

    internal class ByteRegister : Register
    {
        private byte b;

        public object GetValue()
        {
            return b;
        }

        public void setvalue(object val)
        {
            if (val is byte)
            {
                b = (byte)val;
            }
            else
            {
                throw new InvalidRegisterSizeException("Cannot set register of size 8 to any other size than 8");
            }
        }

        public override string ToString()
        {
            return "Byte: " + b.ToString("x");
        }
    }

    internal class DoubleWordRegister : Register
    {
        private int value;

        public object GetValue()
        {
            return value;
        }

        public void setvalue(object val)
        {
            if (val is int)
            {
                value = (int)val;
            }
            else if (val is byte)
            {
                value = (int)val;
            }
            else
            {
                throw new InvalidRegisterSizeException("Cannot set register of size 32 to any other size than 32,8");
            }
        }

        public override string ToString()
        {
            return "Double Word: " + value.ToString();
        }
    }

    internal class QuadWordRegister : Register
    {
        private long value;

        public object GetValue()
        {
            return value;
        }

        public void setvalue(object val)
        {
            if (val is long)
            {
                value = (long)val;
            }
            else if (val is int)
            {
                value = (int)val;
            }
            else if (val is byte)
            {
                value = (byte)val;
            }
            else
            {
                throw new InvalidRegisterSizeException("Cannot set register of size 64 to any other size than 64,32,8");
            }
        }

        public override string ToString()
        {
            return "Quad Word: " + value.ToString();
        }
    }

    [Serializable]
    public class InvalidRegisterSizeException : Exception
    {
        public InvalidRegisterSizeException() { }

        public InvalidRegisterSizeException(string message) : base(message) { }

        public InvalidRegisterSizeException(string message, Exception inner) : base(message, inner) { }

        protected InvalidRegisterSizeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public class Memory
    {
        private byte[] data;

        public int Length
        {
            get
            {
                return data.Length;
            }
        }

        public Memory(int size)
        {
            data = new byte[size];
        }

        public Memory(int p, byte[] contents)
        {
            this.data = new byte[p];
            if (contents.Length > data.Length)
            {
                throw new OutOfMemoryException("Not enough memory to load program of size: " + contents.Length + " bytes");
            }
            for (int i = 0; i < contents.Length && i < data.Length; i++)
            {
                data[i] = contents[i];
            }
        }

        public void Write(uint address, byte b)
        {
            data[address] = b;
        }

        public void WriteDoubleWord(uint address, int i)
        {
            byte[] bs = BitConverter.GetBytes(i);
            data[address] = bs[0];
            data[address + 1] = bs[1];
            data[address + 2] = bs[2];
            data[address + 3] = bs[3];
        }

        public void WriteQuadWord(uint address, long l)
        {
            byte[] bs = BitConverter.GetBytes(l);
            data[address] = bs[0];
            data[address + 1] = bs[1];
            data[address + 2] = bs[2];
            data[address + 3] = bs[3];
            data[address + 4] = bs[4];
            data[address + 5] = bs[5];
            data[address + 6] = bs[6];
            data[address + 7] = bs[7];
        }

        public void WriteString(uint address, string value)
        {
            foreach (char c in value)
            {
                Write(address, (byte)c);
                address++;
            }
        }

        public byte Read(uint address)
        {
            return data[address];
        }

        public int ReadDoubleWord(uint address)
        {
            return BitConverter.ToInt32(data, (int)address);
        }

        public long ReadQuadWord(uint address)
        {
            return BitConverter.ToInt64(data, (int)address);
        }

        public string ReadString(uint address)
        {
            StringBuilder str = new StringBuilder();
            while (data[address] != 0x00)
            {
                str.Append((char)data[address]);
                address++;
            }
            return str.ToString();
        }

        internal void WriteBytes(uint address, byte[] bytes)
        {
            if (bytes.Length > data.Length - address)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    data[i] = bytes[i];
                }
            }
        }

        internal byte[] GetBytes()
        {
            return data;
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