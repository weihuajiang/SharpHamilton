using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    internal interface IProtoclSimulator
    {
        void Start(STARCommand device);
        void End();
        IAutoloadSimulator Autoload { get; }
        IPipettingChannelSimulator Channel1000 { get; }
        ICoreGripperSimulator CoreGripper1000 { get; }
        Task Initialize();
        Task SendFirmware(string command, string parameter);
    }
    /// <summary>
    /// inteface for CORE gripper simulator
    /// </summary>
    internal interface ICoreGripperSimulator
    {
        /// <summary>
        /// pick up gripper
        /// </summary>
        /// <param name="gripper">gripper rack</param>
        /// <param name="frontChannel">front channel, zero based</param>
        /// <returns></returns>
        Task PickupGripper(Rack gripper, int frontChannel);
        /// <summary>
        /// eject gripper
        /// </summary>
        /// <returns></returns>
        Task EjectGripper();
        /// <summary>
        /// get plate
        /// </summary>
        /// <param name="rack"></param>
        /// <param name="gripper">rack for core gripper. If it is null, system will search for available gripper rack</param>
        /// <param name="frontChannel"></param>
        /// <param name="gripHeight"></param>
        /// <param name="gripWidth"></param>
        /// <param name="openWidth"></param>
        /// <returns></returns>
        Task Get(Rack rack, Rack gripper, int frontChannel, double gripHeight, double gripWidth, double openWidth);
        /// <summary>
        /// place plate
        /// </summary>
        /// <param name="rack"></param>
        /// <param name="ejectGripper"></param>
        /// <returns></returns>
        Task Place(Rack rack, bool ejectGripper);
        /// <summary>
        /// move plate to rack
        /// </summary>
        /// <param name="rack"></param>
        /// <returns></returns>
        Task Move(Rack rack);
        /// <summary>
        /// read barcode with autoload
        /// </summary>
        /// <param name="track"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Task ReadBarcode(int track, double height);
    }
    internal interface IAutoloadSimulator
    {
        Task Load(Rack rack);
        Task Unload(Rack rack);
        Task Move(int track);
    }
    internal interface IPipettingChannelSimulator
    {
        Task PickupTips(Container[] tips);
        Task Pipette(Container[] containers, double position, double followHeight);
        Task Pipette(Container[] containers, double[] positions, double[] followHeights);
        Task EjectTips();
        Task EjectTips(Container[] tips);
    }
}
