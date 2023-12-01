using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    /// <summary>
    /// Rack event args for rack add/remove event
    /// </summary>
    public class RackEventArgs : EventArgs
    {
        /// <summary>
        /// add/remove rack
        /// </summary>
        public Rack Rack { get; internal set; }
        /// <summary>
        /// site in parent rack
        /// </summary>
        public string ParentSite { get; internal set; }
        /// <summary>
        /// parent rack
        /// </summary>
        public Rack Parent { get; internal set; }
    }

}
