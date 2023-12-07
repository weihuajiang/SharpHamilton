using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    /// <summary>
    /// container list used for replacing sequence
    /// </summary>
    public class ContainerSequence : List<Container>
    {
        int? _end = null;
        /// <summary>
        /// Current Position, like current position of sequence, 1 based
        /// </summary>
        public int Current { get; set; } = 1;
        /// <summary>
        /// End position, like end position of sequence
        /// </summary>
        public int End { get
            {
                if (_end == null) return Count;
                if (_end > Count) return -1;
                if (_end <= 0) return -1;
                return _end.Value;
            }
            set { _end = value;
                if (_end > Count) _end= -1;
                if (_end <= 0) _end= -1;
            }
        }
        /// <summary>
        /// Increment sequence by offset
        /// </summary>
        /// <param name="size"></param>
        public void Increment(int size)
        {
            if (Current <= 0) return;
            Current += size;
            if (Current > End)
                Current = -1;
            if (Current < 1) Current = -1;
        }
        /// <summary>
        /// get container array with specific size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public Container[] Get(int size)
        {
            Container[] cnts = new Container[size];
            for(int i = 0; i < size; i++)
            {
                if (Current + i >= 1 && Current + i < End)
                    cnts[i] = this[Current + i - 1];
                else
                    cnts[i] = null;
            }
            return cnts;
        }
        /// <summary>
        /// get container array from sequence
        /// </summary>
        /// <param name="size"></param>
        /// <param name="cnts"></param>
        public void Get(Container[] cnts, int size)
        {
            if (cnts == null) return;
            for (int i = 0; i < cnts.Length; i++) cnts[i] = null;
            for (int i = 0; i < Math.Min(size, cnts.Length); i++)
            {
                if (Current + i >= 1 && Current + i < End)
                    cnts[i] = this[Current + i - 1];
                else
                    cnts[i] = null;
            }
        }
        /// <summary>
        /// get container array from sequence
        /// </summary>
        /// <param name="cnts"></param>
        public void Get(Container[] cnts)
        {
            if (cnts == null) return;
            for (int i = 0; i < cnts.Length; i++)
            {
                if (Current + i >= 1 && Current + i < End)
                    cnts[i] = this[Current + i - 1];
                else
                    cnts[i] = null;
            }
        }
    }
    /// <summary>
    /// Array of container extension, to convert array of sequence to container sequence
    /// </summary>
    public static class SequenceExtension
    {
        /// <summary>
        /// convert container array to sequence
        /// </summary>
        /// <param name="cnts"></param>
        /// <returns></returns>
        public static ContainerSequence ToSeqnece(this Container[] cnts)
        {
            ContainerSequence seq = new ContainerSequence();
            foreach (var c in cnts)
                seq.Add(c);
            return seq;
        }
    }
}
