using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Huarui.STARLine
{

    [Guid("1CF2B120-547D-101B-8E65-08002B2BD119")]
    [InterfaceType(1)]
    [Browsable(false)]
    public interface IErrorInfo
    {
        void GetGUID(out Guid pGUID);
        void GetSource(out string pBstrSource);
        void GetDescription(out string pBstrDescription);
        void GetHelpFile(out string pBstrHelpFile);
        void GetHelpContext(out uint pdwHelpContext);
    }
}
