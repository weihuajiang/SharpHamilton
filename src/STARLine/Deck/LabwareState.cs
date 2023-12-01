using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Labware State enum
    /// </summary>
    public enum LabwareState
    {
        /// <summary>
        /// unload the labware (no longer visible)
        /// </summary>
        Unload = 0,

        /// <summary>
        /// load the labware
        /// </summary>
        Load = 1,

        /// <summary>
        /// preparing to unload
        /// </summary>
        PrepareToUnload = 2,

        /// <summary>
        /// preparing to load
        /// </summary>
        PrepareToLoad = 3,

        /// <summary>
        /// cause labware to flash 
        /// </summary>
        Flash = 4,

        /// <summary>
        /// cause labware to flash 
        /// </summary>
        Flash2 = 5
    }
}
