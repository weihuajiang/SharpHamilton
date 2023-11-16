using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Channel. All the operation were implemented by the firmware
    /// </summary>
    public partial class Channel
    {
        /// <summary>
        /// Channel max speed of Y axis in steps/second, range 20 to 8000
        /// </summary>
        public int YSpeed
        {
            get
            {
                if (_command.IsSimulation)
                    return 2700;
                string response = _command.SendFirmware("PXRA", "rayv");
                return int.Parse(response.Substring(response.IndexOf("yv") + 2, 4));
            }
            set
            {
                if (_command.IsSimulation)
                    return;
                if (value < 20 || value > 8000)
                    throw new ArgumentOutOfRangeException("YSpeed");
                _command.SendFirmware("PXAA", "yv" + Util.FormatInt(value, 4));
            }
        }
        /// <summary>
        /// Acceleration ramp of Y axis in 5000 steps/second2, range 1 to 4, default 4
        /// </summary>
        public int YAcceleration
        {
            get
            {
                if (_command.IsSimulation)
                    return 1;
                string response = _command.SendFirmware("PXRA", "rayr");
                return int.Parse(response.Substring(response.IndexOf("yr") +2, 1));
            }
            set
            {
                if (_command.IsSimulation)
                    return;
                if (value < 1 || value > 4)
                    throw new ArgumentOutOfRangeException("YAcceleration");
                _command.SendFirmware("PXAA", "yr" + value);
            }
        }
        /// <summary>
        /// Channel max speed of Z axis in step/second, range 20 to 15000, default 12000
        /// </summary>
        public int ZSpeed
        {
            get
            {
                if (_command.IsSimulation)
                    return 2700;
                string response = _command.SendFirmware("PXRA", "razv");
                return int.Parse(response.Substring(response.IndexOf("zv") + 2, 5));
            }
            set
            {
                if (_command.IsSimulation)
                    return;
                if (value < 20 || value > 15000)
                    throw new ArgumentOutOfRangeException("ZSpeed");
                _command.SendFirmware("PXAA", "zv" + Util.FormatInt(value, 5));
            }
        }
        /// <summary>
        /// Acceleration ramp of Z axis in 1000 steps/second2, range 5 to 150, default 75
        /// </summary>
        public int ZAcceleration
        {
            get
            {
                if (_command.IsSimulation)
                    return 75;
                string response = _command.SendFirmware("PXRA", "razr");
                return int.Parse(response.Substring(response.IndexOf("zr") + 2, 3));
            }
            set
            {
                if (_command.IsSimulation)
                    return;
                if (value < 5 || value > 150)
                    throw new ArgumentOutOfRangeException("ZAcceleration");
                _command.SendFirmware("PXAA", "zr" + Util.FormatInt(value, 3));
            }
        }
        /// <summary>
        /// Pickup tips with firmware
        /// </summary>
        /// <param name="tips">tip position</param>
        private void PickupTipEx(Container[] tips)
        {
            string xp = "xp";
            string yp = "yp";
            string tm = "tm";
            string parameter = "";
            int tt=-1;
            double tz = 0;
            for(int i = 0; i < Count; i++)
            {
                if (tips[i] != null)
                {
                    xp += Util.FormatInt((int)(tips[i].X * 10), 5);
                    yp += Util.FormatInt((int)(tips[i].Y * 10), 4);
                    tm += "1";
                    if (tz == 0)
                        tz = tips[i].LiquidSeekingStartPosition;
                    if(tt==-1)
                        tt = int.Parse(_command.Deck[tips[i].Labware].Properties["MlStarTipRack"]);
                    else
                    {
                        int typeType= int.Parse(_command.Deck[tips[i].Labware].Properties["MlStarTipRack"]);
                        if (tt != typeType)
                        {
                            throw new Exception("Not the same tip types in tips");
                        }
                    }
                }
                else
                {
                    xp += "00000";
                    yp += "0000";
                    tm += "0";
                }
                if (i < Count - 1)
                {
                    xp += " ";
                    yp += " ";
                    tm += " ";
                }
            }
            parameter += xp + yp + tm + "tt" + Util.FormatInt( tt, 2) + "tz" + Util.FormatInt((int)(tz * 10), 4)+"tp"+
                Util.FormatInt((int)(tz*10-100), 4)+ "th2450td1";
            string response = _command.SendFirmware("C0TP", parameter);
            ModuleErrors me = Util.ParseResponse(response);
            if (me != null)
                throw new STARException("Failed to Pick up Channel", null, me);
            _command.SendFirmware("C0RT", "");
        }
        Rack FindDefaultWaste(Rack r)
        {
            if(r.Properties!=null && r.Properties.ContainsKey("MlStarIsDefaultWasteRack"))
            {
                return r;
            }
            foreach(var childs in r.Sites.Values){
                foreach(Rack c in childs.Racks)
                {
                    Rack result = FindDefaultWaste(c);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
        Rack defaultWaste = null;
        double wasteMaxY;
        double wasteMinY;
        double wasteX;
        double wasteZ;
        /// <summary>
        /// Eject tips with firmware
        /// </summary>
        /// <param name="tips">tip position. eject to default waste if null</param>
        private void EjectTipsEx(Container[] tips = null)
        {
            string xp = "xp";
            string yp = "yp";
            string tm = "tm";
            string parameter = "";
            double tz = 0;
            if (tips != null)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (tips[i] != null)
                    {
                        xp += Util.FormatInt((int)(tips[i].X * 10), 5);
                        yp += Util.FormatInt((int)(tips[i].Y * 10), 4);
                        tm += "1";
                        if (tz == 0)
                            tz = tips[i].Z;
                    }
                    else
                    {
                        xp += "00000";
                        yp += "0000";
                        tm += "0";
                    }
                    if (i < Count - 1)
                    {
                        xp += " ";
                        yp += " ";
                        tm += " ";
                    }
                }
            }
            else
            {
                if (defaultWaste == null)
                {
                    defaultWaste = FindDefaultWaste(_command.Deck);
                    Container[] defaultContainer = defaultWaste.Containers.Values.ToArray();
                    for(int i=0;i<defaultContainer.Length-1;i++)
                        for(int j = i + 1; j < defaultContainer.Length; j++)
                        {
                            if (defaultContainer[i].Y < defaultContainer[j].Y)
                            {
                                Container c = defaultContainer[i];
                                defaultContainer[i] = defaultContainer[j];
                                defaultContainer[j] = c;
                            }
                        }
                    wasteX = defaultContainer[0].X;
                    wasteZ = defaultContainer[0].Z;
                    wasteMaxY = defaultContainer[0].Y;
                    wasteMinY = defaultContainer[defaultContainer.Length - 1].Y;
                }
                xp += Util.FormatInt((int)(wasteX * 10), 5)+"&";
                tz = wasteZ;
                tm += "1&";
                for(int i = 0; i < Count; i++)
                {
                    yp += Util.FormatInt((int)((wasteMaxY - (wasteMaxY - wasteMinY) * i / Count) * 10), 4);
                    if (i < Count - 1)
                        yp += " ";
                }

            }
            parameter += xp + yp  + "tz" + Util.FormatInt((int)(tz * 10+100), 4) + "tp" +
                Util.FormatInt((int)(tz * 10), 4) + "th2450te2450" + tm + "ti"+(tips==null?0:1);
            Console.WriteLine(parameter);
            string response = _command.SendFirmware("C0TR", parameter);
            ModuleErrors me = Util.ParseResponse(response);
            if (me != null)
                throw new STARException("Failed to eject tips", null, me);
            _command.SendFirmware("C0RT", "");
        }
    }
}
