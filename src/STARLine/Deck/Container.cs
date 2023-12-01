using Hamilton.Interop.HxCfgFil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Container
    /// </summary>
    public class Container : IDisposable
    {
        /// <summary>
        /// Position Id
        /// </summary>
        public string Position { get; internal set; }
        /// <summary>
        /// Parent rack labware id
        /// </summary>
        public string Labware { get;internal set; }
        /// <summary>
        /// Barcode
        /// </summary>
        public string Barcode { get;set; }
        /// <summary>
        /// X position
        /// </summary>
        public double X { get; internal set; }
        /// <summary>
        /// Y position
        /// </summary>
        public double Y { get; internal set; }
        /// <summary>
        /// Z position
        /// </summary>
        public double Z { get; internal set; }
        /// <summary>
        /// width of container
        /// </summary>
        public double XDim { get; internal set; }
        /// <summary>
        /// height of container
        /// </summary>
        public double YDim { get; internal set; }
        /// <summary>
        /// Liquid level detection valid
        /// </summary>
        public bool LiquidSeekValid { get; internal set; } = false;
        /// <summary>
        /// Liquid Seeking Start Position
        /// </summary>
        public double LiquidSeekingStartPosition { get; internal set; }
        /// <summary>
        /// cLLD sensitivity for container
        /// </summary>
        public LLDSensitivity CLLD { get; internal set; }

        /// <summary>
        /// Clearance Height
        /// </summary>
        public double Clearance { get; internal set; }
        /// <summary>
        /// diameter of container
        /// </summary>
        public double Diameter { get; internal set; }
        /// <summary>
        /// shape of container
        /// </summary>
        public Shape Shape { get; internal set; }
        /// <summary>
        /// File of container
        /// </summary>
        public string File { get; internal set; }

        /// <summary>
        /// Container depth
        /// </summary>
        public double Depth { get; internal set; }
        /// <summary>
        /// Max Pipetting depth
        /// </summary>
        public double MaxPipettingDepth { get; internal set; }
        /// <summary>
        /// is container visible in the decklyaout
        /// </summary>
        public bool IsVisble { get; set; } = true;
        /// <summary>
        /// Container State
        /// </summary>
        public ContainerStatus State { get; set; }
        /// <summary>
        /// Container segments
        /// </summary>
        public List<Segment> Segments { get; internal set; } = new List<Segment>();
        /// <summary>
        /// Model information
        /// </summary>
        public ModelData Model { get; set; }
        /// <summary>
        /// object for developer to store someinformation 
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// Calculates the volume in ml for the container 
        /// </summary>
        /// <param name="height">The internal height in mm in the container</param>
        /// <param name="deckCoordinates">Specifies whether the internal height is measured in deck coordinates or container coordinates </param>
        /// <returns></returns>
        public double ComputeVolume(double height, bool deckCoordinates = false)
        {
            if (deckCoordinates)
                height = height - Z;
            Hamilton.Interop.HxLabwr3.Container cnt = new Hamilton.Interop.HxLabwr3.Container();
            HxCfgFile cfg = new HxCfgFile();
            cfg.LoadFile(File);
            cnt.InitFromCfgFil(cfg);
            double v= cnt.ComputContainerVolumeForHt(height);
            ReleaseComObject(cfg);
            ReleaseComObject(cnt);
            return v;
        }

        static void ReleaseComObject(object obj)
        {
            if (obj == null)
                return;
            /*
            int count = 0;
            while ((Marshal.ReleaseComObject(obj)) > 0) ;
            {
                count++;
                if (count > 1000)
                    return;
            }
            */
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(obj);
            obj = null;
        }
        /// <summary>
        /// Dispose container
        /// </summary>
        public void Dispose()
        {
        }
    }
}
