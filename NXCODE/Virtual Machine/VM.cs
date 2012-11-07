using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace NXCODE.Virtual_Machine
{
    public class VM
    {
        string ProgramName;
        string ProgramAuthor;
        internal List<cmd> code;
        List<vThread> threads;
        Stack<Error> errorstack;
        internal List<vClass> classes;
        internal List<vMethod> methods;
        internal Dictionary<string, int> labels;
        internal Dictionary<int, Interupt> ints;

        public VM()
        {
            threads = new List<vThread>();
            errorstack = new Stack<Error>();
            classes = new List<vClass>();
            methods = new List<vMethod>();
            labels = new Dictionary<string, int>();
            ints = new Dictionary<int, Interupt>();

            PopulateInterupts();
        }

        public void LoadProgram(string File)
        {
            FileStream s = new FileStream(File, FileMode.Open);
            BinaryReader r = new BinaryReader(s);
            if (r.ReadByte() == Assembler.NXEHEADER)
            {
                ProgramName = r.ReadString();
                ProgramAuthor = r.ReadString();
                string LabelXML = r.ReadString();
                string ClassXML = r.ReadString();
                LoadCode(r);
                LoadLabels(LabelXML);
                LoadClasses(ClassXML);
            }
            else
            {
                throw new Exception("The file is not a valid NXE file");
            }
        }

        private void LoadClasses(string ClassXML)
        {
            StringReader r = new StringReader(ClassXML);
            XmlReader xr = XmlReader.Create(r);
            xr.Read();

            if (xr.Name == "meta")
            {
                while (xr.Read() && xr.NodeType != XmlNodeType.EndElement)
                {
                    if (xr.Name == "class")
                    {
                        LoadClass(xr, xr.GetAttribute("name"));
                    }
                    else if (xr.Name == "function")
                    {
                        vMethod m = new vMethod(xr.GetAttribute("name"), Convert.ToInt32(xr.GetAttribute("address")));
                        methods.Add(m);
                    }
                }
            }
            xr.Close();
            r.Close();
        }

        private void LoadClass(XmlReader xr,string name)
        {
            vClass c = new vClass(name);
            while (xr.Read() && xr.NodeType != XmlNodeType.EndElement)
            {
                if (xr.Name == "field")
                {
                    Field f = new Field();
                    f.name = xr.GetAttribute("name");
                    f.access = (xr.GetAttribute("access") == "pub" ? true : false);
                    c.fields.Add(f);
                }
                else if (xr.Name == "function")
                {
                    vMethod m = new vMethod(xr.GetAttribute("name"), Convert.ToInt32(xr.GetAttribute("address")));
                    c.methods.Add(m);
                }
            }
            classes.Add(c);
        }

        private void LoadLabels(string LabelXML)
        {
            StringReader r = new StringReader(LabelXML);
            XmlReader xr = XmlReader.Create(r);

            xr.Read();
            if (xr.Name == "labels")
            {
                xr = xr.ReadSubtree();
                while (xr.Read())
                {
                    if (xr.Name == "label")
                    {
                        labels.Add(xr.GetAttribute("name"), Convert.ToInt32(xr.GetAttribute("address")));
                    }
                }
            }
            xr.Close();
            r.Close();
        }

        void LoadCode(BinaryReader r)
        {
            List<cmd> cmds = new List<cmd>();
            byte opcode = 0x00;
            while (opcode != 0xff)
            {
                opcode = r.ReadByte();
                if (opcode == Assembler.bytecodes["push"])
                {
                    cmds.Add(new push() { val = LoadExpr(r) });
                }
                else if (opcode == Assembler.bytecodes["pop"])
                {
                    cmds.Add(new pop());
                }
                else if (opcode == Assembler.bytecodes["stloc"])
                {
                    cmds.Add(new stloc() { name = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["ldloc"])
                {
                    cmds.Add(new ldloc() { name = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["call"])
                {
                    cmds.Add(new call() { method = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["ret"])
                {
                    cmds.Add(new ret());
                }
                else if (opcode == Assembler.bytecodes["jmp"])
                {
                    cmds.Add(new jmp() { label = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["je"])
                {
                    cmds.Add(new je() { label = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["jn"])
                {
                    cmds.Add(new jn() { label = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["newobj"])
                {
                    cmds.Add(new newobj() { type = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["newarray"])
                {
                    cmds.Add(new newarray());
                }
                else if (opcode == Assembler.bytecodes["arrayget"])
                {
                    cmds.Add(new arrayget());
                }
                else if (opcode == Assembler.bytecodes["arrayset"])
                {
                    cmds.Add(new arrayset());
                }
                else if (opcode == Assembler.bytecodes["con_print"])
                {
                    cmds.Add(new con_print());
                }
                else if (opcode == Assembler.bytecodes["con_println"])
                {
                    cmds.Add(new con_println());
                }
                else if (opcode == Assembler.bytecodes["con_read"])
                {
                    cmds.Add(new con_read());
                }
                else if (opcode == Assembler.bytecodes["con_title"])
                {
                    cmds.Add(new con_title());
                }
                else if (opcode == Assembler.bytecodes["error"])
                {
                    cmds.Add(new error() { title = r.ReadString(), msg = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["errorpop"])
                {
                    cmds.Add(new errorget());
                }
                else if (opcode == Assembler.bytecodes["int"])
                {
                    cmds.Add(new Int() { n = r.ReadInt32() });
                }
                else if (opcode == Assembler.bytecodes["entrypoint"])
                {
                    cmds.Add(new Entrypoint() { method = r.ReadString() });
                }
                else if (opcode == Assembler.bytecodes["terminate"])
                {
                    cmds.Add(new Terminate());
                }
                else if (opcode == Assembler.bytecodes["convert"])
                {
                    cmds.Add(new convert());
                }

                //Threading
                else if (opcode == Assembler.bytecodes["threadnew"])
                {
                    cmds.Add(new threadnew());
                }
                else if (opcode == Assembler.bytecodes["threadstart"])
                {
                    cmds.Add(new threadstart());
                }
                else if (opcode == Assembler.bytecodes["threadend"])
                {
                    cmds.Add(new threadend());
                }
                else if (opcode == Assembler.bytecodes["threadload"])
                {
                    cmds.Add(new threadload());
                }
                else if (opcode == Assembler.bytecodes["threadsleep"])
                {
                    cmds.Add(new threadsleep());
                }
                else if (opcode == Assembler.bytecodes["threadpri"])
                {
                    cmds.Add(new threadpri());
                }

                //StackSaveing
                else if (opcode == Assembler.bytecodes["stacksave"])
                {
                    cmds.Add(new stacksave());
                }
                else if (opcode == Assembler.bytecodes["stackrestore"])
                {
                    cmds.Add(new stackrestore());
                }

                //MATH
                else if (opcode == Assembler.bytecodes["add"])
                {
                    cmds.Add(new add());
                }
                else if (opcode == Assembler.bytecodes["sub"])
                {
                    cmds.Add(new sub());
                }
                else if (opcode == Assembler.bytecodes["mul"])
                {
                    cmds.Add(new mul());
                }
                else if (opcode == Assembler.bytecodes["div"])
                {
                    cmds.Add(new div());
                }
                else if (opcode == Assembler.bytecodes["mod"])
                {
                    cmds.Add(new mod());
                }

                //CMP
                else if (opcode == Assembler.bytecodes["cmp"])
                {
                    cmds.Add(new cmp());
                }
                else if (opcode == Assembler.bytecodes["cls"])
                {
                    cmds.Add(new cls());
                }
                else if (opcode == Assembler.bytecodes["cle"])
                {
                    cmds.Add(new cle());
                }
                else if (opcode == Assembler.bytecodes["cgt"])
                {
                    cmds.Add(new cgt());
                }
                else if (opcode == Assembler.bytecodes["cge"])
                {
                    cmds.Add(new cge());
                }

                //String functions
                else if (opcode == Assembler.bytecodes["str_concat"])
                {
                    cmds.Add(new str_concat());
                }
                else if (opcode == Assembler.bytecodes["str_cmp"])
                {
                    cmds.Add(new str_cmp());
                }
                else if (opcode == Assembler.bytecodes["str_sub"])
                {
                    cmds.Add(new str_sub());
                }
                else if (opcode == Assembler.bytecodes["str_subl"])
                {
                    cmds.Add(new str_subl());
                }
                else if (opcode == Assembler.bytecodes["str_split"])
                {
                    cmds.Add(new str_split());
                }

                //Args
                else if (opcode == Assembler.bytecodes["callargs"])
                {
                    cmds.Add(new callargs() { len = r.ReadInt32() });
                }
                else if (opcode == Assembler.bytecodes["starg"])
                {
                    cmds.Add(new starg() { ind = r.ReadInt32() });
                }
                else if (opcode == Assembler.bytecodes["ldarg"])
                {
                    cmds.Add(new ldarg() { ind = r.ReadInt32() });
                }
            }

            code = cmds;
        }

        private object LoadExpr(BinaryReader r)
        {
            byte ecode = r.ReadByte();
            if (ecode == Assembler.exprcodes["string"])
            {
                return r.ReadString();
            }
            else if (ecode == Assembler.exprcodes["int"])
            {
                return r.ReadInt32();
            }
            else if (ecode == Assembler.exprcodes["double"])
            {
                return r.ReadDouble();
            }
            else if (ecode == Assembler.exprcodes["bool"])
            {
                return r.ReadBoolean();
            }
            else if (ecode == Assembler.exprcodes["char"])
            {
                return r.ReadChar();
            }
            else
            {
                return null;
            }
        }

        private void PopulateInterupts()
        {
            ints.Add(1, new Interupt1());
            ints.Add(2, new Interupt2());
            ints.Add(3, new Interupt3());
        }

        public void Error(string title, string message)
        {
            Error e = new Error() { Title = title, Message = message };
            errorstack.Push(e);
        }

        public void Run(int thread)
        {
            threads[thread].Start();
        }

        public void NewThread(string Name)
        {
            vThread t = new vThread(Name, this);
            t.startingInstruction = 0;
            threads.Add(t);
        }

        internal void AddThread(vThread t)
        {
            threads.Add(t);
        }

        public int ThreadCount { get { return threads.Count; } }

        internal void EndThread(int p)
        {
            threads[p].End();
        }

        internal int GetThreadIndex(string p)
        {
            for (int i = 0; i < threads.Count; i++)
            {
                if (threads[i].Name == p)
                {
                    return i;
                }
            }
            return 0;
        }

        internal object PopError()
        {
            Error e = errorstack.Pop();
            return e.Title + " - " + e.Message;
        }

        internal void Interupt(int n, Stack<object> stack, Dictionary<string, object> globalvars, object[] callargs)
        {
            if (ints.ContainsKey(n))
            {
                ints[n].Do(stack, globalvars, callargs);
            }
        }

        internal void AddInterupt(int key, Interupt i)
        {
            ints.Add(key, i);
        }

        internal void Stop()
        {
            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].End();
                Thread.Sleep(20);
            }
        }

        internal vThread GetThread(int p)
        {
            return threads[p];
        }
    }

    public class vThread
    {
        internal string Name;
        internal VM parent;
        Thread b;

        internal Stack<object> stack;
        internal Stack<vCall> callstack;
        Dictionary<string, object> globalvars;
        Dictionary<string, object> globalvarSave;
        object[] callargs;

        internal int instructionIndex;
        internal int startingInstruction;
        bool running;

        public vThread(string Name, VM parent)
        {
            this.Name = Name;
            this.parent = parent;
            this.b = new Thread(new ThreadStart(Run));
            b.Name = Name;
            b.Priority = ThreadPriority.Normal;
            stack = new Stack<object>();
            callstack = new Stack<vCall>();
            globalvars = new Dictionary<string, object>();
            callargs = new object[0];
            instructionIndex = 0;
        }

        internal void SetPriority(ThreadPriority p)
        {
            b.Priority = p;
        }

        void Run()
        {
            running = true;
            for (instructionIndex = startingInstruction; (instructionIndex < parent.code.Count) && running; instructionIndex++)
            {
                parent.code[instructionIndex].Run(this);
            }
        }

        internal void SetVar(string name, object value)
        {
            if (name.Contains('.'))
            {
                try
                {
                    if (globalvars.ContainsKey(name.Split('.')[0]))
                    {
                        if (globalvars[name.Split('.')[0]] is Instance)
                        {
                            (globalvars[name.Split('.')[0]] as Instance).SetVar(name.Substring(name.IndexOf('.') + 1), value);
                        }
                        else if (globalvars[name.Split('.')[0]] is vArray)
                        {
                            (globalvars[name.Split('.')[0]] as vArray).SetValue(name.Substring(name.IndexOf('.') + 1), value);
                        }
                        else
                        {
                            parent.Error("Var not found!", "Could not find variable: " + name + " in globalvars of thread: " + this.Name);
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                if (globalvars.ContainsKey(name))
                {
                    globalvars[name] = value;
                }
                else
                {
                    globalvars.Add(name, value);
                }
            }
        }

        internal object GetVar(string name)
        {
            if (name.Contains('.'))
            {
                try
                {
                    if (globalvars.ContainsKey(name.Split('.')[0]))
                    {
                        if (globalvars[name.Split('.')[0]] is Instance)
                        {
                            return (globalvars[name.Split('.')[0]] as Instance).GetVar(name.Substring(name.IndexOf('.') + 1));
                        }
                        else if (globalvars[name.Split('.')[0]] is vArray)
                        {
                            return (globalvars[name.Split('.')[0]] as vArray).GetValue(name.Substring(name.IndexOf('.') + 1));
                        }
                        else
                        {
                            parent.Error("Var not found!", "Could not find variable: " + name + " in globalvars of thread: " + this.Name);
                        }
                    }
                }
                catch
                {
                }
                return null;
            }
            else
            {
                if (globalvars.ContainsKey(name))
                {
                    return globalvars[name];
                }
                else
                {
                    parent.Error("Var not found!", "Could not find variable: " + name + " in globalvars of thread: " + this.Name);
                    return null;
                }
            }
        }

        internal VM getVM()
        {
            return parent;
        }

        internal Stack<object> getStack()
        {
            return stack;
        }
        
        internal void Start()
        {
            b.Start();
        }

        internal vClass GetClass(string type)
        {
            foreach (vClass c in parent.classes)
            {
                if (c.getName() == type)
                {
                    return c;
                }
            }
            return null;
        }

        internal object[] getCallArgs()
        {
            return callargs;
        }

        internal void CALL(string name)
        {
            if (name.Contains('.'))
            {
                try
                {
                    if (globalvars.ContainsKey(name.Split('.')[0]))
                    {
                        if (globalvars[name.Split('.')[0]] is Instance)
                        {
                            (globalvars[name.Split('.')[0]] as Instance).Call(name.Substring(name.IndexOf('.') + 1));
                        }
                        else
                        {
                            parent.Error("Var not found!", "Could not find variable: " + name + " in globalvars of thread: " + this.Name);
                        }
                    }
                    else if (ContainsClass(name.Split('.')[0]))
                    {
                        GetClass(name.Split('.')[0]).Call(name.Substring(name.IndexOf('.') + 1),this);
                    }
                }
                catch
                {
                }
            }
            else
            {
                if (globalvars.ContainsKey(name))
                {
                    if (globalvars[name] is FuncPtr)
                    {
                        int adrs = (globalvars[name] as FuncPtr).address;
                        vCall c = new vCall();
                        c.startaddress = adrs;
                        c.retaddress = instructionIndex;
                        c.referance = new vMethod("Temp",adrs);
                        callstack.Push(c);
                        c.Run(this);
                    }
                }
                else
                {
                    vMethod m = GetMethod(name);
                    if (m != null)
                    {
                        vCall c = new vCall();
                        c.startaddress = m.GetAddress();
                        c.retaddress = instructionIndex;
                        c.referance = m;
                        callstack.Push(c);
                        c.Run(this);
                    }
                    else
                    {
                        parent.Error("Method not found!", "Could not find method: " + name + " in global method list.");
                    }
                }
            }
        }

        private bool ContainsClass(string p)
        {
            bool c = false;
            foreach (vClass v in parent.classes)
            {
                if (v.getName() == p)
                {
                    c = true;
                    break;
                }
            }
            return c;
        }

        internal void CALLSTATIC(string name)
        {
            if (name.Contains('.'))
            {
                try
                {
                    string[] cn = name.Split('.');
                    vClass c = GetClass(cn[0]);
                    vMethod m = c.getMethod(name.Substring(name.IndexOf('.') + 1));
                    vCall ca = new vCall();
                    ca.referance = m;
                    ca.retaddress = instructionIndex;
                    ca.startaddress = m.GetAddress();
                    callstack.Push(ca);
                    ca.Run(this);
                }
                catch
                {
                }
            }
            else
            {
                vMethod m = GetMethod(name);
                if (m != null)
                {
                    vCall c = new vCall();
                    c.startaddress = m.GetAddress();
                    c.retaddress = instructionIndex;
                    c.referance = m;
                    callstack.Push(c);
                    c.Run(this);
                }
                else
                {
                    parent.Error("Method not found!", "Could not find method: " + name + " in global method list.");
                }
            }
        }

        internal vMethod GetMethod(string name)
        {
            foreach (vMethod m in parent.methods)
            {
                if (m.Name == name)
                {
                    return m;
                }
            }
            return null;
        }

        internal bool HasMethod(string p)
        {
            bool result = false;
            foreach (vMethod m in parent.methods)
            {
                if (m.Name == p)
                {
                    result = true;
                }
            }
            return result;
        }

        internal void SaveGlobalVars()
        {
            globalvarSave = globalvars;
        }

        internal void SetGlobalVars(Dictionary<string, object> fields)
        {
            globalvars = fields;
        }

        internal void RestoreGlobalVars()
        {
            if (globalvarSave != null)
            {
                globalvars = globalvarSave;
            }
        }

        internal void End()
        {
            running = false;
        }

        internal void Jmp(string label)
        {
            if (parent.labels.ContainsKey(label))
            {
                instructionIndex = parent.labels[label] - 1;
            }
        }

        internal void Interupt(int n)
        {
            parent.Interupt(n, stack, globalvars, callargs);
        }

        internal void newCallArgs(int len)
        {
            callargs = new object[len];
        }

        internal void SetArg(int ind, object p)
        {
            callargs[ind] = p;
        }

        internal object getArg(int ind)
        {
            return callargs[ind];
        }

        
    }

    public class Field
    {
        internal bool access;
        internal string name;
    }

    public class vClass
    {
        string Name;
        internal List<vMethod> methods;
        internal List<Field> fields;

        public vClass(string Name)
        {
            this.Name = Name;
            methods = new List<vMethod>();
            fields = new List<Field>();
        }

        internal string getName()
        {
            return Name;
        }

        internal Instance CreateInstance(vThread p)
        {
            Instance i = new Instance(this,p);
            foreach (Field str in fields)
            {
                i.AddVar(str.name, null);
            }
            return i;
        }

        internal Field getField(string name)
        {
            foreach (Field f in fields)
            {
                if (f.name == name)
                {
                    return f;
                }
            }
            return null;
        }

        internal vMethod getMethod(string p)
        {
            foreach (vMethod m in methods)
            {
                if (m.Name == p)
                {
                    return m;
                }
            }
            return null;
        }

        internal void Call(string p, vThread from)
        {
            vMethod m = getMethod(p);
            if (m != null)
            {
                vCall c = new vCall();
                c.referance = m;
                c.retaddress = from.instructionIndex;
                c.startaddress = m.GetAddress();
                from.callstack.Push(c);
                from.callstack.Peek().Run(from);
            }
        }
    }

    public class Instance
    {
        vClass type;
        Dictionary<string, object> fields;
        vThread parent;

        internal Instance(vClass b, vThread p)
        {
            type = b;
            fields = new Dictionary<string, object>();
            parent = p;
        }

        internal vClass getBase()
        {
            return type;
        }

        internal Dictionary<string, object> getFields()
        {
            return fields;
        }

        internal void AddVar(string name, object value)
        {
            fields.Add(name, value);
        }

        internal void SetVar(string name, object value)
        {
            if (name.Contains('.'))
            {
                try
                {
                    if (fields.ContainsKey(name.Split('.')[0]))
                    {
                        if (fields[name.Split('.')[0]] is Instance)
                        {
                            (fields[name.Split('.')[0]] as Instance).SetVar(name.Substring(name.IndexOf('.') + 1), value);
                        }
                        else if (fields[name.Split('.')[0]] is vArray)
                        {
                            (fields[name.Split('.')[0]] as vArray).SetValue(name.Substring(name.IndexOf('.') + 1), value);
                        }
                        else
                        {
                            parent.parent.Error("Var not found!", "Could not find var: " + name.Split('.')[0] + " in class: " + type.getName());
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                if (fields.ContainsKey(name))
                {
                    if (type.getField(name).access)
                    {
                        fields[name] = value;
                    }
                    else
                    {
                        parent.parent.Error("Variable not public", "The variable: " + name + " is not public and cannot be edited");
                    }
                }
            }
        }

        internal object GetVar(string name)
        {
            if (name.Contains('.'))
            {
                try
                {
                    if (fields.ContainsKey(name.Split('.')[0]))
                    {
                        if (fields[name.Split('.')[0]] is Instance)
                        {
                            return (fields[name.Split('.')[0]] as Instance).GetVar(name.Substring(0, name.IndexOf('.')));
                        }
                        else if (fields[name.Split('.')[0]] is vArray)
                        {
                            return (fields[name.Split('.')[0]] as vArray).GetValue(name.Substring(name.IndexOf('.') + 1));
                        }
                        else
                        {
                            parent.parent.Error("Var not found!", "Could not find var: " + name.Split('.')[0] + " in class: " + type.getName());
                        }
                    }
                }
                catch
                {
                }
                return null;
            }
            else
            {
                if (fields.ContainsKey(name))
                {
                    if (type.getField(name).access)
                    {
                        return fields[name];
                    }
                    else
                    {
                        parent.parent.Error("Variable not public", "The variable: " + name + " is not public and cannot be referanced");
                        return "ERROR";
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        internal void Call(string name)
        {
            if (name.Contains('.'))
            {
                try
                {
                    if (fields.ContainsKey(name.Split('.')[0]))
                    {
                        if (fields[name.Split('.')[0]] is Instance)
                        {
                            (fields[name.Split('.')[0]] as Instance).Call(name.Substring(name.IndexOf('.') + 1));
                        }
                        else
                        {
                            parent.parent.Error("Var not found!", "Could not find var: " + name.Split('.')[0] + " in class: " + type.getName());
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                vMethod m = GetMethod(name);
                if (m != null)
                {
                    vCall c = new vCall();
                    c.startaddress = m.GetAddress();
                    c.retaddress = parent.instructionIndex;
                    c.referance = m;
                    parent.callstack.Push(c);
                    parent.SaveGlobalVars();
                    parent.SetGlobalVars(fields);
                    c.Run(parent);
                }
                else
                {
                    parent.parent.Error("Method not found!", "Could not find method: " + name + " in class: " + type.getName());
                }
            }
        }

        private vMethod GetMethod(string name)
        {
            foreach (vMethod m in type.methods)
            {
                if (m.Name == name)
                {
                    return m;
                }
            }
            return null;
        }
    }

    public class FuncPtr
    {
        public int address;
    }

    public class vArray
    {
        internal object[] array;
        internal vThread parent;

        public vArray(int size, vThread parent)
        {
            array = new object[size];
            this.parent = parent;
        }

        internal void SetValue(string name, object value)
        {
            if (name.Contains('.'))
            {
                try
                {
                    int ind = Convert.ToInt32(name.Split('.')[0]);
                    if (array.Length > ind)
                    {
                        if (array[ind] is Instance)
                        {
                            (array[ind] as Instance).SetVar(name.Substring(name.IndexOf('.') + 1), value);
                        }
                        else if (array[ind] is vArray)
                        {
                            (array[ind] as vArray).SetValue(name.Substring(name.IndexOf('.') + 1), value);
                        }
                        else
                        {
                            parent.parent.Error("Var not found!", "Could not find var: " + name.Split('.')[0] + " in array");
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                int ind = Convert.ToInt32(name.Split('.')[0]);
                if (array.Length > ind)
                {
                    array[ind] = value;
                }
            }
        }

        internal object GetValue(string name)
        {
            if (name.Contains('.'))
            {
                try
                {
                    int ind = Convert.ToInt32(name.Split('.')[0]);
                    if (array.Length > ind)
                    {
                        if (array[ind] is Instance)
                        {
                            return (array[ind] as Instance).GetVar(name.Substring(name.IndexOf('.') + 1));
                        }
                        else if (array[ind] is vArray)
                        {
                            return (array[ind] as vArray).GetValue(name.Substring(name.IndexOf('.') + 1));
                        }
                        else
                        {
                            parent.parent.Error("Var not found!", "Could not find var: " + name.Split('.')[0] + " in array");
                        }
                    }
                }
                catch
                {
                }
                return null;
            }
            else
            {
                int ind = Convert.ToInt32(name);
                if (array.Length > ind)
                {
                    return array[ind];
                }
                else
                {
                    return null;
                }
            }
        }

        internal string CSTring()
        {
            StringBuilder s = new StringBuilder("");
            foreach (object o in array)
            {
                s.Append(Convert.ToString(o));
            }
            return s.ToString();
        }

        internal int CInt()
        {
            int s = 0;
            foreach (object o in array)
            {
                s += Convert.ToInt32(o);
            }
            return s;
        }

        internal double CDouble()
        {
            double s = 0;
            foreach (object o in array)
            {
                s += Convert.ToDouble(o);
            }
            return s;
        }

        internal bool CBool()
        {
            bool s = Convert.ToBoolean(array[0]);
            return s;
        }

        internal char CChar()
        {
            char c = Convert.ToChar(array[0]);
            return c;
        }
    }

    public class vMethod
    {
        string name;
        Stack<object> currentstack;
        int address;

        public vMethod(string Name, int address)
        {
            name = Name;
            currentstack = null;
            this.address = address;
        }

        public string Name
        {
            get { return name; }
        }

        internal int GetAddress()
        {
            return address;
        }

        internal void SaveStack(vThread from)
        {
            currentstack = new Stack<object>(from.stack);
        }

        internal void RestoreStack(vThread from)
        {
            from.stack = currentstack;
        }
    }

    public class vCall
    {
        internal int startaddress;
        internal int retaddress;
        internal vMethod referance;

        internal void Run(vThread f)
        {
            f.instructionIndex = startaddress - 1;
        }

        internal vMethod getMethod()
        {
            return referance;
        }
    }

    public interface Interupt
    {
        void Do(Stack<object> stack, Dictionary<string, object> vars, object[] callargs);
    }

    public class Error
    {
        public string Title;
        public string Message;
    }

    public interface cmd
    {
        void Run(vThread from);
    }
}
