using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        static Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        static void Main()
        {
            AllocConsole();
            FileInfo finfo = new FileInfo(typeof(Program).Assembly.Location);
            string curentPath = finfo.DirectoryName;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string fileName = new AssemblyName(args.Name).Name + ".dll";
                String resourceName = "RoslynPad.Package." + fileName;
                //Must return the EXACT same assembly, do not reload from a new stream
                if (loadedAssemblies.TryGetValue(resourceName, out Assembly loadedAssembly))
                {
                    return loadedAssembly;
                }
                //check dll in resource
                bool isDllEmbed = false;
                foreach (var resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                {
                    if (resourceName.Equals(resource))
                        isDllEmbed = true;
                }
                if (!isDllEmbed)
                {
                    string dllPath = Path.Combine(curentPath, fileName);
                    if (!File.Exists(dllPath))
                        return null;
                    using (var stream = new FileStream(dllPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (stream == null)
                            return null;
                        Byte[] assemblyData = new Byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);

                        var assembly = Assembly.Load(assemblyData);
                        loadedAssemblies[resourceName] = assembly;
                        return assembly;
                    }
                }
                else
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        if (stream == null)
                            return null;
                        Byte[] assemblyData = new Byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);

                        var assembly = Assembly.Load(assemblyData);
                        loadedAssemblies[resourceName] = assembly;
                        return assembly;
                    }
                }
            };
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
