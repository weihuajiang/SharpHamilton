using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// all the instrument's error will be in the exception
    /// using the try catch code, you can get the error, and perform the error handling
    /// </summary>
    /// <example>
    /// 
    /// <code language="cs">
    /// ErrorRecoveryOptions options = new ErrorRecoveryOptions();
    /// options.Add(MainErrorEnum.NoTipError, new ErrorRecoveryOption() { Recovery = RecoveryAction.Cancel });
    /// options.Add(MainErrorEnum.NotExecutedError, new ErrorRecoveryOption() { Recovery = RecoveryAction.Cancel });
    /// try
    /// {
    ///     ML_STAR.Channel.PickupTip(cnts, options);
    /// }
    /// catch (STARException e)
    /// {
    ///     foreach (ModuleError err in e.ModuleErrors)
    ///     {
    ///         Console.WriteLine(err.Module + " " + err.MainError + ":" + err.SlaveError + " " + err.LabwareId + ", " + err.PositionId);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class STARException : Exception
    {
        /// <summary>
        /// STARException construction
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">inner exception</param>
        /// <param name="errors">module errors</param>
        public STARException(string message, Exception innerException, ModuleErrors errors):base(message, innerException)
        {
            ModuleErrors = errors;
        }
        /// <summary>
        /// Module errors from device
        /// </summary>
        public ModuleErrors ModuleErrors { get; private set; }
    }
}
