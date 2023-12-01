using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Liquid level detection sensitivity enum
    /// </summary>
    public enum LLDSensitivity
    {
        /// <summary>
        /// LLD off
        /// </summary>
        Off=0,
        /// <summary>
        /// very high
        /// </summary>
        VeryHight=1,
        /// <summary>
        /// high
        /// </summary>
        Heigh=2,
        /// <summary>
        /// medium
        /// </summary>
        Medium=3,
        /// <summary>
        /// low
        /// </summary>
        Low=4,
        /// <summary>
        /// from labware definition
        /// </summary>
        LabwareDefinition=5
    }
}
