using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ncVM
{
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

        public string ReadString(uint address, out uint len)
        {
            StringBuilder str = new StringBuilder();
            uint l = 0;
            while (data[address] != 0x00)
            {
                str.Append((char)data[address]);
                address++;
                l++;
            }
            len = l;
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

        internal uint GetFreeAddress(int fieldlen)
        {
            int numblank = 0;
            uint addressref = 0;
            for (uint a = 0; a < data.Length; a++)
            {
                if (data[0] == 0x00)
                {
                    if (numblank == 0)
                    {
                        addressref = a;
                    }
                    numblank++;
                }
                else
                {
                    numblank = 0;
                }
                if (numblank >= fieldlen)
                {
                    break;
                }
            }
            if (numblank < fieldlen)
            {
                throw new OutOfMemoryException(String.Format("Not enough memory to allocate space for {0} bytes", fieldlen));
            }
            return addressref;
        }
    }
}