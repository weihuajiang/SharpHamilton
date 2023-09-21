using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// The mode which is used for upload of TADM curves
    /// </summary>
    public enum TADMUploadMode
    {
        /// <summary>
        /// None, i.e. don't upload
        /// </summary>
        None = 0,
        /// <summary>
        /// Error curves only
        /// </summary>
        ErrorCurvesOnly=1,
        /// <summary>
        /// All curves
        /// </summary>
        All = 2
    }

    /// <summary>
    /// TADM record mode
    /// </summary>
    public enum TADMRecordMode
    {
        /// <summary>
        /// Recording
        /// </summary>
        Recording = 0,
        /// <summary>
        /// Monitoring
        /// </summary>
        Monitoring = 1
    }
}
