using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Site for Rack
    /// </summary>
    public class Site
    {
        public Site() { }
        public Site(string id, double x, double y, double z, double width, double depth)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Depth = depth;
        }
        /// <summary>
        /// Site Id
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// x position of left top corner
        /// </summary>
        public double X { get; internal set; }
        /// <summary>
        /// t position of left top corner
        /// </summary>
        public double Y { get; internal set; }
        /// <summary>
        /// z position of site
        /// </summary>
        public double Z { get; internal set; }
        /// <summary>
        /// visible in decklayout view
        /// </summary>
        public bool Visible { get; internal set; }
        /// <summary>
        /// id showed in decklayout view
        /// </summary>
        public bool ShowId { get; internal set; }
        /// <summary>
        /// width of site
        /// </summary>
        public double Width { get; internal set; }
        /// <summary>
        /// depath of site
        /// </summary>
        public double Depth { get; internal set; }
        /// <summary>
        /// child racks on site
        /// </summary>
        public List<Rack> Racks { get; internal set; } = new List<Rack>();
    }
}
