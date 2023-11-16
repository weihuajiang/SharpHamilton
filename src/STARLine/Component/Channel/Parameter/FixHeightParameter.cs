using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// parameter for pipetting with fix height
    /// </summary>
    public class FixHeightParameter : IParameter
    {
        /// <summary>
        /// fix height above bottom when LLD was not set
        /// </summary>
        public double FixHeight
        {
            get; set;
        } = 0;

        /// <summary>
        /// retract height after aspiration to aspirate air for movement
        /// </summary>
        public double RetractDistanceForAirTransport
        {
            get; set;
        } = 5;
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
