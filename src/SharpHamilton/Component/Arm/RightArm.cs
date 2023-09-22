using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Right arm
    /// </summary>
    internal class RightArm : Arm
    {
        STARCommand _command;
        internal RightArm(STARCommand ML_STAR)
        {
            _command = ML_STAR;
        }
        /// <summary>
        /// get right arm position
        /// </summary>
        public override double Position
        {
            get
            {
                string reply = _command.SendFirmware("C0QX", "");
                return int.Parse(reply.Substring(reply.IndexOf("rx") + 2)) / 10.0;
            }
        }
        /// <summary>
        /// Speed in 0.1mm /sec (optimal 3 shoots / sec), range 20 to 999, default 270
        /// </summary>
        public override int Speed
        {
            get
            {
                string response = _command.SendFirmware("X0RA", "rasv");
                return 270;
            }
            set
            {
                if (value < 20 || value > 999)
                    throw new ArgumentOutOfRangeException("Speed");
                _command.SendFirmware("X0AA", "sv" + Util.FormatInt(value, 3));
            }
        }
        /// <summary>
        /// Index of acceleration curve, range 1 to 5, default 4
        /// </summary>
        public override int Acceleration
        {
            get
            {
                string response = _command.SendFirmware("X0RA", "rasr");
                return 4;
            }
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentOutOfRangeException("Acceleration");
                _command.SendFirmware("X0AA", "sr" + value);
            }
        }
        /// <summary>
        /// Move right arm
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="useZSafeHeight">Move X-arm to position with all attached components in Z-safety position</param>
        public override void Move(double position, bool useZSafeHeight = true)
        {
            string paramter = "xs" + Util.FormatInt((int)(position * 10), 5);
            if (useZSafeHeight)
                _command.SendFirmware("C0KR", paramter);
            else
                _command.SendFirmware("C0JS", paramter);
        }
    }
}
