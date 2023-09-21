using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// STAR step result
    /// </summary>
    class StepResult
    {
        /// <summary>
        /// Error Flag
        /// </summary>
        public StepResultErrorFlag ErrorFlag
        {
            get;internal set;
        }
        /// <summary>
        /// Blocks
        /// </summary>
        public List<StepResultEntry> Blocks
        {
            get; internal set;
        } = new List<StepResultEntry>();

        /// <summary>
        /// Parse the step result from STAR
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static StepResult Parse(string result)
        {
            string[] blocks = result.Split(new string[] { "[" }, StringSplitOptions.None);
            StepResult r = new StepResult();
            r.ErrorFlag = (StepResultErrorFlag)int.Parse(blocks[0]);
            for(int i = 1; i < blocks.Length; i++)
            {
                StepResultEntry entry = new StepResultEntry();
                string[] b = blocks[i].Split(new string[] { "," }, StringSplitOptions.None);
                entry.Position = int.Parse(b[0]);
                entry.MainError = int.Parse(b[1]);
                entry.SlaveError = int.Parse(b[2]);
                entry.Recovery = (RecoveryAction)int.Parse(b[3]);
                entry.StepData = b[4];
                entry.LabwareName = b[5];
                entry.LabwarePos = b[6];
                r.Blocks.Add(entry);
            }
            return r;
        }
    }
    /// <summary>
    /// Step block entry
    /// </summary>
    public class StepResultEntry
    {
        /// <summary>
        /// Position
        /// </summary>
        public int Position
        {
            get;internal set;
        }
        /// <summary>
        /// Main Error
        /// </summary>
        public int MainError
        {
            get; internal set;
        }

        /// <summary>
        /// Slave Error
        /// </summary>
        public int SlaveError
        {
            get; internal set;
        }
        /// <summary>
        /// Error Recovery Used
        /// </summary>
        public RecoveryAction Recovery
        {
            get;internal set;
        }

        /// <summary>
        /// Step Data, e.g. barcode for load
        /// </summary>
        public string StepData
        {
            get;internal set;
        }
        /// <summary>
        /// Labware Id
        /// </summary>
        public string LabwareName
        { get; internal set; }
        /// <summary>
        /// Position Id
        /// </summary>
        public string LabwarePos
        { get; internal set; }
    }
}
