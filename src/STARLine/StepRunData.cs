using Hamilton.Interop.HxGruCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    class StepRunData
    {
        public IHxCommandStepRun5 Command { get; set; }
        public string DataId { get; set; }
        public string StepBaseName { get; set; }
    }
}
