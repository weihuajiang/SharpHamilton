using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// The Error of firmware will be throw as exception. 
    /// </summary>
    public class FirmwareException : Exception
    {
        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">inner exception</param>
        /// <param name="error">Firmware error</param>
        public FirmwareException(string message, Exception innerException, FirmwareError error) : base(message, innerException)
        {
            Error = error;
        }
        /// <summary>
        /// Firmware Error
        /// </summary>
        public FirmwareError Error { get; private set; }
    }
}
