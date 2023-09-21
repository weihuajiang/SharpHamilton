using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Container or Rack action in the deck display
    /// </summary>
    public enum LabwareAction
    {
        /// <summary>
        ///  position is selected
        /// </summary>
        Selected = 0,

        /// <summary>
        /// processing
        /// </summary>
        Processing = 1,

        /// <summary>
        /// reserved
        /// </summary>
        Reserved = 2,

        /// <summary>
        /// indicates an error
        /// </summary>
        Error = 3,

        /// <summary>
        /// indicates processed
        /// </summary>
        Processed = 4,

        /// <summary>
        /// reset action state to none
        /// </summary>
        None = 5,

        /// <summary>
        /// position selected and flashing
        /// </summary>
        Flashing = 6
    }
}
