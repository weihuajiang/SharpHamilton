using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Error from Module
    /// </summary>
    public class ModuleError
    {
        public int Index { get; internal set; }
        /// <summary>
        /// Name, e.g. position, or PX
        /// </summary>
        public string Name { get;internal set; }
        /// <summary>
        /// Position id, channel number or track number
        /// </summary>
        public string PositionId { get; internal set; }
        /// <summary>
        /// Labware ID
        /// </summary>
        public string LabwareId { get; internal set; }
        /// <summary>
        /// Error description, eg. E:ID:09/81:Cancel
        /// </summary>
        public string ErrorDetail { get; internal set; }
        /// <summary>
        /// Module node name
        /// </summary>
        public string Module { get; internal set; }
        /// <summary>
        /// Main Error number
        /// </summary>
        public int MainError { get; internal set; }
        /// <summary>
        /// Slave Error Number
        /// </summary>
        public int SlaveError { get; internal set; }
        /// <summary>
        /// Error Description
        /// </summary>
        public int Description { get; internal set; }
        /// <summary>
        /// Error recovery used
        /// </summary>
        public RecoveryAction Recovery { get; internal set; }
    }

    public class ModuleErrors : List<ModuleError>
    {
        public string StepName { get; internal set; }
        internal int TaskId { get; set; }
    }
}
