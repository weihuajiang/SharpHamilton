using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    public enum StepStatusEnum
    {
        Start=0,
        Complete=1,
        Error=2,
        Progress=3,
        CompletedWithError=4
    }
    //
    //string[] status = new string[5] { "start", "complete", "error", "progress", "completed with error" };
}
