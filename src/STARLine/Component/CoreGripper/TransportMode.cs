using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Transport mode for core gripper
    /// </summary>
    public enum TransportMode
    {
        /// <summary>
        /// Transport the plate only
        /// </summary>
        PlateOnly=0,

        /// <summary>
        /// Transport the lid only
        /// </summary>
        LidOnly=1,

        /// <summary>
        /// Transport plate and lid together
        /// </summary>
        PlateAndLid=2
    }
}
