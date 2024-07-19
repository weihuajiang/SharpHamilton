using Huarui.STARLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    public class STARLinePlugin : IPlugin
    {
        static Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        static STARLinePlugin()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var basePath = new FileInfo(typeof(STARLinePlugin).Assembly.Location).Directory.FullName;
                string fileName = Path.Combine(basePath, new AssemblyName(args.Name).Name + ".dll");
                if (loadedAssemblies.TryGetValue(fileName, out Assembly loadedAssembly))
                {
                    return loadedAssembly;
                }
                if (!File.Exists(fileName))
                    return null;
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (stream == null)
                        return null;
                    Byte[] assemblyData = new Byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);

                    var assembly = Assembly.Load(assemblyData);
                    loadedAssemblies[fileName] = assembly;
                    return assembly;
                }
            };
        }

        public Type[] TypeNamespaceImports => new Type[] { typeof(SimulatorExtension), typeof(STARCommand) };
        public string ScriptTemplate => @"var ML_STAR=new STARCommand();
//ML_STAR.Init(@""C:\Program Files(x86)\HAMILTON\Methods\Test\SystemEditor3d.lay"", 0, true);
ML_STAR.Init(true);
//ML_STAR.UseSimulator();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();
//write your code here
ML_STAR.End();";

        public void Start()
        {
        }

        public void Stop()
        {
        }
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool replaced = false)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);
            else if (!replaced) return;
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
        public void TaskAfterRun(string buildPath)
        {
        }

        public void TaskBeforeRun(string buildPath)
        {
            var basePath = new FileInfo(typeof(STARLinePlugin).Assembly.Location).Directory.FullName;
            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);
            var assemblyFileName = this.GetType().Assembly.GetName().Name;
            foreach (var f in Directory.EnumerateFiles(basePath))
            {
                var target = Path.Combine(buildPath, new FileInfo(f).Name);
                if (!File.Exists(target) && !target.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)
                    && !target.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                    && !target.EndsWith(assemblyFileName + ".dll", StringComparison.OrdinalIgnoreCase)
                    && !target.EndsWith("RoslynPad.Plugin.dll", StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(f, target);
                }
            }
            foreach (var f in Directory.EnumerateDirectories(basePath))
            {
                CopyDirectory(f, Path.Combine(buildPath, new DirectoryInfo(f).Name), true, false);
            }
        }
    }
}
