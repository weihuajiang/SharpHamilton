using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    public interface IProtoclSimulator
    {
        void Start(STARCommand device);
        void End();
        IAutoloadSimulator Autoload { get; }
        IPipettingChannelSimulator Channel1000 { get; }
        ICoreGripperSimulator CoreGripper1000 { get; }
        Task Initialize();
        Task SendFirmware(string command, string parameter);
    }
    public interface ICoreGripperSimulator
    {
        /// <summary>
        /// pick up gripper
        /// </summary>
        /// <param name="frontChannel">front channel, zero based</param>
        /// <returns></returns>
        Task PickupGripper(int frontChannel);
        Task EjectGripper();
        Task Get(Rack rack, int frontChannel, double gripHeight, double gripWidth, double openWidth);
        Task Place(Rack rack, bool ejectGripper);
        Task Move(Rack rack);
        Task ReadBarcode(int track, double height);
    }
    public interface IAutoloadSimulator
    {
        Task Load(Rack rack);
        Task Unload(Rack rack);
        Task Move(int track);
    }
    public interface IPipettingChannelSimulator
    {
        Task PickupTips(Container[] tips);
        Task Pipette(Container[] containers, double position, double followHeight);
        Task Pipette(Container[] containers, double[] positions, double[] followHeights);
        Task EjectTips();
        Task EjectTips(Container[] tips);
    }
}
