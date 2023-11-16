using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Cotainer segmemtn
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// Segment shape
        /// </summary>
        public Shape Shape { get; internal set; }
        /// <summary>
        /// segment top
        /// </summary>
        public double MaxHeight { get; internal set; }
        /// <summary>
        /// segment bottom
        /// </summary>
        public double MinHeight { get; internal set; }
        /// <summary>
        /// x dimension
        /// </summary>
        public double Dimension1 { get; internal set; }
        /// <summary>
        /// y dimension
        /// </summary>
        public double Dimension2 { get; internal set; }
        /// <summary>
        /// diameter
        /// </summary>
        public double Diameter { get; internal set; }
    }
}
