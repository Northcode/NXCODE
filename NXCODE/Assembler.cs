using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NXCODE
{
    class aLabel
    {
        internal string name;
        internal int index;

        internal string ToXML()
        {
            return "<label name=\"" + name + "\" address=\"" + index + "\"/>";
        }
    }

    class aClass
    {
        internal string name;
        internal List<aFunction> functions;
        internal List<aField> fields;

        internal string ToXML()
        {
            StringBuilder xml = new StringBuilder("<class name=\"" + name + "\">\n");
            foreach (aField f in fields)
            {
                xml.Append(f.ToXML() + "\n");
            }
            foreach (aFunction f in functions)
            {
                xml.Append(f.ToXML() + "\n");
            }
            xml.Append("</class>");
            return xml.ToString();
        }
    }

    class aField
    {
        internal string name;
        internal string access;
        
        internal string ToXML()
        {
            return "<field name=\"" + name + "\" access=\"" + access + "\" />";
        }
    }

    class aFunction
    {
        internal string name;
        internal int address;

        internal string ToXML()
        {
            return "<function name=\"" + name + "\" address=\"" + address + "\"/>";
        }
    }

    class aOpen
    {
        internal string name;

        internal string ToXML()
        {
            return "<open name=\"" + name + "\"/>";
        }
    }

    public class Assembler
    {
        internal const byte NXEHEADER = 0xfe;

        string name;
        string author;
        string labeldata;
        string classdata;
        byte[] bytecode;
        List<aClass> classes;
        List<aFunction> functions;
        List<aLabel> labels;
        List<aOpen> opens;
        int len = 0;

        public static Dictionary<string, byte> bytecodes;
        public static Dictionary<string, byte> exprcodes;

        string code;
        BinaryWriter stream;
        
        bool isClass = false;
        aClass cref = null;

        public Assembler(string name, string author, string code)
        {
            this.name = name;
            this.author = author;
            this.code = code;
            classes = new List<aClass>();
            functions = new List<aFunction>();
            labels = new List<aLabel>();
            opens = new List<aOpen>();
            PopulateByteCodes();
        }

        public static void PopulateByteCodes()
        {
            bytecodes = new Dictionary<string, byte>();
            exprcodes = new Dictionary<string, byte>();

            exprcodes.Add("string", 0x00);
            exprcodes.Add("int", 0x01);
            exprcodes.Add("double", 0x02);
            exprcodes.Add("char", 0x03);
            exprcodes.Add("bool", 0x04);
            exprcodes.Add("byte", 0x05);

            bytecodes.Add("push", 0x00);
            bytecodes.Add("pop", 0x01);
            bytecodes.Add("stloc", 0x02);
            bytecodes.Add("ldloc", 0x03);
            bytecodes.Add("call", 0x04);
            bytecodes.Add("ret", 0x05);
            bytecodes.Add("error", 0x06);
            bytecodes.Add("errorpop", 0x07);
            bytecodes.Add("int", 0x08);
            bytecodes.Add("newobj", 0x09);
            bytecodes.Add("con_print", 0x0a);
            bytecodes.Add("con_println", 0x0b);
            bytecodes.Add("con_read", 0x0c);
            bytecodes.Add("con_title", 0x0d);
            bytecodes.Add("add", 0x10);
            bytecodes.Add("sub", 0x11);
            bytecodes.Add("mul", 0x12);
            bytecodes.Add("div", 0x13);
            bytecodes.Add("mod", 0x14);
            bytecodes.Add("jmp", 0x15);
            bytecodes.Add("je", 0x16);
            bytecodes.Add("jn", 0x17);
            bytecodes.Add("stacksave", 0x18);
            bytecodes.Add("stackrestore", 0x19);
            bytecodes.Add("cmp", 0x20);
            bytecodes.Add("cls", 0x21);
            bytecodes.Add("cle", 0x22);
            bytecodes.Add("cgt", 0x23);
            bytecodes.Add("cge", 0x24);
            bytecodes.Add("str_concat", 0x25);
            bytecodes.Add("str_sub", 0x26);
            bytecodes.Add("str_subl", 0x27);
            bytecodes.Add("str_upp", 0x28);
            bytecodes.Add("str_low", 0x29);
            bytecodes.Add("threadnew", 0x2a);
            bytecodes.Add("threadstart", 0x2b);
            bytecodes.Add("threadend", 0x2c);
            bytecodes.Add("threadload", 0x2d);
            bytecodes.Add("entrypoint", 0x30);
            bytecodes.Add("terminate", 0x31);
            bytecodes.Add("callargs", 0x32);
            bytecodes.Add("starg", 0x33);
            bytecodes.Add("ldarg", 0x34);
            bytecodes.Add("str_cmp", 0x35);
            bytecodes.Add("newarray", 0x36);
            bytecodes.Add("arrayset", 0x37);
            bytecodes.Add("arrayget", 0x38);
            bytecodes.Add("threadsleep", 0x39);
            bytecodes.Add("threadpri", 0x3a);
            bytecodes.Add("str_split", 0x3b);
            bytecodes.Add("convert", 0x40);
        }

        public void Assemble()
        {
            MemoryStream mstream = new MemoryStream();
            stream = new BinaryWriter(mstream);
            len = 0;
            

            //COMPILE
            string newcode = code.Replace("\r", "");

            string[] lines = newcode.Split('\n');

            foreach (string l in lines)
            {
                ParseLine(l,stream);
                len += 1;
            }

            //SAVE
            int pos = (int)mstream.Position;
            byte[] rawcode = mstream.GetBuffer();
            bytecode = new byte[pos];

            for (int i = 0; i < pos; i++)
            {
                bytecode[i] = rawcode[i];
            }

            StringBuilder lbldb = new StringBuilder("<labels>\n");
            foreach (aLabel l in labels)
            {
                lbldb.Append(l.ToXML() + "\n");
            }
            lbldb.Append("</labels>");
            labeldata = lbldb.ToString();
            StringBuilder cdb = new StringBuilder("<meta>\n");
            foreach (aClass c in classes)
            {
                cdb.Append(c.ToXML() + "\n");
            }
            foreach (aFunction f in functions)
            {
                cdb.Append(f.ToXML() + "\n");
            }
            cdb.Append("</meta>\n");
            classdata = cdb.ToString();
        }

        public void Save(string Filename)
        {
            FileStream s = new FileStream(Filename, FileMode.OpenOrCreate);
            BinaryWriter w = new BinaryWriter(s);
            w.Write(NXEHEADER);
            w.Write(name);
            w.Write(author);
            w.Write(labeldata);
            w.Write(classdata);
            w.Write(bytecode);
            w.Write(0xff);
            w.Close();
            s.Close();
        }

        private void ParseLine(string l, BinaryWriter stream)
        {
            l = l.Trim();
            if (l.StartsWith("push "))
            {
                ParsePush(l, stream);
            }
            else if (l.Equals("pop"))
            {
                ParsePop(l, stream);
            }
            else if (l.StartsWith("stloc "))
            {
                ParseStloc(l, stream);
            }
            else if (l.StartsWith("ldloc "))
            {
                ParseLdloc(l, stream);
            }
            else if (l.StartsWith("call "))
            {
                ParseCall(l, stream);
            }
            else if (l.Equals("ret"))
            {
                ParseRet(l, stream);
            }
            else if (l.StartsWith("jmp "))
            {
                ParseJmp(l, stream);
            }
            else if (l.StartsWith("je "))
            {
                ParseJe(l, stream);
            }
            else if (l.StartsWith("jn "))
            {
                ParseJn(l, stream);
            }
            else if (l.StartsWith("error "))
            {
                ParseError(l, stream);
            }
            else if (l.Equals("errorpop"))
            {
                ParseErrorPop(l, stream);
            }
            else if (l.StartsWith("newobj "))
            {
                ParseNewObj(l, stream);
            }
            else if (l.StartsWith("int "))
            {
                ParseInterupt(l, stream);
            }
            else if (l.Equals("newarray"))
            {
                ParseNewArray(l, stream);
            }
            else if (l.Equals("arrayset"))
            {
                ParseArraySet(l, stream);
            }
            else if (l.Equals("arrayget"))
            {
                ParseArrayGet(l, stream);
            }
            else if (l.Equals("convert"))
            {
                ParseConvert(l, stream);
            }

                //Console
            else if (l.Equals("con_print"))
            {
                ParseConPrint(l,stream);
            }
            else if (l.Equals("con_println"))
            {
                ParseConPrintln(l, stream);
            }
            else if (l.Equals("con_read"))
            {
                ParseConRead(l, stream);
            }
            else if (l.Equals("con_title"))
            {
                ParseConTitle(l, stream);
            }

            //Math
            else if (l.Equals("add"))
            {
                ParseAdd(l, stream);
            }
            else if (l.Equals("sub"))
            {
                ParseSub(l, stream);
            }
            else if (l.Equals("mul"))
            {
                ParseMul(l, stream);
            }
            else if (l.Equals("div"))
            {
                ParseDiv(l, stream);
            }
            else if (l.Equals("mod"))
            {
                ParseMod(l, stream);
            }

            //Comparing
            else if (l.Equals("cmp"))
            {
                ParseCmp(l, stream);
            }
            else if (l.Equals("cls"))
            {
                ParseCls(l, stream);
            }
            else if (l.Equals("cle"))
            {
                ParseCle(l, stream);
            }
            else if (l.Equals("cgt"))
            {
                ParseCgt(l, stream);
            }
            else if (l.Equals("cge"))
            {
                ParseCge(l, stream);
            }

            //Stack Saving
            else if (l.Equals("stacksave"))
            {
                ParseStackSave(l, stream);
            }
            else if (l.Equals("stackrestore"))
            {
                ParseStackRestore(l, stream);
            }

            //Threading
            else if (l.Equals("threadnew"))
            {
                ParseThreadNew(l, stream);
            }
            else if (l.StartsWith("threadstart"))
            {
                ParseThreadStart(l, stream);
            }
            else if (l.StartsWith("threadend"))
            {
                ParseThreadEnd(l, stream);
            }
            else if (l.StartsWith("threadload"))
            {
                ParseThreadLoad(l, stream);
            }
            else if (l.Equals("threadsleep"))
            {
                ParseThreadSleep(l, stream);
            }
            else if (l.Equals("threadpri"))
            {
                ParseThreadPriority(l, stream);
            }

            //Program running
            else if (l.StartsWith("entrypoint "))
            {
                ParseEntryPoint(l, stream);
            }
            else if (l.Equals("terminate"))
            {
                ParseTerminate(l, stream);
            }

            //Args
            else if (l.StartsWith("callargs "))
            {
                ParseCallArgs(l, stream);
            }
            else if (l.StartsWith("starg "))
            {
                ParseStArg(l, stream);
            }
            else if (l.StartsWith("ldarg "))
            {
                ParseLdArg(l, stream);
            }

            //String functions
            else if (l.Equals("str_concat"))
            {
                ParseStrConcat(l, stream);
            }
            else if (l.Equals("str_cmp"))
            {
                ParseStrCmp(l, stream);
            }
            else if (l.Equals("str_sub"))
            {
                ParseStrSub(l, stream);
            }
            else if (l.Equals("str_subl"))
            {
                ParseStrSubl(l, stream);
            }
            else if (l.Equals("str_split"))
            {
                ParseStrSplit(l, stream);
            }

            //Special
            else if (l.StartsWith(":"))
            {
                string lname = l.Substring(1);
                aLabel lbl = new aLabel() { index = len, name = lname };
                labels.Add(lbl);
                len--;
            }
            else if (l.StartsWith("func "))
            {
                string fname = l.Substring(5);
                aFunction f = new aFunction() { name = fname, address = len };
                if (isClass)
                {
                    cref.functions.Add(f);
                }
                else
                {
                    functions.Add(f);
                }
                len--;
            }
            else if (l.StartsWith("class "))
            {
                string cname = l.Substring(6);
                aClass c = new aClass() { name = cname , functions = new List<aFunction>() , fields = new List<aField>() };
                classes.Add(c);
                isClass = true;
                cref = c;
                len--;
            }
            else if (l.StartsWith("open "))
            {
                string name = l.Substring(5);
                string code = File.ReadAllText(name);
                string newcode = code.Replace("\r", "");

                string[] lines = newcode.Split('\n');

                foreach (string line in lines)
                {
                    ParseLine(line, stream);
                    len += 1;
                }
                len--;
            }
            else if (l.StartsWith("var private "))
            {
                if (isClass)
                {
                    string name = l.Substring(12);
                    aField f = new aField() { name = name , access = "pri" };
                    cref.fields.Add(f);
                }
                len--;
            }
            else if (l.StartsWith("var public "))
            {
                if (isClass)
                {
                    string name = l.Substring(11);
                    aField f = new aField() { name = name , access = "pub" };
                    cref.fields.Add(f);
                }
                len--;
            }
            else if (l.Equals("end"))
            {
                if (isClass)
                {
                    isClass = false;
                }
                len--;
            }
            else
            {
                len--;
            }
        }

        private void ParseCallStatic(string l, BinaryWriter stream)
        {
            string method = l.Substring(11);
            stream.Write(bytecodes["callstatic"]);
            stream.Write(method);
        }

        private void ParseConvert(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["convert"]);
        }

        private void ParseThreadPriority(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["threadpri"]);
        }

        private void ParseThreadSleep(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["threadsleep"]);
        }

        private void ParseStrSplit(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["str_split"]);
        }

        private void ParseStrSubl(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["str_subl"]);
        }

        private void ParseStrSub(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["str_sub"]);
        }

        private void ParseStrCmp(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["str_cmp"]);
        }

        private void ParseStrConcat(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["str_concat"]);
        }

        private void ParseArrayGet(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["arrayget"]);
        }

        private void ParseArraySet(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["arrayset"]);
        }

        private void ParseNewArray(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["newarray"]);
        }

        private void ParseCge(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["cge"]);
        }

        private void ParseCgt(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["cgt"]);
        }

        private void ParseCle(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["cle"]);
        }

        private void ParseCls(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["cls"]);
        }

        private void ParseCmp(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["cmp"]);
        }

        private void ParseJn(string l, BinaryWriter stream)
        {
            string label = l.Substring(3);
            stream.Write(bytecodes["jn"]);
            stream.Write(label);
        }

        private void ParseJe(string l, BinaryWriter stream)
        {
            string label = l.Substring(3);
            stream.Write(bytecodes["je"]);
            stream.Write(label);
        }

        private void ParseJmp(string l, BinaryWriter stream)
        {
            string label = l.Substring(4);
            stream.Write(bytecodes["jmp"]);
            stream.Write(label);
        }

        private void ParseLdArg(string l, BinaryWriter stream)
        {
            int argn = Convert.ToInt32(l.Substring(6));
            stream.Write(bytecodes["ldarg"]);
            stream.Write(argn);
        }

        private void ParseStArg(string l, BinaryWriter stream)
        {
            int argn = Convert.ToInt32(l.Substring(6));
            stream.Write(bytecodes["starg"]);
            stream.Write(argn);
        }

        private void ParseCallArgs(string l, BinaryWriter stream)
        {
            int len = Convert.ToInt32(l.Substring(9));
            stream.Write(bytecodes["callargs"]);
            stream.Write(len);
        }

        private void ParseThreadLoad(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["threadload"]);
        }

        private void ParseThreadEnd(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["threadend"]);
        }

        private void ParseThreadStart(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["threadstart"]);
        }

        private void ParseThreadNew(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["threadnew"]);
        }

        private void ParseTerminate(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["terminate"]);
        }

        private void ParseEntryPoint(string l, BinaryWriter stream)
        {
            string method = l.Substring(11);
            stream.Write(bytecodes["entrypoint"]);
            stream.Write(method);
        }

        private void ParseInterupt(string l, BinaryWriter stream)
        {
            //int 
            string s = l.Substring(4);
            stream.Write(bytecodes["int"]);
            stream.Write(Convert.ToInt32(s));
        }

        private void ParseStackRestore(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["stackrestore"]);
        }

        private void ParseStackSave(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["stacksave"]);
        }

        private void ParseMod(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["mod"]);
        }

        private void ParseDiv(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["div"]);
        }

        private void ParseMul(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["mul"]);
        }

        private void ParseSub(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["sub"]);
        }

        private void ParseAdd(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["add"]);
        }

        private void ParseConTitle(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["con_title"]);
        }

        private void ParseConRead(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["con_read"]);
        }

        private void ParseConPrintln(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["con_println"]);
        }

        private void ParseConPrint(string l, BinaryWriter stream)
        {
            //con_print
            stream.Write(bytecodes["con_print"]);
        }

        private void ParseNewObj(string l, BinaryWriter stream)
        {
            //newobj 
            string type = l.Substring(7);
            stream.Write(bytecodes["newobj"]);
            stream.Write(type);
        }

        private void ParseErrorPop(string l, BinaryWriter stream)
        {
            //errorpop
            stream.Write(bytecodes["errorpop"]);
        }

        private void ParseError(string l, BinaryWriter stream)
        {
            //error 
            string[] m = l.Substring(6).Split(',');
            stream.Write(bytecodes["error"]);
            stream.Write(m[0]);
            stream.Write(m[1]);
        }

        private void ParseRet(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["ret"]);
        }

        private void ParseCall(string l, BinaryWriter stream)
        {
            //call 
            string method = l.Substring(5);
            stream.Write(bytecodes["call"]);
            stream.Write(method);
        }

        private void ParseLdloc(string l, BinaryWriter stream)
        {
            //ldloc 
            string name = l.Substring(6);
            stream.Write(bytecodes["ldloc"]);
            stream.Write(name);
        }

        private void ParseStloc(string l, BinaryWriter stream)
        {
            //stloc 
            string name = l.Substring(6);
            stream.Write(bytecodes["stloc"]);
            stream.Write(name);
        }

        private void ParsePop(string l, BinaryWriter stream)
        {
            stream.Write(bytecodes["pop"]);
        }

        private void ParsePush(string l, BinaryWriter stream)
        {
            //push 
            stream.Write(bytecodes["push"]);
            ParseExpr(l.Substring(5),stream);
        }

        private void ParseExpr(string l, BinaryWriter stream)
        {
            if (l.StartsWith("\'"))
            {
                string v = l.Substring(1, l.LastIndexOf('\'') - 1);
                stream.Write(exprcodes["string"]);
                stream.Write(v);
            }
            else if (l.StartsWith("\""))
            {
                string v = l.Substring(1, l.LastIndexOf('\"') - 1);
                stream.Write(exprcodes["string"]);
                stream.Write(v);
            }
            else if (char.IsDigit(l[0]) || l[0] == '-')
            {
                StringBuilder ci = new StringBuilder();
                for (int i = 0; i < l.Length; i++)
                {
                    ci.Append(l[i]);
                }
                int val = Convert.ToInt32(ci.ToString());

                stream.Write(exprcodes["int"]);
                stream.Write(val);
            }
            else if (l[0] == 'c')
            {
                char c = l[1];
                stream.Write(exprcodes["char"]);
                stream.Write(c);
            }
            else if (l[0] == 'd')
            {
                StringBuilder ci = new StringBuilder();
                int i = 0;
                for (i = 1; i < l.Length; i++)
                {
                    ci.Append(l[i]);
                }

                double val = Convert.ToDouble(ci.ToString());

                stream.Write(exprcodes["double"]);
                stream.Write(val);
            }
            else if (l[0] == 'b')
            {
                string s = l.Substring(1);
                byte val = Convert.ToByte(Int32.Parse(s, NumberStyles.HexNumber));

                stream.Write(exprcodes["byte"]);
                stream.Write(val);
            }
            else if (l == "true")
            {
                stream.Write(exprcodes["bool"]);
                stream.Write(true);
            }
            else if (l == "false")
            {
                stream.Write(exprcodes["bool"]);
                stream.Write(false);
            }
        }
    }
}
