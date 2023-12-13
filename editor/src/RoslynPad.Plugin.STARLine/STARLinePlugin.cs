using Huarui.STARLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    public class STARLinePlugin : IPlugin
    {
        public Type[] TypeNamespaceImports => new Type[] { typeof(STARCommand) };

        public string ScriptTemplate => @"var ML_STAR=new STARCommand();
//ML_STAR.Init(@""C:\Program Files(x86)\HAMILTON\Methods\Test\SystemEditor3d.lay"", 0, true);
ML_STAR.Init(true);
//ML_STAR.Show3DSystemView();//show 3D deck layout
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

        public void TaskAfterRun(string buildPath)
        {
        }

        public void TaskBeforeRun(string buildPath)
        {
        }
    }
}
