using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    public interface IPlugin
    {
        string ScriptTemplate { get; }
        Type[] TypeNamespaceImports { get; }
        void TaskBeforeRun(string buildPath);
        void TaskAfterRun(string buildPath);
        void Start();
        void Stop();
    }
}
