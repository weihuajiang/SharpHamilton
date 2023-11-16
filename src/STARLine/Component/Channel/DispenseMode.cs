using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Dispense mode enum
    /// </summary>
    public enum DispenseMode
    {
        /// <summary>
        /// jet part mode
        /// </summary>
        JetPart=0,
        /// <summary>
        /// jet empty tip mode
        /// </summary>
        JetEmpty=1,
        /// <summary>
        /// surface part volume mode
        /// </summary>
        SurfacePart=2,
        /// <summary>
        /// surface empty tip mode
        /// </summary>
        SurfaceEmpty=3,
        /// <summary>
        /// drain tip in jet mode
        /// </summary>
        DrainTipInJetMode=4,
        /// <summary>
        /// use the dispense mode in liquid class
        /// </summary>
        FromLiquiClass=8,
        /// <summary>
        /// blowout tip mode
        /// </summary>
        BlowoutTip=9
    }
    
}
