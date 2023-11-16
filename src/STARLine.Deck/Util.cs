using Hamilton.Interop.HxCoreLiquid;
using Hamilton.Interop.HxGruCommand;
using Hamilton.Interop.HxParams;
using Hamilton.Interop.HxTrace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Huarui.STARLine
{
    class Util
    {
        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();

        public static string FormatInt(int value, int length)
        {
            string l = value + "";
            while (l.Length < length)
                l = "0" + l;
            return l;
        }
        public static void ListLayout(Rack r)
        {
            Console.WriteLine(r.ID);
            if (r.Sites != null && r.Sites.Count > 0)
            {
                foreach (string s in r.Sites.Keys)
                {
                    Console.WriteLine("site " + s + " in " + r.ID);
                    foreach (Rack rx in r.Sites[s].Racks)
                    {
                        ListLayout(rx);
                    }
                }
            }
            if (r.Containers != null && r.Containers.Count > 0)
            {
                foreach (string c in r.Containers.Keys)
                {
                    Console.Write(c + ", ");
                }
                Console.WriteLine();
            }
        }

        static bool GetHxCommandKeys(int v, out object e)
        {
            e = null;
            Type t = typeof(HxCommandKeys);
            foreach (object k in t.GetEnumValues())
            {
                if (v == (int)k)
                {
                    e = k;
                    return true;
                }
            }
            return false;
        }
        static bool GetHxAtsInstrumentParsKeysKeys(int v, out object e)
        {
            e = null;
            Type t = typeof(HxAtsInstrumentParsKeys);
            foreach (object k in t.GetEnumValues())
            {
                if (v == (int)k)
                {
                    e = k;
                    return true;
                }
            }
            return false;
        }
        static bool GetHxTraceFormatKeys(int v, out object e)
        {
            e = null;
            Type t = typeof(HxCoreLiquidParsKeys);
            foreach (object k in t.GetEnumValues())
            {
                if (v == (int)k)
                {
                    e = k;
                    return true;
                }
            }
            return false;
        }
        static bool GetHxTraceStepStatus(int v, out object e)
        {
            e = null;
            Type t = typeof(HxTraceStepStatus);
            foreach (object k in t.GetEnumValues())
            {
                if (v == (int)k)
                {
                    e = k;
                    return true;
                }
            }
            return false;
        }
        public static void TraceIHxPars(IHxPars par, int level = 0)
        {
            String prefix = "";
            for (int j = 0; j < level; j++)
            {
                prefix += "  ";
            }
            if (par is IHxPars3)
            {
                IHxPars3 par3 = (IHxPars3)par;
                object[] keys = (object[])par3.GetKeys();
                for (int i = 0; i < keys.Length; i++)
                {
                    object value = par3.Item1(keys[i]);
                    object key = keys[i].ToString();
                    try
                    {
                        if (!GetHxCommandKeys(int.Parse(keys[i].ToString()), out key))
                        {
                            if(!GetHxAtsInstrumentParsKeysKeys(int.Parse(keys[i].ToString()), out key))
                            {
                                if(!GetHxTraceFormatKeys(int.Parse(keys[i].ToString()), out key))
                                {
                                }
                            }

                        }
                    }
                    catch (Exception e) { }
                    Console.Write("{0}{2}[{1}] = ", prefix, keys[i], key);
                    if (value is IHxPars3)
                    {
                        Console.WriteLine("{");
                        TraceIHxPars((IHxPars)value, level + 1);
                        Console.WriteLine("{0}{1}", prefix, "}");
                    }
                    else
                        Console.WriteLine("{0}"/*[{1}]"*/, value.ToString().Replace("\n", " "), value.GetType().ToString());
                }
            }
            else
            {
                Console.WriteLine("======================================");
            }
        }

        public static void ReleaseComObject(object obj)
        {
            if (obj == null)
                return;
            /*
            int count = 0;
            while ((Marshal.ReleaseComObject(obj)) > 0) ;
            {
                count++;
                if (count > 1000)
                    return;
            }
            */
            Marshal.FinalReleaseComObject(obj);
            obj = null;
        }
    }
}
