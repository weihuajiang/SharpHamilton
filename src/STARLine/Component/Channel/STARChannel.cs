using Hamilton.Interop.HxCfgFil;
using Hamilton.Interop.HxCoreLiquid;
using Hamilton.Interop.HxGruCommand;
using Hamilton.Interop.HxLabwr3;
using Hamilton.Interop.HxParams;
using Hamilton.Interop.HxReg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    /// <summary>
    /// STAR Pipetting Channel (PX)
    /// </summary>
    public partial class Channel
    {
        STARCommand _command;
        int channelTask = 2;
        internal Channel(STARCommand cmd)
        {
            _command = cmd;
            //load tadm feature installation status
            
        }

        /// <summary>
        /// Channel Count
        /// </summary>
        public int Count
        {
            get
            {
                int channelCount = (_command.MlSTAR as IHxCommandEdit5).GetValueWithKey(1);
                return channelCount;
            }
        }

        /// <summary>
        /// Channel Y Distance
        /// </summary>
        public double YRaster
        {
            get
            {
                double Ydistance = (_command.MlSTAR as IHxCommandEdit5).GetValueWithKey(26);
                return Ydistance;
            }
        }
        /// <summary>
        /// TADM curve upload mode
        /// </summary>
        public TADMUploadMode TADMUploadMode
        {
            get
            {
                int m = (_command.MlSTAR as IHxCommandEdit5).GetValueWithKey(7);
                return (TADMUploadMode)m;
            }
        }
        /// <summary>
        /// TADM Record Mode
        /// </summary>
        public TADMRecordMode TADMRecordMode
        {
            get
            {
                int m = (_command.MlSTAR as IHxCommandEdit5).GetValueWithKey(8);
                return (TADMRecordMode)m;
            }
        }
        string PXSequence = "PXSequence";

        /// <summary>
        /// Pickup tips
        /// </summary>
        /// <param name="tips">tip position</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void PickupTip(Container[] tips, ErrorRecoveryOptions options = null)
        {
            string pattern = "";
            List<int> channel = new List<int>();
            int count = Count;
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = 0; i < Math.Min(tips.Length, count); i++)
            {
                Container c = tips[i];
                if (c != null)
                {
                    pattern = pattern + "1";
                    channel.Add(1);
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
                else
                {
                    pattern = pattern + "0";
                    channel.Add(0);
                }
            }
            for (int i = Math.Min(tips.Length, count); i < count; i++)
            {
                pattern = pattern + "0";
                channel.Add(0);
            }
                (seqc as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PXSequence, seqc);

            string stepBaseName = "TipPickUp";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);

            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceObject");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceName");
            usedVariable.Add(1, _command.InstrumentName + "." + PXSequence);
            usedVariable.Add(pattern, "ChannelPattern");
            usedVariable.Add(1, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
            usedVariable.Add(1, "SequenceCounting");
            index = 1;
            foreach (int c in channel)
            {
                usedVariable.Add(c, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.activeTip);
                index++;
            }
            usedVariable.Add("SequenceObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PXSequence, 0, 0);

            AddSequences(usedVariable, objBounds);

            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);

            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task chnTask = null;
                if (_command.Simulator != null && _command.Simulator.Channel1000 != null && _command.IsSimulation)
                    chnTask = _command.Simulator.Channel1000.PickupTips(tips);
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);

                if (chnTask != null) chnTask.Wait();
            }
            catch (Exception e)
            {
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqc);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqc);
        }
        void AddSequences(HxPars usedVariable, HxPars objBounds)
        {
            _command.AddSequences(usedVariable, objBounds);
        }

        /// <summary>
        /// Eject tips to containers
        /// </summary>
        /// <param name="tips">containers to drop the tips</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void EjectTip(Container[] tips, ErrorRecoveryOptions options = null)
        {
            if (tips == null)
            {
                EjectTip();
                return;
            }
            string pattern = "";
            List<int> channel = new List<int>();
            int count = Count;
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = 0; i < Math.Min(tips.Length, count); i++)
            {
                Container c = tips[i];
                if (c != null)
                {
                    pattern = pattern + "1";
                    channel.Add(1);
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
                else
                {
                    pattern = pattern + "0";
                    channel.Add(0);
                }
            }
            for (int i = Math.Min(tips.Length, count); i < count; i++)
            {
                pattern = pattern + "0";
                channel.Add(0);
            }
            (seqc as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PXSequence, seqc);

            string stepBaseName = "TipEject";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);

            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceObject");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceName");
            usedVariable.Add(4, _command.InstrumentName + "." + PXSequence);
            usedVariable.Add(0, "UseDefaultWaste");
            usedVariable.Add(pattern, "ChannelPattern");
            usedVariable.Add(1, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
            usedVariable.Add(1, "SequenceCounting");
            index = 1;
            foreach (int c in channel)
            {
                usedVariable.Add(c, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.activeTip);
                index++;
            }
            usedVariable.Add("SequenceObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PXSequence, 0, 0);

            AddSequences(usedVariable, objBounds);
            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task chnTask = null;
                if (_command.Simulator != null && _command.Simulator.Channel1000 != null && _command.IsSimulation)
                    chnTask = _command.Simulator.Channel1000.EjectTips(tips);

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);

                if (chnTask != null) chnTask.Wait();
            }
            catch (Exception e)
            {
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqc);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqc);
        }
        /// <summary>
        /// Eject tips to waste
        /// </summary>
        /// <param name="options">error handling option</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void EjectTip(ErrorRecoveryOptions options = null)
        {
            string pattern = "";
            List<int> channel = new List<int>();
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                pattern = pattern + "1";
                channel.Add(1);
            }
            string stepBaseName = "TipEject";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);

            usedVariable.Add(1, "UseDefaultWaste");
            usedVariable.Add(pattern, "ChannelPattern");
            usedVariable.Add("", "SequenceName");
            usedVariable.Add(1, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
            usedVariable.Add(0, "SequenceCounting");
            int index = 1;
            foreach (int c in channel)
            {
                usedVariable.Add(c, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.activeTip);
                index++;
            }
            AddSequences(usedVariable, objBounds);
            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);

            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task chnTask = null;
                if (_command.Simulator != null && _command.Simulator.Channel1000 != null && _command.IsSimulation)
                    chnTask = _command.Simulator.Channel1000.EjectTips();

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);

                if (chnTask != null) chnTask.Wait();
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
        }
        internal static PipettingParameter SetParameter(IParameter parameter)
        {
            if (parameter == null)
                return null;
            PipettingParameter pt = new PipettingParameter();
            //liquid class
            pt.TipType = parameter.LiquidClassParameter.TipType;
            pt.DispenseMode = parameter.LiquidClassParameter.DispenseMode;
            pt.LiquidClass = parameter.LiquidClassParameter.LiquidClass;

            //liquid following
            pt.LiquidFollowing = parameter.AdvancedParameters.LiquidFollowing;
            pt.ZMoveAfterDispense = parameter.AdvancedParameters.ZMOveAfterDispense;
            
            //mix 
            pt.MixCycle = parameter.AdvancedParameters.MixCycle;
            pt.MixPosition = parameter.AdvancedParameters.MixPosition;
            pt.MixVolume = parameter.AdvancedParameters.MixVolume;
            if(parameter is LLDsParameter)
            {
                pt.cLLDSensitivity = (parameter as LLDsParameter).cLLDSensitivity;
                pt.pLLDSensitivity = (parameter as LLDsParameter).pLLDSensitivity;
                pt.SubmergeDepth = (parameter as LLDsParameter).SubmergeDepth;
                pt.MaxHeightDifference = (parameter as LLDsParameter).MaxHeightDifference;
            }
            else if(parameter is CLLDParameter)
            {
                pt.cLLDSensitivity = (parameter as CLLDParameter).cLLDSensitivity;
                pt.SubmergeDepth = (parameter as CLLDParameter).SubmergeDepth;
            }
            else if(parameter is FixHeightParameter)
            {
                pt.FixHeight = (parameter as FixHeightParameter).FixHeight;
                pt.RetractDistanceForAirTransport = (parameter as FixHeightParameter).RetractDistanceForAirTransport;
            }
            else if(parameter is TouchOffParameter)
            {
                pt.TouchOff = true;
                pt.RetractDistanceForAirTransport = (parameter as TouchOffParameter).RetractDistanceForAirTransport;
                pt.PositionAboveTouch = (parameter as TouchOffParameter).PositionAboveTouch;
            }
            else if(parameter is SideTouchParameter)
            {
                pt.TouchSide = true;
                pt.RetractDistanceForAirTransport = (parameter as SideTouchParameter).RetractDistanceForAirTransport;
            }
            return pt;
        }
        /// <summary>
        /// Aspirate with different parameter for each channel
        /// </summary>
        /// <param name="cnts"></param>
        /// <param name="volume"></param>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="options"></param>
        public void Aspirate(Container[] cnts, double volume, IParameter[] parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] pts = new PipettingParameter[parameter.Length];
            for(int i=0;i<parameter.Length;i++)
                pts[i] = SetParameter(parameter[i]);
            double[] vs = new double[Count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volume;
            AspirateImpl(cnts, vs, pts, mode, options);
        }
        /// <summary>
        /// Aspirate with different parameter for each channel
        /// </summary>
        /// <param name="cnts"></param>
        /// <param name="volumes"></param>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="options"></param>
        public void Aspirate(Container[] cnts, double[] volumes, IParameter[] parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] pts = new PipettingParameter[parameter.Length];
            for (int i = 0; i < parameter.Length; i++)
                pts[i] = SetParameter(parameter[i]);
            AspirateImpl(cnts, volumes, pts, mode, options);
        }
        /// <summary>
        /// aspirate with liquid level detection
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(Container[] cnts, double volume, LLDsParameter parameter, AspirateMode mode=AspirateMode.Aspiration, ErrorRecoveryOptions options=null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volume, pt, mode, options);
        }
        /// <summary>
        /// aspirate with liquid level detection
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(Container[] cnts, double[] volumes, LLDsParameter parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volumes, pt, mode, options);
        }
        /// <summary>
        /// aspirate with fix height
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(Container[] cnts, double volume, FixHeightParameter parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volume, pt, mode, options);
        }
        /// <summary>
        /// aspirate with fix height
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(Container[] cnts, double[] volumes, FixHeightParameter parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volumes, pt, mode, options);
        }
        /// <summary>
        /// aspirate with Touch off
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(Container[] cnts, double volume, TouchOffParameter parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volume, pt, mode, options);
        }
        /// <summary>
        /// aspirate with touch off
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(Container[] cnts, double[] volumes, TouchOffParameter parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volumes, pt, mode, options);
        }
        /// <summary>
        /// Aspirate
        /// </summary>
        /// <param name="cnts">sequence</param>
        /// <param name="volumes">volume</param>
        /// <param name="parameters">aspirate parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="options">error handling option</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        private void Aspirate(Container[] cnts, double volumes, PipettingParameter parameters, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            int count = Count;
            double[] vs = new double[count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volumes;
            Aspirate(cnts, vs, parameters, mode, options);
        }

        /// <summary>
        /// Aspirate
        /// </summary>
        /// <param name="cnts">sequence</param>
        /// <param name="volumes">volume</param>
        /// <param name="parameter">aspirate parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="options">error handling option</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        private void Aspirate(Container[] cnts, double[] volumes, PipettingParameter parameter, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] parameters = new PipettingParameter[Count];
            for (int i = 0; i < parameters.Length;i++)
                parameters[i] = parameter;
            AspirateImpl(cnts, volumes, parameters, mode, options);
        }
        PipettingParameter GetNotNull(PipettingParameter[] parameters)
        {
            foreach (var c in parameters)
                if (c != null)
                    return c;
            return null;
        }
        private void AspirateImpl(Container[] cnts, double[] volumes, PipettingParameter[] parameters, AspirateMode mode = AspirateMode.Aspiration, ErrorRecoveryOptions options = null)
        {
            string pattern = "";
            List<int> channel = new List<int>();
            int count = Count;
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = 0; i < Math.Min(cnts.Length, count); i++)
            {
                Container c = cnts[i];
                if (c != null)
                {
                    pattern = pattern + "1";
                    channel.Add(1);
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
                else
                {
                    pattern = pattern + "0";
                    channel.Add(0);
                }
            }
            for (int i = Math.Min(cnts.Length, count); i < count; i++)
            {
                pattern = pattern + "0";
                channel.Add(0);
            }
                (seqc as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PXSequence, seqc);

            string stepBaseName = "Aspirate";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);

            usedVariable.Add(instanceId, "CommandStepFileGuid");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceObject");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceName");
            usedVariable.Add(2, _command.InstrumentName + "." + PXSequence);
            usedVariable.Add((int)GetNotNull(parameters).TipType, "TipType");
            usedVariable.Add((int)GetNotNull(parameters).DispenseMode, "DispenseMode");
            usedVariable.Add(pattern, "ChannelPattern");
            usedVariable.Add(1, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
            usedVariable.Add(1, "SequenceCounting");
            usedVariable.Add(3, "ParsCommandVersion");
            //Remove default value
            usedVariable.Remove(HxAtsInstrumentParsKeys.aspAirRetractDist);
            usedVariable.Remove("LiquidFollowing");
            usedVariable.Remove("DefaultLiquidType");
            usedVariable.Remove("LLDFluidHeight");
            usedVariable.Remove("MixPosition");
            usedVariable.Remove("LLDPressure");
            usedVariable.Remove("TouchOffMode");
            usedVariable.Remove("MixVolume");
            usedVariable.Remove("MixCycles");
            usedVariable.Remove("LLDDualDifference");
            usedVariable.Remove(HxAtsInstrumentParsKeys.touchofDistance);
            usedVariable.Remove("AspirateMode");
            usedVariable.Remove("LLDCapacitive");
            usedVariable.Remove("LLDSubmerge");

            index = 1;
            foreach (int c in channel)
            {
                PipettingParameter parameter = parameters[index - 1];
                if (parameter == null)
                    parameter = GetNotNull(parameters);
                usedVariable.Add(c, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.activeTip);
                usedVariable.Add(parameter.MaxHeightDifference, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.differenceDualLld);
                usedVariable.Add((int)parameter.cLLDSensitivity, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.lldSetting);
                usedVariable.Add(parameter.LiquidFollowing ? 1 : 0, HxAtsInstrumentParsKeys.channel, index, "LiquidFollowing");
                usedVariable.Add(parameter.TouchOff ? 1 : 0, HxAtsInstrumentParsKeys.channel, index, "TouchOffMode");
                usedVariable.Add(parameter.RetractDistanceForAirTransport, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.aspAirRetractDist);
                usedVariable.Add(parameter.LiquidClass, HxAtsInstrumentParsKeys.channel, index, "LiquidName");
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.lldMode);//??????????
                usedVariable.Add(parameter.PositionAboveTouch, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.touchofDistance);
                usedVariable.Add((int)mode, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.aspirationType);
                usedVariable.Add((int)parameter.pLLDSensitivity, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.presureLldSettings);
                usedVariable.Add(volumes[index - 1], HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.aspirationVolume);
                usedVariable.Add(parameter.MixCycle, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixCycles);
                usedVariable.Add(parameter.MixPosition, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixPosition);
                usedVariable.Add(parameter.MixVolume, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixVolume);
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.followDistance);
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixFollowDistance);
                usedVariable.Add(parameter.SubmergeDepth, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.submergeDepth);
                usedVariable.Add(parameter.FixHeight, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.fluidHeight);
                index++;
            }
            usedVariable.Add("SequenceObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PXSequence, 0, 0);

            AddSequences(usedVariable, objBounds);

            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task chnTask = null;
                if (_command.Simulator != null && _command.Simulator.Channel1000 != null && _command.IsSimulation)
                    chnTask = _command.Simulator.Channel1000.Pipette(cnts, 5, 5);

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);

                if (chnTask != null) chnTask.Wait();
            }
            catch (Exception e)
            {
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqc);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqc);
        }
        /// <summary>
        /// Dispense with different parameter of each channel
        /// </summary>
        /// <param name="cnts"></param>
        /// <param name="volume"></param>
        /// <param name="parameters"></param>
        /// <param name="mode"></param>
        /// <param name="useFirstAspirtateLiquidClass"></param>
        /// <param name="options"></param>
        public void Dispense(Container[] cnts, double volume, IParameter[] parameters, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            bool optimizZMove = false;
            PipettingParameter[] pts = new PipettingParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                pts[i] = SetParameter(parameters[i]);
            double[] vs = new double[Count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volume;
            DispenseImpl(cnts, vs, pts, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with different parameter of each channel
        /// </summary>
        /// <param name="cnts"></param>
        /// <param name="volumes"></param>
        /// <param name="parameters"></param>
        /// <param name="mode"></param>
        /// <param name="useFirstAspirtateLiquidClass"></param>
        /// <param name="options"></param>
        public void Dispense(Container[] cnts, double[] volumes, IParameter[] parameters, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            bool optimizZMove = false;
            PipettingParameter[] pts = new PipettingParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                pts[i] = SetParameter(parameters[i]);
            DispenseImpl(cnts, volumes, pts, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with LLD
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double volume, CLLDParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options=null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove=(pt.ZMoveAfterDispense==ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with LLD
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double[] volumes, CLLDParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with fix height
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double volume, FixHeightParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with fix height
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double[] volumes, FixHeightParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with touch off
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double volume, TouchOffParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with touch off
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double[] volumes, TouchOffParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with side touch
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">side touch parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double volume, SideTouchParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, true, optimizZMove, options);
        }
        /// <summary>
        /// Dispense with side touch
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">side touch parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(Container[] cnts, double[] volumes, SideTouchParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, true, optimizZMove, options);
        }
        /// <summary>
        /// Dispense
        /// </summary>
        /// <param name="cnts">sequence</param>
        /// <param name="volume">volume</param>
        /// <param name="parameter">dispense parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspiration liquid class</param>
        /// <param name="sideTouch">use side touch?</param>
        /// <param name="optimizeZMove">optimize z move, false for normal, true for minized z move</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        private void Dispense(Container[] cnts, double volume, PipettingParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass,
            bool useFirstAspirtateLiquidClass = true, bool sideTouch = false, bool optimizeZMove = false, ErrorRecoveryOptions options = null)
        {
            int count = Count;
            double[] vs = new double[count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volume;
            Dispense(cnts, vs, parameter, mode, useFirstAspirtateLiquidClass, sideTouch, optimizeZMove, options);
        }

        /// <summary>
        /// Dispense
        /// </summary>
        /// <param name="cnts">sequence</param>
        /// <param name="volumes">volumes</param>
        /// <param name="parameter">dispense parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspiration liquid class</param>
        /// <param name="sideTouch">use side touch?</param>
        /// <param name="optimizeZMove">optimize z move</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        private void Dispense(Container[] cnts, double[] volumes, PipettingParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass,
            bool useFirstAspirtateLiquidClass = true, bool sideTouch = false, bool optimizeZMove = false, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] parameters = new PipettingParameter[Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = parameter;
            DispenseImpl(cnts, volumes, parameters, mode, useFirstAspirtateLiquidClass,sideTouch, optimizeZMove, options);
        }
        private void DispenseImpl(Container[] cnts, double[] volumes, PipettingParameter[] parameters, DispenseMode mode = DispenseMode.FromLiquiClass,
            bool useFirstAspirtateLiquidClass = true, bool sideTouch = false, bool optimizeZMove = false, ErrorRecoveryOptions options = null)
        {
            string pattern = "";
            List<int> channel = new List<int>();
            int count = Count;
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = 0; i < Math.Min(cnts.Length, count); i++)
            {
                Container c = cnts[i];
                if (c != null)
                {
                    pattern = pattern + "1";
                    channel.Add(1);
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
                else
                {
                    pattern = pattern + "0";
                    channel.Add(0);
                }
            }
            for (int i = Math.Min(cnts.Length, count); i < count; i++)
            {
                pattern = pattern + "0";
                channel.Add(0);
            }
                (seqc as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PXSequence, seqc);

            string stepBaseName = "Dispense";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);
            usedVariable.Add(instanceId, "CommandStepFileGuid");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceObject");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceName");
            usedVariable.Add(2, _command.InstrumentName + "." + PXSequence);
            usedVariable.Add((int)GetNotNull(parameters).TipType, "TipType");
            usedVariable.Add(3, "DispenseMode");//to do.............
            usedVariable.Add(pattern, "ChannelPattern");
            usedVariable.Add(1, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
            usedVariable.Add(1, "SequenceCounting");
            usedVariable.Add(optimizeZMove ? 1 : 0, "OptimizeZMove");
            usedVariable.Add(useFirstAspirtateLiquidClass ? 1 : 0, "SameLiquid");
            usedVariable.Add(sideTouch ? 1 : 0, "SideTouchMode");
            //remove default value
            usedVariable.Remove("LiquidFollowing");
            usedVariable.Remove("DefaultLiquidType");
            usedVariable.Remove("LLDFluidHeight");
            usedVariable.Remove("LLDPressure");
            usedVariable.Remove("MixPosition");
            usedVariable.Remove("MixCycles");
            usedVariable.Remove("MixVolume");
            usedVariable.Remove("TouchOffMode");
            usedVariable.Remove(HxAtsInstrumentParsKeys.aspAirRetractDist);
            usedVariable.Remove("LLDCapacitive");
            usedVariable.Remove(HxAtsInstrumentParsKeys.touchofDistance);
            usedVariable.Remove("LLDSubmerge");

            index = 1;
            foreach (int c in channel)
            {
                PipettingParameter parameter = parameters[index - 1];
                if (parameter == null) parameter = GetNotNull(parameters);
                usedVariable.Add(c, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.activeTip);
                usedVariable.Add(parameter.cLLDSensitivity, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.lldSetting);
                usedVariable.Add(parameter.LiquidFollowing ? 1 : 0, HxAtsInstrumentParsKeys.channel, index, "LiquidFollowing");
                usedVariable.Add(parameter.TouchOff ? 1 : 0, HxAtsInstrumentParsKeys.channel, index, "TouchOffMode");
                usedVariable.Add(parameter.RetractDistanceForAirTransport, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.aspAirRetractDist);
                usedVariable.Add(parameter.LiquidClass, HxAtsInstrumentParsKeys.channel, index, "LiquidName");
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.lldMode);//??????????
                usedVariable.Add(parameter.PositionAboveTouch, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.touchofDistance);
                usedVariable.Add((int)mode, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.dispensationType);
                usedVariable.Add((int)parameter.pLLDSensitivity, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.presureLldSettings);
                usedVariable.Add(volumes[index - 1], HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.dispensationVolume);
                usedVariable.Add(parameter.MixCycle, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixCycles);
                usedVariable.Add(parameter.MixPosition, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixPosition);
                usedVariable.Add(parameter.MixVolume, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixVolume);
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.followDistance);
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.mixFollowDistance);
                usedVariable.Add(parameter.SubmergeDepth, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.submergeDepth);
                usedVariable.Add(parameter.FixHeight, HxAtsInstrumentParsKeys.channel, index, HxAtsInstrumentParsKeys.fluidHeight);
                index++;
            }
            usedVariable.Add("SequenceObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PXSequence, 0, 0);

            AddSequences(usedVariable, objBounds);

            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);

            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task chnTask = null;
                if (_command.Simulator != null && _command.Simulator.Channel1000 != null && _command.IsSimulation)
                    chnTask = _command.Simulator.Channel1000.Pipette(cnts, 0, -5);

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);

                if (chnTask != null) chnTask.Wait();
            }
            catch (Exception e)
            {
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqc);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqc);
        }
        /// <summary>
        /// Move To Position
        /// </summary>
        /// <param name="cnts">positions</param>
        /// <param name="zmode">z end mode after move</param>
        /// <param name="positionAboveContainerBottom">distance above container bottom, for zmode=3</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Move(Container[] cnts, MoveZEndMode zmode, double positionAboveContainerBottom = 0, ErrorRecoveryOptions options = null)
        {
            string stepBaseName = "MoveToPosition";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);

            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = 0; i < cnts.Length; i++)
            {
                Container c = cnts[i];
                if (c == null)
                    continue;
                HxPars param = new HxPars();
                param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                index++;
                seqc.AppendItem(param);
            }
            (seqc as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PXSequence, seqc);

            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceObject");
            usedVariable.Add(_command.InstrumentName + "." + PXSequence, "SequenceName");
            usedVariable.Add(37, _command.InstrumentName + "." + PXSequence);
            usedVariable.Add("SequenceObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PXSequence, 0, 0);


            usedVariable.Add(2, "MoveDirection");
            usedVariable.Add((int)zmode, "MoveZEndMode");//////

            usedVariable.Add(2, "MoveType");
            usedVariable.Add(0, "MoveRelDistance");
            usedVariable.Add(0, "MoveAbsDistance");
            for (int i = 1; i <= Count; i++)
            {
                usedVariable.Add(positionAboveContainerBottom, HxAtsInstrumentParsKeys.channel, i, "MoveZEndDelta");
            }

            AddSequences(usedVariable, objBounds);
            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                StepResult results = StepResult.Parse(result.Item2(HxCommandKeys.resultData, 3));
                Util.ReleaseComObject(result);
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);
            }
            catch (Exception e)
            {
                _command.HxInstrumentDeck.DeleteSequence(PXSequence);
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);

        }
        /// <summary>
        /// Move Channel
        /// </summary>
        /// <param name="type">absolute or relate move</param>
        /// <param name="axis">move direction, x, y or z</param>
        /// <param name="distance">move distance</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Move(MovementType type, Direction axis, double distance, ErrorRecoveryOptions options = null)
        {
            string stepBaseName = "MoveToPosition";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);
            usedVariable.Add((int)axis, "MoveDirection");
            usedVariable.Add("", "SequenceObject");
            usedVariable.Add("", "SequenceName");
            usedVariable.Add(0, "MoveZEndMode");
            if (type == MovementType.Relative)
            {
                usedVariable.Add(1, "MoveType");
                usedVariable.Add(distance, "MoveRelDistance");
                usedVariable.Add(0, "MoveAbsDistance");
            }
            else
            {
                usedVariable.Add(0, "MoveType");
                usedVariable.Add(0, "MoveRelDistance");
                usedVariable.Add(distance, "MoveAbsDistance");
            }
            //usedVariable.Add((distance ), "instrument::yPosition");
            for (int i = 1; i <= Count; i++)
            {
                usedVariable.Add(0, HxAtsInstrumentParsKeys.channel, i, "MoveZEndDelta");
            }

            //设置错误处理
            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                StepResult results = StepResult.Parse(result.Item2(HxCommandKeys.resultData, 3));
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
        }
        /// <summary>
        /// Get liquid level for last operation
        /// </summary>
        /// <returns></returns>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public double[] GetLastLiquidLevel()
        {
            double[] levels = new double[Count];
            string stepBaseName = "GetLastLiquidLevel";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);

            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                StepResult results= StepResult.Parse(result.Item2(HxCommandKeys.resultData, 3));
                for (int i = 0; i < results.Blocks.Count; i++)
                    levels[i] = double.Parse(results.Blocks[i].StepData);
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            return levels;
        }
        /// <summary>
        /// set speed factor for pipetting
        /// </summary>
        /// <param name="factor">Speed Factor. Default set to 1 (one). Enter a value smaller than one to slow down respectively a value greater than one to speed up tip tracking during aspiration or dispensation. Considered resolution is 0.01.</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void SetTipTrackSpeed(double factor)
        {
            string stepBaseName = "OptimizeFollowSpeed";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);
            usedVariable.Add(1, "OptimizeForChannel");
            usedVariable.Add(factor, "OptimizeFactor");
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
        }
        /// <summary>
        /// get channel exclude state
        /// </summary>
        /// <returns>exclude state, true for excluded</returns>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public bool[] GetExcludeState()
        {
            string stepBaseName = "GetChannelExcludeState";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);
            usedVariable.Add(0, "ChannelType");
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                string pattern = result.Item(HxCommandKeys.resultData, 4);
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                bool[] c = new bool[pattern.Length];
                for (int i = 0; i < pattern.Length; i++)
                    c[i] = (pattern[i] == '1');
                return c;
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
        }
        /// <summary>
        /// wait for Tadm upload
        /// </summary>
        public void WaitForTadmUpload()
        {
            string stepBaseName = "WaitForTadmUpload";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            _command.GetRunParameter(usedVariable, instanceId);
            usedVariable.Add(0, "ChannelType");
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(channelTask);
                _command.ClearErrorForTask(channelTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
        }
        /// <summary>
        /// turn on or off Asipiration monitoring by pressure monitor (monitored air displacement)
        /// </summary>
        public bool AspirationMonitoring
        {
            set
            {
                int on = value ? 1 : 0;
                for (int i = 0; i < Count; i++)
                {
                    string[] status = _command.SendFirmware("P" + i + "RA", "raau").Split(' ');
                    _command.SendFirmware("P" + i + "AA", "au" + status[0] + " "+on+" " + status[2] + " " + status[3]);
                }
            }
        }

        /// <summary>
        ///  Turn on or off Clot detection monitoring with the cLLD
        /// </summary>
        public bool ClotDetectionMonitoring
        {
            set
            {
                int on = value ? 1 : 0;
                for (int i = 0; i < Count; i++)
                {
                    string[] status = _command.SendFirmware("P" + i + "RA", "raau").Split(' ');
                    _command.SendFirmware("P" + i + "AA", "au" + status[0] + " " + status[1] + " " + status[2] + " " + on);
                }
            }
        }
        /// <summary>
        /// Turn on or off Anti Droplet Control
        /// </summary>
        public bool AntiDropletControl
        {
            set
            {
                int on = value ? 1 : 0;
                _command.SendFirmware("PXAA", "bl"+on);
            }
            get
            {
                string reply = _command.SendFirmware("PXRA", "rabl");
                string parameter = reply.Substring(reply.IndexOf("bl") + 2,1);
                return "1".Equals(parameter);
            }
        }
        /// <summary>
        /// pressure threshold for clot detection by cLLD, value range 0 to 1023
        /// </summary>
        public int PressureThreshold
        {
            set
            {
                if (value <= 0 || value > 1023)
                    throw new Exception("Invalid parameter range, valid range is 0 to 1023");
                string parameter = value + "";
                while (parameter.Length < 4)
                {
                    parameter = "0" + parameter;
                }
                parameter = "+" + parameter;
                _command.SendFirmware("PXAA", "kc" + parameter);
            }
            get
            {
                string reply = _command.SendFirmware("PXRA", "rakc");
                string parameter = reply.Substring(reply.IndexOf("kc") + 2);
                if (parameter.StartsWith("+"))
                    parameter = parameter.Substring(1);
                return int.Parse(parameter);
            }
        }
    }
}
