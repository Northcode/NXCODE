using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NXCODE.Virtual_Machine
{
    //Standard
    class ret : cmd
    {
        public void Run(vThread from)
        {
            if (from.callstack.Count > 0)
            {
                from.instructionIndex = from.callstack.Pop().retaddress;
                from.RestoreGlobalVars();
            }
            else
            {
                from.End();
            }
        }
    }

    class stloc : cmd
    {
        public string name;

        public void Run(vThread from)
        {
            from.SetVar(name, from.stack.Pop());
        }
    }

    class ldloc : cmd
    {
        public string name;

        public void Run(vThread from)
        {
            from.stack.Push(from.GetVar(name));
        }
    }

    class push : cmd
    {
        public object val;

        public void Run(vThread from)
        {
            from.getStack().Push(val);
        }
    }

    class pop : cmd
    {
        public void Run(vThread from)
        {
            from.getStack().Pop();
        }
    }

    class call : cmd
    {
        public string method;

        public void Run(vThread from)
        {
            from.CALL(method);
        }
    }

    class staticcall : cmd
    {
        public string method;
        public void Run(vThread from)
        {
            from.CALLSTATIC(method);
        }
    }

    class error : cmd
    {
        public string title;
        public string msg;

        public void Run(vThread from)
        {
            from.getVM().Error(title, msg);
        }
    }

    class errorget : cmd
    {
        public void Run(vThread from)
        {
            from.stack.Push(from.parent.PopError());
        }
    }

    class newobj : cmd
    {
        public string type;

        public void Run(vThread from)
        {
            Instance i = from.GetClass(type).CreateInstance(from);
            from.getStack().Push(i);
        }
    }

    class Int : cmd
    {
        public int n;

        public void Run(vThread from)
        {
            from.Interupt(n);
        }
    }

    class newarray : cmd
    {
        public void Run(vThread from)
        {
            int len = (int)from.stack.Pop();
            vArray a = new vArray(len, from);
            from.stack.Push(a);
        }
    }

    class arrayget : cmd
    {
        public void Run(vThread from)
        {
            vArray array = from.getArg(0) as vArray;
            int index = (int)from.getArg(1);
            from.stack.Push(array.array[index]);
        }
    }

    class arrayset : cmd
    {
        public void Run(vThread from)
        {
            vArray array = from.getArg(0) as vArray;
            object val = from.stack.Pop();
            int index = (int)from.getArg(1);
            array.array[index] = val;
        }
    }

    class convert : cmd
    {
        public void Run(vThread from)
        {
            string to = from.stack.Pop() as string;
            object f = from.stack.Pop();
            object r = 0;

            if (f is string)
            {
                if (to == "string")
                {
                    r = f as string;
                }
                else if (to == "int")
                {
                    r = Convert.ToInt32(f as string);
                }
                else if (to == "double")
                {
                    r = Convert.ToDouble(f as string);
                }
                else if (to == "bool")
                {
                    r = Convert.ToBoolean(f as string);
                }
                else if (to == "char")
                {
                    char[] ca = (f as string).ToCharArray();
                    vArray a = new vArray(ca.Length, from);
                    object[] oa = new object[ca.Length];
                    for (int i = 0; i < ca.Length; i++)
                    {
                        oa[i] = (object)ca[i];
                    }
                    a.array = oa;
                    r = a;
                }
            }
            else if (f is int)
            {
                if (to == "string")
                {
                    r = Convert.ToString((int)f);
                }
                else if (to == "int")
                {
                    r = (int)f;
                }
                else if (to == "double")
                {
                    r = Convert.ToDouble((int)f);
                }
                else if (to == "bool")
                {
                    r = Convert.ToBoolean((int)f);
                }
                else if (to == "char")
                {
                    r = (char)((int)f);
                }
            }
            else if (f is double)
            {
                if (to == "string")
                {
                    r = Convert.ToString((double)f);
                }
                else if (to == "int")
                {
                    r = Convert.ToInt32((double)f);
                }
                else if (to == "double")
                {
                    r = (double)f;
                }
                else if (to == "bool")
                {
                    r = Convert.ToBoolean((double)f);
                }
                else if (to == "char")
                {
                    r = (char)(Convert.ToInt32((double)f));
                }
            }
            else if (f is bool)
            {
                if (to == "string")
                {
                    r = Convert.ToString((bool)f);
                }
                else if (to == "int")
                {
                    r = Convert.ToInt32((bool)f);
                }
                else if (to == "double")
                {
                    r = Convert.ToDouble((bool)f);
                }
                else if (to == "bool")
                {
                    r = (bool)f;
                }
                else if (to == "char")
                {
                    r = Convert.ToChar((bool)f);
                }
            }
            else if (f is char)
            {
                if (to == "string")
                {
                    r = Convert.ToString((char)f);
                }
                else if (to == "int")
                {
                    r = Convert.ToInt32((char)f);
                }
                else if (to == "double")
                {
                    r = Convert.ToDouble((char)f);
                }
                else if (to == "bool")
                {
                    r = Convert.ToBoolean((char)f);
                }
                else if (to == "char")
                {
                    r = (char)f;
                }
            }
            else if (f is vArray)
            {
                if (to == "string")
                {
                    r = ((vArray)f).CSTring();
                }
                else if (to == "int")
                {
                    r = ((vArray)f).CInt();
                }
                else if (to == "double")
                {
                    r = ((vArray)f).CDouble();
                }
                else if (to == "bool")
                {
                    r = ((vArray)f).CBool();
                }
                else if (to == "char")
                {
                    r = ((vArray)f).CChar();
                }
            }
            from.stack.Push(r);
        }
    }

    class funcptr : cmd
    {
        string method;

        public void Run(vThread from)
        {
            
        }
    }

    //Console
    class con_print : cmd
    {
        public void Run(vThread from)
        {
            Console.Write(from.stack.Pop());
        }
    }

    class con_println : cmd
    {
        public void Run(vThread from)
        {
            Console.WriteLine(from.getStack().Pop());
        }
    }

    class con_read : cmd
    {
        public void Run(vThread from)
        {
            from.getStack().Push(Console.ReadLine());
        }
    }

    class con_title : cmd
    {
        public void Run(vThread from)
        {
            string s = from.stack.Pop() as string;
            Console.Title = s;
        }
    }

    //Math
    class add : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object a = from.stack.Pop();
                object b = from.stack.Pop();

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        int c = ai + bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai + bd;
                        from.stack.Push(c);
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        double c = ai + bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai + bd;
                        from.stack.Push(c);
                    }
                }
                else if (a is string && b is string)
                {
                    from.stack.Push(((a as string) + (b as string)));
                }
            }
            else
            {
                from.parent.Error("Stack Empty", "Stack empty when trying to add");
            }
        }
    }

    class sub : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        int c = ai - bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai - bd;
                        from.stack.Push(c);
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        double c = ai - bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai - bd;
                        from.stack.Push(c);
                    }
                }
            }
            else
            {
                from.parent.Error("Stack Empty", "Stack empty when trying to add");
            }
        }
    }

    class mul : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object a = from.stack.Pop();
                object b = from.stack.Pop();

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        int c = ai * bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai * bd;
                        from.stack.Push(c);
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        double c = ai * bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai * bd;
                        from.stack.Push(c);
                    }
                }
            }
            else
            {
                from.parent.Error("Stack Empty", "Stack empty when trying to add");
            }
        }
    }

    class div : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        int c = ai / bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai / bd;
                        from.stack.Push(c);
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        double c = ai / bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai / bd;
                        from.stack.Push(c);
                    }
                }
            }
            else
            {
                from.parent.Error("Stack Empty", "Stack empty when trying to add");
            }
        }
    }

    class mod : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        int c = ai % bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai % bd;
                        from.stack.Push(c);
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        double c = ai % bi;
                        from.stack.Push(c);
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        double c = ai % bd;
                        from.stack.Push(c);
                    }
                }
            }
            else
            {
                from.parent.Error("Stack Empty", "Stack empty when trying to add");
            }
        }
    }

    //stack saving (yield return)
    class stacksave : cmd
    {
        public void Run(vThread from)
        {
            from.callstack.Peek().getMethod().SaveStack(from);
        }
    }

    class stackrestore : cmd
    {

        public void Run(vThread from)
        {
            from.callstack.Peek().getMethod().RestoreStack(from);
        }
    }

    //jumping
    class jmp : cmd
    {
        public string label;

        public void Run(vThread from)
        {
            from.Jmp(label);
        }
    }

    class je : cmd
    {
        public string label;

        public void Run(vThread from)
        {
            if (from.stack.Count > 0)
            {
                object o = from.stack.Pop();
                if (o is bool && ((bool)o) == true)
                {
                    from.Jmp(label);
                }
            }
        }
    }

    class jn : cmd
    {
        public string label;

        public void Run(vThread from)
        {
            if (from.stack.Count > 0)
            {
                object o = from.stack.Pop();
                if (o is bool && ((bool)o) == false)
                {
                    from.Jmp(label);
                }
            }
        }
    }

    //checking
    class cmp : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object a = from.stack.Pop();
                object b = from.stack.Pop();
                bool r = false;

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai == bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai == bd;
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai == bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai == bd;
                    }
                }
                else if ((a is string) && (b is string))
                {
                    r = (a as string) == (b as string);
                }
                from.stack.Push(r);
            }
        }
    }

    class cls : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();
                bool r = false;

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai < bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai < bd;
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai < bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai < bd;
                    }
                }
                from.stack.Push(r);
            }
        }
    }

    class cle : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();
                bool r = false;

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai <= bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai <= bd;
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai <= bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai <= bd;
                    }
                }
                from.stack.Push(r);
            }
        }
    }

    class cgt : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();
                bool r = false;

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai > bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai > bd;
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai > bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai > bd;
                    }
                }
                from.stack.Push(r);
            }
        }
    }

    class cge : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();
                bool r = false;

                if (a is int)
                {
                    int ai = (int)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai >= bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai >= bd;
                    }
                }
                else if (a is double)
                {
                    double ai = (double)a;
                    if (b is int)
                    {
                        int bi = (int)b;
                        r = ai >= bi;
                    }
                    else if (b is double)
                    {
                        double bd = (double)b;
                        r = ai >= bd;
                    }
                }
                from.stack.Push(r);
            }
        }
    }

    //String functions
    class str_concat : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();

                if (a is string && b is string)
                {
                    from.stack.Push((a as string) + (b as string));
                }
            }
        }
    }

    class str_sub : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object b = from.stack.Pop();
                object a = from.stack.Pop();

                if (a is string && b is int)
                {
                    from.stack.Push((a as string).Substring((int)b));
                }
            }
        }
    }

    class str_subl : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 2)
            {
                object c = from.stack.Pop();
                object b = from.stack.Pop();
                object a = from.stack.Pop();

                if (a is string && b is int && c is int)
                {
                    from.stack.Push((a as string).Substring((int)b,(int)c));
                }
            }
        }
    }

    class str_upp : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 0)
            {
                object o = from.stack.Pop();
                if (o is string)
                {
                    from.stack.Push((o as string).ToUpper());
                }
            }
        }
    }

    class str_low : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 0)
            {
                object o = from.stack.Pop();
                if (o is string)
                {
                    from.stack.Push((o as string).ToLower());
                }
            }
        }
    }

    class str_cmp : cmd
    {
        public void Run(vThread from)
        {
            if (from.stack.Count > 1)
            {
                object a = from.stack.Pop();
                object b = from.stack.Pop();
                bool r = false;

                if (a is string && b is string)
                {
                    r = (a as string) == (b as string);
                }

                from.stack.Push(r);
            }
        }
    }

    class str_split : cmd
    {
        public void Run(vThread from)
        {
            char c = (char)from.stack.Pop();
            string s = from.stack.Pop() as string;

            string[] arr = s.Split(c);

            vArray a = new vArray(arr.Length, from);
            a.array = arr;

            from.stack.Push(a);
        }
    }

    //Threading
    class threadnew : cmd
    {
        public void Run(vThread from)
        {
            string function = from.stack.Pop() as string;
            vThread t = new vThread(from.stack.Pop() as string, from.parent);
            t.startingInstruction = from.GetMethod(function).GetAddress();
            from.parent.AddThread(t);
            from.stack.Push(from.parent.ThreadCount - 1);
        }
    }

    class threadstart : cmd
    {
        public void Run(vThread from)
        {
            from.parent.Run((int)from.stack.Pop());
        }
    }

    class threadend : cmd
    {
        public void Run(vThread from)
        {
            from.parent.EndThread((int)from.stack.Pop());
        }
    }

    class threadload : cmd
    {
        public void Run(vThread from)
        {
            from.stack.Push(from.parent.GetThread((int)from.stack.Pop()));
        }
    }

    class threadsleep : cmd
    {
        public void Run(vThread from)
        {
            int l = (int)from.stack.Pop();
            Thread.Sleep(l);
        }
    }

    class threadpri : cmd
    {
        public void Run(vThread from)
        {
            string state = from.stack.Pop() as string;
            int thread = (int)from.stack.Pop();
            if (state == "low")
            {
                from.parent.GetThread(thread).SetPriority(ThreadPriority.Lowest);
            }
            else if (state == "normal")
            {
                from.parent.GetThread(thread).SetPriority(ThreadPriority.Normal);
            }
            else if (state == "high")
            {
                from.parent.GetThread(thread).SetPriority(ThreadPriority.Highest);
            }
        }
    }

    //Program running
    class Entrypoint : cmd
    {
        public string method;

        public void Run(vThread from)
        {
            from.CALL(method);
            from.End();
        }
    }

    class Terminate : cmd
    {
        public void Run(vThread from)
        {
            from.End();
        }
    }

    //Args
    class callargs : cmd
    {
        public int len;

        public void Run(vThread from)
        {
            from.newCallArgs(len);
        }
    }

    class starg : cmd
    {
        public int ind;

        public void Run(vThread from)
        {
            from.SetArg(ind, from.stack.Pop());
        }
    }

    class ldarg : cmd
    {
        public int ind;

        public void Run(vThread from)
        {
            from.stack.Push(from.getArg(ind));
        }
    }
}
