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
        /// constructor
        /// </summary>
        public LiquidClassParameter() { }
        /// <summary>
        /// constructor for liquid class parameter
        /// </summary>
        /// <param name="tipType">tip type</param>
        /// <param name="dispenseMode">dispense mode</param>
        /// <param name="liquidClass">liquid class name</param>
        public LiquidClassParameter(CoreTipType tipType, DispenseMode dispenseMode, string liquidClass)
        {
            TipType = tipType;
            DispenseMode = dispenseMode;
            LiquidClass = liquidClass;
        }
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
