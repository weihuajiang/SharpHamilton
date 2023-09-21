using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    public interface ModuleErrorHandler
    {
        RecoveryAction AskForRecovery(ModuleErrors error);
    }
}
