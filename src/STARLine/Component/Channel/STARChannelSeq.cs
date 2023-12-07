using Hamilton.Interop.HxGruCommand;
using Hamilton.Interop.HxLabwr3;
using Hamilton.Interop.HxParams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Container = Huarui.STARLine.Container;

namespace Huarui.STARLine
{
    public enum ChannelUseType
    {
        AllPosition=1,
        ChannelPattern=2,
    }
    public partial class Channel
    {
        /// <summary>
        /// Pickup tips
        /// </summary>
        /// <param name="tips">tip position</param>
        /// <param name="autoCounting">auto counting for current position</param>
        /// <param name="pattern">channel pattern, string of 1 and 0</param>
        /// <param name="useType">channel use type</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void PickupTip(ContainerSequence tips, bool autoCounting = true, string pattern = null, ChannelUseType useType= ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            if (tips.Current <= 0 || tips.End == 0 || tips.Count == 0) throw new Exception("no positions in sequence");
            //string pattern = "";
            List<int> channel = new List<int>();
            int count = Count;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min( count, tips.End-tips.Current+1); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = tips.Current-1; i < tips.End; i++)
            {
                Container c = tips[i];
                if (c != null)
                {
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
            }
            var tipsForSim = new Container[count];
            int nullNumber = 0;
            for(int i = 0; i < count; i++)
            {
                index = tips.Current + i;
                if (pattern[i]=='1' && index <= tips.End && tips[index - 1] != null)
                {
                    channel.Add(1);
                    tipsForSim[i] = tips[index - 1];
                }
                else
                {
                    channel.Add(0);
                    tipsForSim[i] = null;
                    nullNumber++;
                }
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
            usedVariable.Add((int)useType, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
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
                    chnTask = _command.Simulator.Channel1000.PickupTips(tipsForSim);
                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                if(autoCounting)
                    tips.Increment(result.Item2(HxCommandKeys.sequenceData, _command.InstrumentName + "." + PXSequence));
                //Util.TraceIHxPars(result);
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
        /// Eject tips to containers
        /// </summary>
        /// <param name="tips">containers to drop the tips</param>
        /// <param name="autoCounting">sequence automatic counting</param>
        /// <param name="pattern">channel pattern, string of 1 and 0</param>
        /// <param name="useType">channel used type</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void EjectTip(ContainerSequence tips, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            if (tips == null)
            {
                EjectTip();
                return;
            }
            if (tips.Current <= 0 || tips.End == 0 || tips.Count == 0) throw new Exception("no positions in sequence");
            List<int> channel = new List<int>();
            int count = Count;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, tips.End - tips.Current + 1); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = tips.Current - 1; i < tips.End; i++)
            {
                Container c = tips[i];
                if (c != null)
                {
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
            }
            var tipsForSim = new Container[count];
            int nullNumber = 0;
            for (int i = 0; i < count; i++)
            {
                index = tips.Current + i;
                if (pattern[i] == '1' && index <= tips.End && tips[index - 1] != null)
                {
                    channel.Add(1);
                    tipsForSim[i] = tips[index - 1];
                }
                else
                {
                    channel.Add(0);
                    tipsForSim[i] = null;
                    nullNumber++;
                }
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
            usedVariable.Add((int)useType, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
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
                    chnTask = _command.Simulator.Channel1000.EjectTips(tipsForSim);

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                if (autoCounting)
                    tips.Increment(result.Item2(HxCommandKeys.sequenceData, _command.InstrumentName + "." + PXSequence));
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
        /// Aspriate
        /// </summary>
        /// <param name="cnts">sequence</param>
        /// <param name="volume">volume</param>
        /// <param name="parameter">aspirate parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double volume, IParameter[] parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] pts = new PipettingParameter[parameter.Length];
            for (int i = 0; i < parameter.Length; i++)
                pts[i] = SetParameter(parameter[i]);
            double[] vs = new double[Count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volume;
            AspirateImpl(cnts, vs, pts, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Aspirate
        /// </summary>
        /// <param name="cnts">sequence</param>
        /// <param name="volumes">volumes</param>
        /// <param name="parameter">aspirate parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double[] volumes, IParameter[] parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] pts = new PipettingParameter[parameter.Length];
            for (int i = 0; i < parameter.Length; i++)
                pts[i] = SetParameter(parameter[i]);
            AspirateImpl(cnts, volumes, pts, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// aspirate with liquid level detection
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double volume, LLDsParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volume, pt, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// aspirate with liquid level detection
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double[] volumes, LLDsParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volumes, pt, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// aspirate with fix height
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">aspirate mode</param>
        //// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double volume, FixHeightParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volume, pt, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// aspirate with fix height
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double[] volumes, FixHeightParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volumes, pt, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// aspirate with Touch off
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double volume, TouchOffParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volume, pt, mode, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// aspirate with touch off
        /// </summary>
        /// <param name="cnts">aspirate positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">aspirate mode</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Aspirate(ContainerSequence cnts, double[] volumes, TouchOffParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            Aspirate(cnts, volumes, pt, mode, autoCounting, pattern, useType, options);
        }
        private void Aspirate(ContainerSequence cnts, double volumes, PipettingParameter parameters, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            int count = Count;
            double[] vs = new double[count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volumes;
            Aspirate(cnts, vs, parameters, mode, autoCounting, pattern, useType, options);
        }
        private void Aspirate(ContainerSequence cnts, double[] volumes, PipettingParameter parameter, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] parameters = new PipettingParameter[Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = parameter;
            AspirateImpl(cnts, volumes, parameters, mode, autoCounting, pattern, useType, options);
        }
        private void AspirateImpl(ContainerSequence cnts, double[] volumes, PipettingParameter[] parameters, AspirateMode mode = AspirateMode.Aspiration, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            List<int> channel = new List<int>();
            int count = Count;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, cnts.End - cnts.Current + 1); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = cnts.Current - 1; i < cnts.End; i++)
            {
                Container c = cnts[i];
                if (c != null)
                {
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
            }
            var cntsForSim = new Container[count];
            int nullNumber = 0;
            for (int i = 0; i < count; i++)
            {
                index = cnts.Current + i;
                if (pattern[i] == '1' && index <= cnts.End && cnts[index - 1] != null)
                {
                    channel.Add(1);
                    cntsForSim[i] = cnts[index - 1];
                }
                else
                {
                    channel.Add(0);
                    cntsForSim[i] = null;
                    nullNumber++;
                }
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
            usedVariable.Add((int)useType, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
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
                    chnTask = _command.Simulator.Channel1000.Pipette(cntsForSim, GetPipettPositions(cntsForSim, volumes, parameters), GetFollowingHeights(cntsForSim, volumes, parameters, true));

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, channelTask, objBounds) as HxPars;
                if (autoCounting)
                    cnts.Increment(result.Item2(HxCommandKeys.sequenceData, _command.InstrumentName + "." + PXSequence));
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
        /// <param name="cnts">dispense sequence</param>
        /// <param name="volume">volume</param>
        /// <param name="parameters">dispense parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double volume, IParameter[] parameters, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            bool optimizZMove = false;
            PipettingParameter[] pts = new PipettingParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                pts[i] = SetParameter(parameters[i]);
            double[] vs = new double[Count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volume;
            DispenseImpl(cnts, vs, pts, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with different parameter of each channel
        /// </summary>
        /// <param name="cnts">dispense sequence</param>
        /// <param name="volumes">volumes</param>
        /// <param name="parameters">dispense parameters</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double[] volumes, IParameter[] parameters, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            bool optimizZMove = false;
            PipettingParameter[] pts = new PipettingParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                pts[i] = SetParameter(parameters[i]);
            DispenseImpl(cnts, volumes, pts, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with LLD
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double volume, CLLDParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with LLD
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">LLD parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double[] volumes, CLLDParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with fix height
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double volume, FixHeightParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with fix height
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">fix height parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double[] volumes, FixHeightParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with touch off
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double volume, TouchOffParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with touch off
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">touch off parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double[] volumes, TouchOffParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, false, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with side touch
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volume">liquid volume</param>
        /// <param name="parameter">side touch parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double volume, SideTouchParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volume, pt, mode, useFirstAspirtateLiquidClass, true, optimizZMove, autoCounting, pattern, useType, options);
        }
        /// <summary>
        /// Dispense with side touch
        /// </summary>
        /// <param name="cnts">dispense positions</param>
        /// <param name="volumes">liquid volumes</param>
        /// <param name="parameter">side touch parameter</param>
        /// <param name="mode">dispense mode</param>
        /// <param name="useFirstAspirtateLiquidClass">use first aspirate liquid class, otherwise use liquid class in the parameter</param>
        /// <param name="autoCounting">automatic counting of sequence</param>
        /// <param name="pattern">channel pattern</param>
        /// <param name="useType">use type of channel</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Dispense(ContainerSequence cnts, double[] volumes, SideTouchParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass, bool useFirstAspirtateLiquidClass = true
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter pt = SetParameter(parameter);
            bool optimizZMove = (pt.ZMoveAfterDispense == ZMoveAfterDispense.Minimized);
            Dispense(cnts, volumes, pt, mode, useFirstAspirtateLiquidClass, true, optimizZMove, autoCounting, pattern, useType, options);
        }
        private void Dispense(ContainerSequence cnts, double volume, PipettingParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass,
           bool useFirstAspirtateLiquidClass = true, bool sideTouch = false, bool optimizeZMove = false
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            int count = Count;
            double[] vs = new double[count];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = volume;
            Dispense(cnts, vs, parameter, mode, useFirstAspirtateLiquidClass, sideTouch, optimizeZMove, autoCounting, pattern, useType, options);
        }
        private void Dispense(ContainerSequence cnts, double[] volumes, PipettingParameter parameter, DispenseMode mode = DispenseMode.FromLiquiClass,
           bool useFirstAspirtateLiquidClass = true, bool sideTouch = false, bool optimizeZMove = false
            , bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            PipettingParameter[] parameters = new PipettingParameter[Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = parameter;
            DispenseImpl(cnts, volumes, parameters, mode, useFirstAspirtateLiquidClass, sideTouch, optimizeZMove, autoCounting, pattern, useType, options);
        }


        private void DispenseImpl(ContainerSequence cnts, double[] volumes, PipettingParameter[] parameters, DispenseMode mode = DispenseMode.FromLiquiClass,
            bool useFirstAspirtateLiquidClass = true, bool sideTouch = false, bool optimizeZMove = false, bool autoCounting = true, string pattern = null, ChannelUseType useType = ChannelUseType.AllPosition, ErrorRecoveryOptions options = null)
        {
            List<int> channel = new List<int>();
            int count = Count;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, cnts.End - cnts.Current + 1); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            IEditSequence2 seqc = new Sequence() as IEditSequence2;
            int index = 0;
            for (int i = cnts.Current - 1; i < cnts.End; i++)
            {
                Container c = cnts[i];
                if (c != null)
                {
                    HxPars param = new HxPars();
                    param.Add(index, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    index++;
                    seqc.AppendItem(param);
                }
            }
            var cntsForSim = new Container[count];
            int nullNumber = 0;
            for (int i = 0; i < count; i++)
            {
                index = cnts.Current + i;
                if (pattern[i] == '1' && index <= cnts.End && cnts[index - 1] != null)
                {
                    channel.Add(1);
                    cntsForSim[i] = cnts[index - 1];
                }
                else
                {
                    channel.Add(0);
                    cntsForSim[i] = null;
                    nullNumber++;
                }
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
            usedVariable.Add((int)useType, "Optimizing channel use");// 1 for all sequence positon, 2 for channel pattern
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
                    chnTask = _command.Simulator.Channel1000.Pipette(cntsForSim, GetPipettPositions(cntsForSim, volumes, parameters), GetFollowingHeights(cntsForSim, volumes, parameters, false));

                _command.ClearErrorForTask(channelTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, 2, objBounds) as HxPars;
                if (autoCounting)
                    cnts.Increment(result.Item2(HxCommandKeys.sequenceData, _command.InstrumentName + "." + PXSequence));
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
    }
}
