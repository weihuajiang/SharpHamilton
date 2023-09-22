using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Arm Definition
    /// </summary>
    public abstract class Arm
    {
        /// <summary>
        /// Arm installed
        /// </summary>
        public bool IsInstalled { get; internal set; }
        /// <summary>
        /// Wrap size of arm
        /// </summary>
        public double WrapSize { get; internal set; }
        /// <summary>
        /// Minimal position of arm
        /// </summary>
        public double MinimalPosition { get; internal set; } 
        /// <summary>
        /// Maximal position of arm
        /// </summary>
        public double MaximalPosition { get; internal set; }
        /// <summary>
        /// get arm position
        /// </summary>
        public abstract double Position { get; }
        /// <summary>
        /// Speed in 0.1mm /sec (optimal 3 shoots / sec), range 20 to 999, default 270
        /// </summary>
        public abstract int Speed { get; set; }
        /// <summary>
        /// Index of acceleration curve, range 1 to 5, default 4
        /// </summary>
        public abstract int Acceleration { get; set; }
        /// <summary>
        /// Move arm
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="useZSafeHeight">Move X-arm to position with all attached components in Z-safety position</param>
        public abstract void Move(double position, bool useZSafeHeight = true);
    }
}
