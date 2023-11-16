using Hamilton.Interop.HxReg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Venus intallation information
    /// </summary>
    public class STARRegistry : IDisposable
    {
        static HxRegistry registry;
        private static HxRegistry Reg
        {
            get
            {
                if(registry==null)
                {
                    try { registry = new HxRegistry(); }catch(Exception e) { }
                }
                return registry;
            }
        }
        /// <summary>
        /// Construction
        /// </summary>
        private STARRegistry()
        {
        }
        /// <summary>
        /// venus application folder (bin)
        /// </summary>
        public static string BinPath {
            get
            {
                return Reg?.BinPath;
            }
        }
        /// <summary>
        /// Venus Application config folder
        /// </summary>
        public static string ConfigPath
        {
            get
            {
                return Reg?.ConfigPath;
            }
        }
        /// <summary>
        /// Venus methods folder
        /// </summary>
        public static string MethodsPath
        {
            get
            {
                return Reg?.MethodsPath;
            }
        }
        /// <summary>
        /// Venus LogFiles folder
        /// </summary>
        public static string LogFilesPath
        {
            get
            {
                return Reg?.LogFilesPath;
            }
        }
        /// <summary>
        /// Venus Labware folder
        /// </summary>
        public static string LabwarePath
        {
            get
            {
                return Reg?.LabwarePath;
            }
        }
        /// <summary>
        /// Venus Language folder
        /// </summary>
        public static string LanguagePath
        {
            get
            {
                return Reg?.LanguagePath;
            }
        }
        /// <summary>
        /// Venus Graphics folder
        /// </summary>
        public static string GraphicPath
        {
            get
            {
                return Reg?.GraphicPath;
            }
        }
        /// <summary>
        /// Help folder
        /// </summary>
        public static string HelpPath
        {
            get
            {
                return Reg?.HelpPath;
            }
        }
        /// <summary>
        /// Free the object
        /// </summary>
        public void Dispose()
        {
            Util.ReleaseComObject(registry);
        }
    }
}
