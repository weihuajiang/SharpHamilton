using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine.SmartStep
{
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
                for(int i=pattern.Length-1;i>=0;i--)
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
        public static void Simple(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target, 
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null)
        {
            int count = ML_STAR.Channel.Count;
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
            while(source.Current>0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                for (int i = 1; i <= times; i++)
                {
                    ML_STAR.Channel.Aspirate(source, eachVolume, aspParameters, autoCounting:(i==times), pattern:stepPattern);
                    ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: (i == times), pattern:stepPattern);
                }
                ML_STAR.Channel.EjectTip();
            }

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
        public static void Replicate(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null)
        {
            if (target.PositionsLeft % source.PositionsLeft != 0) throw new Exception("sequence number was not right for replicate");
            int n = target.PositionsLeft / source.PositionsLeft;
            int count = ML_STAR.Channel.Count;
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
            while (source.Current > 0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                for (int j = 1; j <= n; j++)
                {
                    for (int i = 1; i <= times; i++)
                    {
                        ML_STAR.Channel.Aspirate(source, eachVolume, aspParameters, autoCounting: (i==times && j==n), pattern: stepPattern);
                        ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: (i == times), pattern: stepPattern);
                    }
                }
                ML_STAR.Channel.EjectTip();
            }
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
        public static void Pool(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null)
        {
            if (source.PositionsLeft % target.PositionsLeft != 0) throw new Exception("sequence number was not right for replicate");
            int n = source.PositionsLeft / target.PositionsLeft;
            int count = ML_STAR.Channel.Count;
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
            while (source.Current > 0 && target.Current > 0)
            {
                var stepPattern = GetPattern(pattern, Math.Min(source.PositionsLeft, target.PositionsLeft));
                for (int j = 1; j <= n; j++)
                {
                    ML_STAR.Channel.PickupTip(tips, true, stepPattern);
                    for (int i = 1; i <= times; i++)
                    {
                        ML_STAR.Channel.Aspirate(source, eachVolume, aspParameters, autoCounting: (i == times), pattern: stepPattern);
                        ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: (i == times && j == n), pattern: stepPattern);
                    }
                    ML_STAR.Channel.EjectTip();
                }
            }
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
        public static void Aliquot(this STARCommand ML_STAR, ContainerSequence source, ContainerSequence target,
            double volume, IParameter aspParameter, IParameter dispParameter, ContainerSequence tips, string pattern = null)
        {
            int count = ML_STAR.Channel.Count;
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
            ML_STAR.Channel.PickupTip(tips, true, pattern);
            double[] volumes =new double[count];
            double[] disVolumes = new double[count];
            var usecount = ChannelsInPattern(pattern);
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
                for(int i = 0; i < left; i++)
                {
                    while(d>=count || stepPattern[d] == '0')
                    {
                        d++;
                        if (d >= count) d = 0;
                    }
                    if (volumes[d] + volume > maxVolume) break;
                    volumes[d] += volume;
                    d++;
                }
                ML_STAR.Channel.Aspirate(source, volumes, aspParameters, autoCounting: false, pattern: stepPattern);
                string disPattern = "";
                while (true)
                {
                    bool allZero = true;
                    for(int i = 0; i < volumes.Length; i++)
                    {
                        if (volumes[i] > 0) { disVolumes[i] = volume; volumes[i] -= volume; disPattern = disPattern + "1"; allZero = false; }
                        else { disVolumes[i] = 0; disPattern = disPattern + "0"; }
                    }
                    if (allZero) break;
                    ML_STAR.Channel.Dispense(target, disVolumes, disParameters, autoCounting: true, pattern: disPattern);
                }
                /*
                int max = target.PositionsLeft / usecount;
                if (target.PositionsLeft < usecount) max = 1;
                times = (int)Math.Min(max, (int)( maxVolume / volume));

                ML_STAR.Channel.Aspirate(source, eachVolume * times, aspParameters, autoCounting: false, pattern: stepPattern);
                for (int i = 1; i <= times; i++)
                {
                    ML_STAR.Channel.Dispense(target, eachVolume, disParameters, autoCounting: true, pattern: stepPattern);
                }
                */
            }
            ML_STAR.Channel.EjectTip();
        }
    }
}
