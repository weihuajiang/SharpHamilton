using Huarui.STARLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var ML_STAR = new STARCommand();
            ML_STAR.Log = Console.Out;
            ML_STAR.Init(true);
            ML_STAR.Show3DSystemView();
            ML_STAR.Start();
            ML_STAR.Initialize();
            ML_STAR.End();
            ML_STAR.Dispose();
        }
    }
}
