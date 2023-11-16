using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Container shapes, as defined in Venus
    /// </summary>
    public enum Shape
    {
        /// <summary>
        /// Circular Cylinder
        /// </summary>
        CIRC = 0,
        /// <summary>
        /// Rectangle
        /// </summary>
        RECTANGLE = 1,
        /// <summary>
        /// Invert Cone
        /// </summary>
        CONE = 2,
        /// <summary>
        /// V-Cone
        /// </summary>
        VCONE = 3,
        /// <summary>
        /// Round Base
        /// </summary>
        RND_BS = 4,
        /// <summary>
        /// V-Cone Base
        /// </summary>
        VCONE_BS = 5,
        /// <summary>
        /// Flat Base
        /// </summary>
        FLAT_BS = 6
    }
}
