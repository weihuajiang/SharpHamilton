using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// pipetting parameter for aspiration and dispense
    /// </summary>
    internal class PipettingParameter
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
            get;set;
        }
        //LLD setting
        /// <summary>
        /// cLLD sensitivity
        /// if cLLD sensitivity was not off, the submerage depth will be used
        /// </summary>
        public LLDSensitivity cLLDSensitivity
        {
            get; set;
        } = LLDSensitivity.Off;

        /// <summary>
        /// pLLD sensitivity, it is only supported pipetting channel, not 96 head
        /// </summary>
        public LLDSensitivity pLLDSensitivity
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
        /// fix height above bottom when LLD was not set
        /// </summary>
        public double FixHeight
        {
            get; set;
        } = 0;

        /// <summary>
        /// max difference between cLLD and pLLD. disabled if it's 0, it is only supported pipetting channel, not 96 head
        /// </summary>
        public double MaxHeightDifference
        {
            get; set;
        } = 0;

        /// <summary>
        /// retract height after aspiration to aspirate air for movement
        /// </summary>
        public double RetractDistanceForAirTransport
        {
            get; set;
        } = 5;
        //Touch Off setting
        /// <summary>
        /// touch off mode dispense, it is only supported pipetting channel, not 96 head
        /// </summary>
        public bool TouchOff
        {
            get; set;
        } = false;
        /// <summary>
        /// dispense position above touch for touchoff dispense, it is only supported pipetting channel, not 96 head
        /// </summary>
        public double PositionAboveTouch
        {
            get; set;
        } = 0.5;
        /// <summary>
        /// Touch Side
        /// </summary>
        public bool TouchSide { get; set; } = false;
        /// <summary>
        /// liquid follow during aspirate or dispense
        /// </summary>
        public bool LiquidFollowing
        {
            get; set;
        } = false;
        public ZMoveAfterDispense ZMoveAfterDispense { get; set; } = ZMoveAfterDispense.Normal;
        /// <summary>
        /// mix cycle
        /// </summary>
        public int MixCycle
        {
            get; set;
        } = 0;
        /// <summary>
        /// mix position bellow aspiration and dispense height
        /// </summary>
        public double MixPosition
        {
            get; set;
        } = 2;

        /// <summary>
        /// mix volume
        /// </summary>
        public double MixVolume
        {
            get; set;
        } = 0;
    }
}
