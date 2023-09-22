using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Left arm
    /// </summary>
    internal class LeftArm : Arm
    {
        STARCommand _command;
        internal LeftArm(STARCommand ML_STAR)
        {
            _command = ML_STAR;
        }
        /// <summary>
        /// get left arm position
        /// </summary>
        public override double Position
        {
            get
            {
                string reply = _command.SendFirmware("C0RX", "");
                return int.Parse(reply.Substring(reply.IndexOf("rx") + 1, 5)) / 10.0;
            }
        }
        /// <summary>
        /// Speed in 0.1mm /sec (optimal 3 shoots / sec), range 20 to 999, default 270
        /// </summary>
        public override int Speed
        {
            get
            {
                if (_command.IsSimulation)
                    return 270;
                string response = _command.SendFirmware("X0RA", "ralv");
                return int.Parse(response.Substring(response.IndexOf("lv") + 2,3));
            }
            set
            {
                if (_command.IsSimulation)
                    return;
                if (value < 20 || value > 999)
                    throw new ArgumentOutOfRangeException("Speed");
                _command.SendFirmware("X0AA", "lv" + Util.FormatInt(value, 2));
            }
        }
        /// <summary>
        /// Index of acceleration curve, range 1 to 5, default 4
        /// </summary>
        public override int Acceleration
        {
            get
            {
                if (_command.IsSimulation)
                    return 4;
                string response = _command.SendFirmware("X0RA", "ralr");
                return int.Parse(response.Substring(response.IndexOf("lr") + 2,1)); ;
            }
            set
            {
                if (_command.IsSimulation)
                    return;
                if (value < 1 || value > 5)
                    throw new ArgumentOutOfRangeException("Acceleration");
                _command.SendFirmware("X0AA", "lr" + value);
            }
        }

        /// <summary>
        /// Move left arm
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="useZSafeHeight">Move X-arm to position with all attached components in Z-safety position</param>
        public override void Move(double position, bool useZSafeHeight = true)
        {
            string paramter = "xs" + Util.FormatInt((int)(position * 10), 5);
            if(useZSafeHeight)
                _command.SendFirmware("C0KX", paramter);
            else
                _command.SendFirmware("C0JX", paramter);
        }
    }
}
