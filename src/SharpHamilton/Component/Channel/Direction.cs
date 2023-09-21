using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Movement Direction
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// X direction
        /// </summary>
        XAxis=0,
        /// <summary>
        /// Y Direction
        /// </summary>
        YAxis=1,
        /// <summary>
        /// Z Direction
        /// </summary>
        ZAxis=2
    }
    /// <summary>
    /// Movement Type
    /// </summary>
    public enum MovementType
    {
        /// <summary>
        /// Absolute move
        /// </summary>
        Absolute=0,
        /// <summary>
        /// Relative move to current position
        /// </summary>
        Relative=1
    }
    /// <summary>
    /// Defines on which z-position the channels shall remain after x/y-position is reached
    /// </summary>
    public enum MoveZEndMode
    {
        /// <summary>
        /// max height
        /// </summary>
        MaxHeight=0,
        /// <summary>
        /// traverse height of instrument
        /// </summary>
        TraverseHeight=1,
        /// <summary>
        /// clearance height of labware
        /// </summary>
        LabwareClearanceHeight=2,
        /// <summary>
        /// container bottom
        /// </summary>
        ContainerBottom=3
    }
}
