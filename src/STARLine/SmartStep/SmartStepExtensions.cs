using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine.SmartStep
{
    /// <summary>
    /// Tip usage for smart step
    /// </summary>
    public enum TipUsageType
    {
        /// <summary>
        /// tip will be ejected after each dispense
        /// </summary>
        AfterEachDispense,
        /// <summary>
        /// tip will be ejected after each sample was processed
        /// </summary>
        AfterSampleProcess,
        /// <summary>
        /// use one set of tip for whole pipette
        /// </summary>
        OneSetForFullPipette,
        /// <summary>
        /// use tips picked up before step
        /// </summary>
        UseTipPickedUpBefore
    }
    /// <summary>
    /// Start step extension for STAR
    /// </summary>
    public static class SmartStepExtensions
    {
        static int ChannelsInPattern(string pattern)
        {
            int size = 0;
            for (int i = 0; i < pattern.Length; i++)
                if (pattern[i] == '1') size++;
            return size;
        }
        static string GetPattern(string pattern, int size)
        {
            while (ChannelsInPattern(pattern) > size)
            {
                for (int i = pattern.Length - 1; i >= 0; i--)
                {
                    if (pattern[i] == '1')
                    {
                        pattern = pattern.Substring(0, i) + "0" + pattern.Substring(i + 1);
                        break;
                    }
                }
            }
            return pattern;
        }
        static double GetTimeVolume(IParameter parameter)
        {
            var maxVolume = 1000;
            switch (parameter.LiquidClassParameter.TipType)
            {
                case CoreTipType.StandardVolumeTip:
                case CoreTipType.StandardVolumeTipFiltered:
                case CoreTipType.SlimTip300ul:
                case CoreTipType.SlimTip300ulFiltered:
                    maxVolume = 300; break;
                case CoreTipType.HighVolumeTip:
                case CoreTipType.HighVolumeTipFiltered:
                    maxVolume = 1000; break;
                case CoreTipType.Tip50ul:
                case CoreTipType.TipFiltered50ul:
                    maxVolume = 50; break;
                case CoreTipType.Tip5ml:
                    maxVolume = 5000; break;
                case CoreTipType.Tip4mlFiltered:
                    maxVolume = 4000; break;
            }
            return maxVolume;
        }
        /// <summary>
        /// Smart Step: simple, 1 to 1
        /// </summary>
        /// <param name="ML_STAR">STAR instrument</param>
        /// <param name="source">source sequence</param>
        /// <param name="target">target sequence</param>
        /// <param name="volume">volume for pipetting</param>
        /// <param name="aspParameter">aspirate parameter</param>
        /// <param name="dispParameter">dispense parameter</param>
        /// <param name="tips">tip sequence</param>
        /// <param name="pattern">channel pattern, if specific pattern used</param>
        /// <param name="tipUsage">tip usage for smart step</param>
        /// <param name="resetSourceSeq">reset source sequence (current position) when complete</param>
        /// <param name="resetTargetSeq">reset target sequence (current position) when complete</param>
        public static void Simple(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips,
            string pattern = null, TipUsageType tipUsage = TipUsageType.AfterSampleProcess, bool resetSourceSeq = true, bool resetTargetSeq = true)
        {
            int currentSource = source.Current;
            int currentTarget = target.Current;
            int count = ML_STAR.Channel.Count;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, Math.Min(source.PositionsLeft, target.PositionsLeft)); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            var aspParameters = new IParameter[count];
            var disParameters = new IParameter[count];
            for (int i = 0; i < count; i++)
            {
                aspParameters[i] = aspParameter;
                disParameters[i] = dispParameter;
            }
            int times = 1;
            var maxVolume = GetTimeVolume(aspParameter);
            times = (int)Math.Ceiling(volume / maxVolume);
            double eachVolume = volume / times;
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.PickupTip(tips, true, pattern);
            while (source.Current > 0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                if (tipUsage == TipUsageType.AfterSampleProcess)
                    ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                for (int i = 1; i <= times; i++)
                {
                    if (tipUsage == TipUsageType.AfterEachDispense)
                        ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                    ML_STAR.Channel.Aspirate(source, eachVolume, aspParameters, autoCounting: (i == times), pattern: stepPattern);
                    ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: (i == times), pattern: stepPattern);
                    if (tipUsage == TipUsageType.AfterEachDispense)
                        ML_STAR.Channel.EjectTip();
                }
                if (tipUsage == TipUsageType.AfterSampleProcess)
                    ML_STAR.Channel.EjectTip();
            }
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.EjectTip();
            if (resetSourceSeq) source.Current = currentSource;
            if (resetTargetSeq) target.Current = currentTarget;
        }
        /// <summary>
        /// Smart step - Simple 1- 1
        /// </summary>
        /// <param name="ML_STAR">STAR instrument</param>
        /// <param name="source">source sequence</param>
        /// <param name="target">target sequence</param>
        /// <param name="volumes">volume array</param>
        /// <param name="aspParameter">aspirate parameter</param>
        /// <param name="dispParameter">dispense parameter</param>
        /// <param name="tips">tip sequence</param>
        /// <param name="pattern">channel pattern, if specific pattern used</param>
        /// <param name="tipUsage">tip usage for smart step</param>
        /// <param name="resetSourceSeq">reset source sequence (current position) when complete</param>
        /// <param name="resetTargetSeq">reset target sequence (current position) when complete</param>
        public static void Simple(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
           double[] volumes, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null,
           TipUsageType tipUsage = TipUsageType.AfterSampleProcess,
           bool resetSourceSeq = true, bool resetTargetSeq = true)
        {
            if (volumes == null || source.Current == -1 || target.Current == -1 || volumes.Length < Math.Min(source.PositionsLeft, target.PositionsLeft))
            {
                throw new Exception("wrong sequence or volume array");
            }
            int count = ML_STAR.Channel.Count;
            int currentSource = source.Current;
            int currentTarget = target.Current;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, Math.Min(source.PositionsLeft, target.PositionsLeft)); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            var aspParameters = new IParameter[count];
            var disParameters = new IParameter[count];
            for (int i = 0; i < count; i++)
            {
                aspParameters[i] = aspParameter;
                disParameters[i] = dispParameter;
            }
            var maxVolume = GetTimeVolume(aspParameter);
            var eachVolumes = new double[count];
            var stepVolumes = new double[count];
            int number = 0;
            int current = 0;
            int index = 0;
            bool allZero = true;
            string stepPattern1 = "";
            ContainerSequence sourceSeq = new ContainerSequence();
            ContainerSequence targetSeq = new ContainerSequence();
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.PickupTip(tips, true, pattern);
            while (source.Current > 0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                number = ChannelsInPattern(pattern);
                index = 0;
                for (int i = 0; i < count; i++)
                {
                    if (i < stepPattern.Length && stepPattern[i] == '1')
                    {
                        eachVolumes[i] = volumes[current + index];
                        stepVolumes[i] = eachVolumes[i] / (Math.Ceiling(eachVolumes[i] / maxVolume));
                        index++;
                    }
                    else
                        eachVolumes[i] = 0;
                }
                if (tipUsage == TipUsageType.AfterSampleProcess)
                    ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                while (true)
                {
                    stepPattern1 = "";
                    sourceSeq.Clear();
                    targetSeq.Clear();
                    index = 0;
                    for (int i = 0; i < count; i++)
                    {
                        if (Math.Abs(stepVolumes[i]) < 0.0001) stepPattern1 = stepPattern1 + "0";
                        else stepPattern1 = stepPattern1 + "1";

                        if (i < stepPattern.Length && stepPattern[i] == '1')
                        {
                            if (Math.Abs(stepVolumes[i]) > 0.0001)
                            {
                                sourceSeq.Add(source[current + index]);
                                targetSeq.Add(target[current + index]);
                            }
                            index++;
                        }
                    }
                    if (tipUsage == TipUsageType.AfterEachDispense)
                        ML_STAR.Channel.PickupTip(tips, true, stepPattern1);
                    ML_STAR.Channel.Aspirate(sourceSeq, stepVolumes, aspParameters, autoCounting: false, pattern: stepPattern1);
                    ML_STAR.Channel.Dispense(targetSeq, stepVolumes, disParameters, autoCounting: false, pattern: stepPattern1);
                    if (tipUsage == TipUsageType.AfterEachDispense)
                        ML_STAR.Channel.EjectTip();
                    allZero = true;
                    for (int i = 0; i < count; i++)
                    {
                        eachVolumes[i] -= stepVolumes[i];
                        if (Math.Abs(eachVolumes[i]) < 0.0001) eachVolumes[i] = 0;
                        if (eachVolumes[i] == 0) stepVolumes[i] = 0;
                        else allZero = false;
                    }
                    if (allZero) break;
                }
                if (tipUsage == TipUsageType.AfterSampleProcess)
                    ML_STAR.Channel.EjectTip();
                source.Increment(number);
                target.Increment(number);
                current += number;
            }
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.EjectTip();
            if (resetSourceSeq) source.Current = currentSource;
            if (resetTargetSeq) target.Current = currentTarget;
        }
        /// <summary>
        /// Smart Step: Replicate, 1 to n
        /// </summary>
        /// <param name="ML_STAR">STAR instrument</param>
        /// <param name="source">source sequence</param>
        /// <param name="target">target sequence</param>
        /// <param name="volume">volume for pipetting</param>
        /// <param name="aspParameter">aspirate parameter</param>
        /// <param name="dispParameter">dispense parameter</param>
        /// <param name="tips">tip sequence</param>
        /// <param name="pattern">channel pattern, if specific pattern used</param>
        /// <param name="tipUsage">tip usage for smart step</param>
        /// <param name="resetSourceSeq">reset source sequence (current position) when complete</param>
        /// <param name="resetTargetSeq">reset target sequence (current position) when complete</param>
        public static void Replicate(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null,
            TipUsageType tipUsage = TipUsageType.AfterSampleProcess,
            bool resetSourceSeq = true, bool resetTargetSeq = true)
        {
            if (target.PositionsLeft % source.PositionsLeft != 0) throw new Exception("sequence number was not right for replicate");
            int n = target.PositionsLeft / source.PositionsLeft;
            int count = ML_STAR.Channel.Count;
            int currentSource = source.Current;
            int currentTarget = target.Current;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, Math.Min(source.PositionsLeft, target.PositionsLeft)); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            var aspParameters = new IParameter[count];
            var disParameters = new IParameter[count];
            for (int i = 0; i < count; i++)
            {
                aspParameters[i] = aspParameter;
                disParameters[i] = dispParameter;
            }
            int times = 1;
            var maxVolume = GetTimeVolume(aspParameter);
            times = (int)Math.Ceiling(volume / maxVolume);
            double eachVolume = volume / times;
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.PickupTip(tips, true, pattern);
            while (source.Current > 0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                if (tipUsage == TipUsageType.OneSetForFullPipette)
                    ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                for (int j = 1; j <= n; j++)
                {
                    for (int i = 1; i <= times; i++)
                    {
                        if (tipUsage == TipUsageType.AfterEachDispense)
                            ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                        ML_STAR.Channel.Aspirate(source, eachVolume, aspParameters, autoCounting: (i == times && j == n), pattern: stepPattern);
                        ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: (i == times), pattern: stepPattern);
                        if (tipUsage == TipUsageType.AfterEachDispense)
                            ML_STAR.Channel.EjectTip();
                    }
                }
                if (tipUsage == TipUsageType.OneSetForFullPipette)
                    ML_STAR.Channel.EjectTip();
            }
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.EjectTip();
            if (resetSourceSeq) source.Current = currentSource;
            if (resetTargetSeq) target.Current = currentTarget;
        }

        /// <summary>
        /// Smart Step: Pool, n to 1
        /// </summary>
        /// <param name="ML_STAR">STAR instrument</param>
        /// <param name="source">source sequence</param>
        /// <param name="target">target sequence</param>
        /// <param name="volume">volume for pipetting</param>
        /// <param name="aspParameter">aspirate parameter</param>
        /// <param name="dispParameter">dispense parameter</param>
        /// <param name="tips">tip sequence</param>
        /// <param name="pattern">channel pattern, if specific pattern used</param>
        /// <param name="tipUsage">tip usage for smart step</param>
        /// <param name="resetSourceSeq">reset source sequence (current position) when complete</param>
        /// <param name="resetTargetSeq">reset target sequence (current position) when complete</param>
        public static void Pool(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null,
            TipUsageType tipUsage = TipUsageType.AfterSampleProcess,
            bool resetSourceSeq = true, bool resetTargetSeq = true)
        {
            if (source.PositionsLeft % target.PositionsLeft != 0) throw new Exception("sequence number was not right for replicate");
            int n = source.PositionsLeft / target.PositionsLeft;
            int count = ML_STAR.Channel.Count;
            int currentSource = source.Current;
            int currentTarget = target.Current;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, Math.Min(source.PositionsLeft, target.PositionsLeft)); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            var aspParameters = new IParameter[count];
            var disParameters = new IParameter[count];
            for (int i = 0; i < count; i++)
            {
                aspParameters[i] = aspParameter;
                disParameters[i] = dispParameter;
            }
            int times = 1;
            var maxVolume = GetTimeVolume(aspParameter);
            times = (int)Math.Ceiling(volume / maxVolume);
            double eachVolume = volume / times;
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.PickupTip(tips, true, pattern);
            while (source.Current > 0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                for (int j = 1; j <= n; j++)
                {
                    if (tipUsage == TipUsageType.AfterSampleProcess)
                        ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                    for (int i = 1; i <= times; i++)
                    {
                        if (tipUsage == TipUsageType.AfterEachDispense)
                            ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                        ML_STAR.Channel.Aspirate(source, eachVolume, aspParameters, autoCounting: (i == times), pattern: stepPattern);
                        ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: (i == times && j == n), pattern: stepPattern);
                        if (tipUsage == TipUsageType.AfterEachDispense)
                            ML_STAR.Channel.EjectTip();
                    }
                    if (tipUsage == TipUsageType.AfterSampleProcess)
                        ML_STAR.Channel.EjectTip();
                }
            }
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.EjectTip();
            if (resetSourceSeq) source.Current = currentSource;
            if (resetTargetSeq) target.Current = currentTarget;
        }

        /// <summary>
        /// Smart Step: Aliquot
        /// </summary>
        /// <param name="ML_STAR">STAR instrument</param>
        /// <param name="source">source sequence</param>
        /// <param name="target">target sequence</param>
        /// <param name="volume">volume for pipetting</param>
        /// <param name="aspParameter">aspirate parameter</param>
        /// <param name="dispParameter">dispense parameter</param>
        /// <param name="tips">tip sequence</param>
        /// <param name="pattern">channel pattern, if specific pattern used</param>
        /// <param name="tipUsage">tip usage for smart step</param>
        /// <param name="resetSourceSeq">reset source sequence (current position) when complete</param>
        /// <param name="resetTargetSeq">reset target sequence (current position) when complete</param>
        public static void Aliquot(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null,
            TipUsageType tipUsage = TipUsageType.OneSetForFullPipette,
            bool resetSourceSeq = true, bool resetTargetSeq = true)
        {
            int count = ML_STAR.Channel.Count;
            int currentSource = source.Current;
            int currentTarget = target.Current;
            if (pattern == null)
            {
                pattern = "";
                for (int i = 0; i < Math.Min(count, Math.Min(source.PositionsLeft, target.PositionsLeft)); i++)
                    pattern = pattern + "1";
            }
            if (pattern.Length < count)
            {
                for (int i = pattern.Length; i < count; i++)
                    pattern = pattern + "0";
            }
            var aspParameters = new IParameter[count];
            var disParameters = new IParameter[count];
            for (int i = 0; i < count; i++)
            {
                aspParameters[i] = aspParameter;
                disParameters[i] = dispParameter;
            }
            int times = 1;
            var maxVolume = GetTimeVolume(aspParameter);
            double eachVolume = volume / times;
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.PickupTip(tips, true, pattern);
            double[] volumes = new double[count];
            double[] disVolumes = new double[count];
            while (target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                for (int i = 0; i < volumes.Length; i++)
                {
                    volumes[i] = 0;
                    disVolumes[i] = 0;
                }
                int left = target.PositionsLeft;
                int d = 0;
                for (int i = 0; i < left; i++)
                {
                    while (d >= count || stepPattern[d] == '0')
                    {
                        d++;
                        if (d >= count) d = 0;
                    }
                    if (volumes[d] + volume > maxVolume) break;
                    volumes[d] += volume;
                    d++;
                }
                if (tipUsage == TipUsageType.AfterEachDispense || tipUsage == TipUsageType.AfterSampleProcess)
                    ML_STAR.Channel.PickupTip(tips, true, pattern);
                ML_STAR.Channel.Aspirate(source, volumes, aspParameters, autoCounting: false, pattern: stepPattern);
                string disPattern = "";
                while (true)
                {
                    bool allZero = true;
                    for (int i = 0; i < volumes.Length; i++)
                    {
                        if (volumes[i] > 0) { disVolumes[i] = volume; volumes[i] -= volume; disPattern = disPattern + "1"; allZero = false; }
                        else { disVolumes[i] = 0; disPattern = disPattern + "0"; }
                    }
                    if (allZero) break;
                    ML_STAR.Channel.Dispense(target, disVolumes, disParameters, autoCounting: true, pattern: disPattern);
                }
                if (tipUsage == TipUsageType.AfterEachDispense || tipUsage == TipUsageType.AfterSampleProcess)
                    ML_STAR.Channel.EjectTip();
            }
            if (tipUsage == TipUsageType.OneSetForFullPipette)
                ML_STAR.Channel.EjectTip();
            if (resetSourceSeq) source.Current = currentSource;
            if (resetTargetSeq) target.Current = currentTarget;
        }
    }
}
