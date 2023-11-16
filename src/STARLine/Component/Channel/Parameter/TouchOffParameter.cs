using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// parameter for pipetting with touch off
    /// </summary>
    public class TouchOffParameter : IParameter
    {
        /// <summary>
        /// retract height after aspiration to aspirate air for movement
        /// </summary>
        public double RetractDistanceForAirTransport
        {
            get; set;
        } = 5;
        /// <summary>
        /// aspirate or dispense position above touch for touchoff dispense, it is only supported pipetting channel, not 96 head
        /// </summary>
        public double PositionAboveTouch
        {
            get; set;
        } = 0.5;
        /// <summary>
        /// Liquid class parameter
        /// </summary>
        public LiquidClassParameter LiquidClassParameter { get; set; } = new LiquidClassParameter();
        /// <summary>
        /// advanced parameters for liquid following and mix
        /// </summary>
        public AdvancedParameter AdvancedParameters { get; set; } = new AdvancedParameter();
    }
}
