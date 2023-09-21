using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Firmware Error
    /// </summary>
    public class FirmwareError
    {
        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="node">module node name</param>
        /// <param name="command">firmware command</param>
        /// <param name="parameter">firmware parameter</param>
        /// <param name="code">error code</param>
        public FirmwareError(string node, string command, string parameter, int code)
        {
            Node = node;
            Command = command;
            Parameter = parameter;
            ErrorCode = code;
        }
        /// <summary>
        /// Module node name
        /// </summary>
        public string Node { get; private set; }
        /// <summary>
        /// Firmware command
        /// </summary>
        public string Command { get; private set; }
        /// <summary>
        /// Firmware parameter
        /// </summary>
        public string Parameter { get; private set; }
        /// <summary>
        /// Firmware Error
        /// </summary>
        public int ErrorCode { get; private set; }
        /// <summary>
        /// Parse reponse for firmware error
        /// </summary>
        /// <param name="command">firmware command</param>
        /// <param name="parameter">firmware parameter</param>
        /// <param name="response">firmware response</param>
        /// <returns></returns>
        public static FirmwareError Parse(string command, string parameter, string response)
        {
            int code = 0;
            int index = response.IndexOf("er");
            if (index >= 0)
            {
                code = int.Parse(response.Substring(index + 2, 2));
            }
            string node = command.Substring(0, 2);
            return new FirmwareError(node, command, parameter, code);
        }
    }
}
