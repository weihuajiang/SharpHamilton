using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Determines how the start and end Z position is set
    /// </summary>
    public enum ZMoveAfterDispense
    {
        /// <summary>
        /// the Z positions are set to the maximal traverse height
        /// </summary>
        Normal,
        /// <summary>
        ///  the Z positions are set to the minimal traverse height of the labware
        /// </summary>
        Minimized
    }
    /// <summary>
    /// advanced parameter for liquid following and mix
    /// </summary>
    public class AdvancedParameter
    {/// <summary>
     /// liquid follow during aspirate or dispense or mix
     /// </summary>
        public bool LiquidFollowing
        {
            get; set;
        } = true;
        /// <summary>
        /// mix cycle
        /// </summary>
        public int MixCycle
        {
            get; set;
        } = 0;
        /// <summary>
        /// mix position bellow aspiration and dispense height
        /// </summary>
        public double MixPosition
        {
            get; set;
        } = 2;

        /// <summary>
        /// mix volume
        /// </summary>
        public double MixVolume
        {
            get; set;
        } = 0;
        /// <summary>
        /// Determines how the start and end Z position is set. this is only for dispensing
        /// </summary>
        public ZMoveAfterDispense ZMOveAfterDispense
        {
            get; set;
        } = ZMoveAfterDispense.Normal;
    }
}
