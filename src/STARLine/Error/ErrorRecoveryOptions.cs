using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Error Reocvery Options for steps
    /// </summary>
    public class ErrorRecoveryOptions : Dictionary<MainErrorEnum, ErrorRecoveryOption>
    {
        static ErrorRecoveryOptions _cancel;
        /// <summary>
        /// options for cancel all the errors
        /// </summary>
        public static ErrorRecoveryOptions CancelAllErrors
        {
            get
            {
                if (_cancel == null)
                {
                    _cancel = new ErrorRecoveryOptions();
                    Type t = typeof(MainErrorEnum);
                    foreach (FieldInfo f in t.GetFields())
                    {
                        if (f.IsLiteral)
                        {
                            _cancel.Add((MainErrorEnum)f.GetValue(t), new ErrorRecoveryOption() { Recovery = RecoveryAction.Cancel });
                        }
                    }
                }
                return _cancel;
            }
        }
        /// <summary>
        /// Recovery Visibility
        /// </summary>
        public Dictionary<RecoveryAction, bool> RecoveryVisibility { get; internal set; } = new Dictionary<RecoveryAction, bool>();
    }
    /// <summary>
    /// Error Recovery Option for specified Error
    /// </summary>
    public class ErrorRecoveryOption
    {
        /// <summary>
        /// First Recovery
        /// </summary>
        public RecoveryAction Recovery { get;  set; } = RecoveryAction.None;
        /// <summary>
        /// Additional Recovery
        /// </summary>
        public RecoveryAction SecondRecovery { get; set; } = RecoveryAction.None;
        /// <summary>
        /// Repeatition Time
        /// </summary>
        public int Repeatition { get; set; } = 0;
    }
}
