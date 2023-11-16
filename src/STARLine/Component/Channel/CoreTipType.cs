using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Huarui.STARLine
{
    /// <summary>
    /// Core tip type enum
    /// </summary>
    public enum CoreTipType
    {
        /// <summary>
        /// Other tip type
        /// </summary>
        Other=-1,
        /// <summary>
        /// 300ul core tip
        /// </summary>
        [XmlEnum("0")]
        StandardVolumeTip =0,
        /// <summary>
        /// 300ul core tip with filter
        /// </summary>
        [XmlEnum("1")]
        StandardVolumeTipFiltered =1,
        /// <summary>
        /// 1000ul core tip
        /// </summary>
        [XmlEnum("4")]
        HighVolumeTip =4,
        /// <summary>
        /// 1000ul core tip with filter
        /// </summary>
        [XmlEnum("5")]
        HighVolumeTipFiltered =5,
        /// <summary>
        /// 50ul core tip
        /// </summary>
        [XmlEnum("22")]
        Tip50ul =22,
        /// <summary>
        /// 50ul core tip with filter
        /// </summary>
        [XmlEnum("23")]
        TipFiltered50ul =23,
        /// <summary>
        /// 5ml core tip
        /// </summary>
        [XmlEnum("25")]
        Tip5ml = 25,
        /// <summary>
        /// 4ml core tip with filter
        /// </summary>
        [XmlEnum("36")]
        Tip4mlFiltered=29,
        /// <summary>
        /// 300ul core slim tip
        /// </summary>
        [XmlEnum("36")]
        SlimTip300ul=36,
        /// <summary>
        /// 300ul core slim tip with filter
        /// </summary>
        [XmlEnum("45")]
        SlimTip300ulFiltered=45,

    }
}
