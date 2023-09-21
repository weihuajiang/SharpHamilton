using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Liquid class parameter
    /// </summary>
    public class LiquidClassParameter
    {
        /// <summary>
        /// tip type
        /// </summary>
        public CoreTipType TipType
        {
            get; set;
        } = CoreTipType.HighVolumeTip;

        /// <summary>
        /// dispense mode
        /// </summary>
        public DispenseMode DispenseMode
        {
            get; set;
        } = DispenseMode.JetEmpty;

        /// <summary>
        /// liquid class
        /// </summary>
        public string LiquidClass
        {
            get; set;
        } = "";
    }
}
