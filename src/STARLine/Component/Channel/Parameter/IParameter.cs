using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Pipetting Parameter interface
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Liquid class parameter
        /// </summary>
        LiquidClassParameter LiquidClassParameter { get; set; }
        /// <summary>
        /// advanced parameters for liquid following and mix
        /// </summary>
        AdvancedParameter AdvancedParameters { get; set; }
    }
}
