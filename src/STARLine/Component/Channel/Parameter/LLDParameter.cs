using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// cLLD parameter
    /// </summary>
    public class CLLDParameter: IParameter
    {
        //LLD setting
        /// <summary>
        /// cLLD sensitivity
        /// if cLLD sensitivity was not off, the submerage depth will be used
        /// </summary>
        public LLDSensitivity CLLDSensitivity
        {
            get; set;
        } = LLDSensitivity.LabwareDefinition;
        /// <summary>
        /// submerge depth in liquid when LLD was set
        /// </summary>
        public double SubmergeDepth
        {
            get; set;
        } = 2;
        /// <summary>
        /// Liquid class parameter
        /// </summary>
        public LiquidClassParameter LiquidClassParameter { get; set; } = new LiquidClassParameter();
        /// <summary>
        /// advanced parameters for liquid following and mix
        /// </summary>
        public AdvancedParameter AdvancedParameters { get; set; } = new AdvancedParameter();
    }
    /// <summary>
    /// parameter for pipetting with LLD
    /// </summary>
    public class LLDsParameter : IParameter
    {
        //LLD setting
        /// <summary>
        /// cLLD sensitivity
        /// if cLLD sensitivity was not off, the submerage depth will be used
        /// </summary>
        public LLDSensitivity CLLDSensitivity
        {
            get; set;
        } = LLDSensitivity.LabwareDefinition;

        /// <summary>
        /// pLLD sensitivity, it is only supported pipetting channel, not 96 head and it is only for aspiration, not dispense
        /// </summary>
        public LLDSensitivity PLLDSensitivity
        {
            get; set;
        } = LLDSensitivity.Off;

        /// <summary>
        /// submerge depth in liquid when LLD was set
        /// </summary>
        public double SubmergeDepth
        {
            get; set;
        } = 2;
        /// <summary>
        /// max difference between cLLD and pLLD. disabled if it's 0, it is only supported pipetting channel, not 96 head
        /// </summary>
        public double MaxHeightDifference
        {
            get; set;
        } = 0;
        /// <summary>
        /// Liquid class parameter
        /// </summary>
        public LiquidClassParameter LiquidClassParameter { get; set; } = new LiquidClassParameter();
        /// <summary>
        /// advanced parameters for liquid following and mix
        /// </summary>
        public AdvancedParameter AdvancedParameters { get; set; } = new AdvancedParameter();
    }
}
