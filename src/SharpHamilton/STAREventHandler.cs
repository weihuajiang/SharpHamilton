using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    public delegate void STAREventHandler();
    public delegate void STARReceiveEventHandler(object receiveInfo, int receiveCount);
    public delegate void STARSystemDeckCreatedEventHandler(object systemDeck);
    public delegate void STARInstrumentDeckCreatedEventHandler(object instrumentDeck, string name);
    public delegate void STARDeviceCreatedEventHandler(object device, string name);
    public delegate void STARDeviceErrorOccuredEventHandler(ModuleErrors errors, string deviceName);
}
