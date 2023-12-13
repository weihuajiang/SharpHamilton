using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace RoslynPad
{
    public partial class App
    {
        public static IPlugin[] Plugins;
        private const string ProfileFileName = "RoslynPad.jitprofile";
        static IPlugin? LoadPlugin(PluginDescriptor i)
        {
            var path = i.Assembly;
            if (!Path.IsPathRooted(path))
            {
                var basePath = new FileInfo(typeof(App).Assembly.Location).Directory.FullName;
                path = Path.Combine(basePath, i.Assembly);
            }
            Assembly a = Assembly.LoadFrom(path);
            var p = Activator.CreateInstance(a.GetType(i.Type));
            return p as IPlugin;
        }
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();
        static App()
        {
            //AllocConsole();
            try
            {
                var plugins = System.Configuration.ConfigurationManager.GetSection("PluginConfiguration") as PluginConfiguration;
                if (plugins == null || plugins.Plugins == null)
                    Plugins = new IPlugin[0];
                else
                {
                    List<IPlugin> ps = new List<IPlugin>();
                    foreach (var i in plugins.Plugins)
                    {
                        try
                        {
                            var p = LoadPlugin(i);
                            if (p != null && p is IPlugin t)
                                ps.Add(t);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Plugins = new IPlugin[0];
                        }
                    }
                    Plugins = ps.ToArray();
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Plugins = new IPlugin[0];
            }
        }
        public App()
        {
            ProfileOptimization.SetProfileRoot(AppDomain.CurrentDomain.BaseDirectory);
            ProfileOptimization.StartProfile(ProfileFileName);

        }
    }
}
