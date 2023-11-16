using Hamilton.Interop.HxCfgFil;
using Hamilton.Interop.HxReg;
using HXRS232COMLib;
using HXTCPIPBDZCOMMLib;
using HXUSBCOMMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// STAR discover tools
    /// </summary>
    /// <example>
    /// it can be used to monitor the connection status of instrument, following example show you how to do it.
    /// <code language="cs">
    /// //creating an instance and attach instrument connection event.
    /// STARDiscover discover = new STARDiscover();
    /// discover.OnConnect += Discover_OnConnect;
    /// discover.OnDisconnect += Discover_OnDisconnect;
    /// discover.OnReceive += Discover_OnReceive;
    /// discover.Start();
    /// </code>
    /// 
    /// When you want to control the instrument, you must stop monitor and release the connection
    /// <code language="cs">
    /// //stop monitoring when instrument connected and you want to control the instrument
    /// discover.Stop();
    /// discover.OnConnect -= Discover_OnConnect;
    /// discover.OnDisconnect -= Discover_OnDisconnect;
    /// discover.OnReceive -= Discover_OnReceive;
    /// discover.Dispose();
    /// discover = null;
    ///</code>
    /// </example>
    public class STARDiscover : IDisposable
    {
        CommuInterfaceEnum communicationInterface;
        object commObj;
        /// <summary>
        /// Construction
        /// </summary>
        public STARDiscover()
        {
        }
        /// <summary>
        /// Instrument Connect status
        /// </summary>
        public bool IsConnect
        {
            get; internal set;
        } = false;
        /// <summary>
        /// Stop discover
        /// </summary>
        public void Stop()
        {
            if (communicationInterface == CommuInterfaceEnum.TCPIP)
            {
                HxTcpIpBdzComm obj = commObj as HxTcpIpBdzComm;
                obj.OnConnect -= Obj_OnConnect;
                obj.OnDisconnect -= Obj_OnDisconnect;
                obj.OnReceive -= Obj_OnReceive;
            }
            else if (communicationInterface == CommuInterfaceEnum.RS232)
            {
                HxRs232Com obj = commObj as HxRs232Com;
                obj.OnConnect -= Obj_OnConnect;
                obj.OnDisconnect -= Obj_OnDisconnect;
                obj.OnReceive -= Obj_OnReceive1;
            }
            else
            {
                HxUsbComm obj = commObj as HxUsbComm;
                obj.OnConnect -= Obj_OnConnect;
                obj.OnDisconnect -= Obj_OnDisconnect;
                obj.OnReceive -= Obj_OnReceive;
            }
            Util.ReleaseComObject(commObj);
            commObj = null;
        }
        /// <summary>
        /// Start Discover
        /// </summary>
        public void Start() { 
            HxRegistry registry = new HxRegistry();
            //init ML_STAR
            string cmdRunCfgFil = "";
            registry.InstCfgFile("ML_STAR", ref cmdRunCfgFil);
            cmdRunCfgFil = registry.ConfigPath + "\\" + cmdRunCfgFil;
            HxCfgFile configFile = new HxCfgFile();
            configFile.LoadFile(cmdRunCfgFil);
            communicationInterface = LoadCommunicationType(configFile);
            if (communicationInterface == CommuInterfaceEnum.TCPIP)
            {
                HxTcpIpBdzComm obj = new HxTcpIpBdzComm();
                obj.OnConnect += Obj_OnConnect;
                obj.OnDisconnect += Obj_OnDisconnect;
                obj.OnReceive += Obj_OnReceive;
                commObj = obj;
                obj.InitFromCfgFil(configFile);
            }
            else if (communicationInterface == CommuInterfaceEnum.RS232)
            {
                HxRs232Com obj = new HxRs232Com();
                obj.OnConnect += Obj_OnConnect;
                obj.OnDisconnect += Obj_OnDisconnect;
                obj.OnReceive += Obj_OnReceive1;
                commObj = obj;
                obj.InitFromCfgFil(configFile);
            }
            else
            {
                HxUsbComm obj = new HxUsbComm();
                obj.OnConnect += Obj_OnConnect;
                obj.OnDisconnect += Obj_OnDisconnect;
                obj.OnReceive += Obj_OnReceive;
                commObj = obj;
                obj.InitFromCfgFil(configFile);
            }
            if (IsConnect && OnConnect!=null)
                OnConnect();
            Util.ReleaseComObject(registry);
            Util.ReleaseComObject(configFile);
        }

        private void Obj_OnReceive1(HxComEventsResult commResult, object receiveInfo, int receiveCount)
        {
            if (OnReceive != null)
            {
                OnReceive(receiveInfo, receiveCount);
            }
        }

        private void Obj_OnReceive(object receiveInfo, int receiveCount)
        {
            if (OnReceive != null)
            {
                OnReceive(receiveInfo, receiveCount);
            }
        }

        private void Obj_OnDisconnect()
        {
            IsConnect = false;
            if (OnDisconnect != null)
                OnDisconnect();
        }

        private void Obj_OnConnect()
        {
            IsConnect = true;
            if (OnConnect != null)
                OnConnect();
        }

        private CommuInterfaceEnum LoadCommunicationType(IHxCfgFile5 pInstrCfgFile)
        {
            string defValueAsString = pInstrCfgFile.GetDataDefValueAsString("MLSTARInstrument", "default", "ServerCLSID");
            return !(defValueAsString == "{FEE4FC08-E545-11D3-B842-002035848439}") ? (!(defValueAsString == "{206068AC-E65F-4243-AEE5-E8854150B8DC}") ? CommuInterfaceEnum.TCPIP :CommuInterfaceEnum.USB) : CommuInterfaceEnum.RS232;

        }
        /// <summary>
        /// Send command to STAR
        /// </summary>
        /// <param name="sendInfo"></param>
        public void Send(string sendInfo)
        {
            if (communicationInterface == CommuInterfaceEnum.TCPIP)
            {
                HxTcpIpBdzComm obj = commObj as  HxTcpIpBdzComm;
                obj.Send(sendInfo);
            }
            else if (communicationInterface == CommuInterfaceEnum.RS232)
            {
                HxRs232Com obj = commObj as HxRs232Com;
                obj.Send(sendInfo);
            }
            else
            {
                HxUsbComm obj = commObj as HxUsbComm;
                obj.Send(sendInfo);
            }
        }
        /// <summary>
        /// Dispose the instance
        /// </summary>
        public void Dispose()
        {
            Util.ReleaseComObject(commObj);
        }
        /// <summary>
        /// device connected event
        /// </summary>
        public event STAREventHandler OnConnect;
        /// <summary>
        /// device disconnected event
        /// </summary>
        public event STAREventHandler OnDisconnect;
        /// <summary>
        /// reply from device received event
        /// </summary>
        public event STARReceiveEventHandler OnReceive;
    }
    enum CommuInterfaceEnum
    {
        RS232,
        USB,
        TCPIP,
    }
}
