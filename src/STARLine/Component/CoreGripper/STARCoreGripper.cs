using Hamilton.Interop.HxGruCommand;
using Hamilton.Interop.HxLabwr3;
using Hamilton.Interop.HxParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huarui.STARLine
{
    /// <summary>
    /// CORE Gripper
    /// </summary>
    public partial class CoreGripper
    {
        STARCommand _command;
        int coreGripperTask = 4;
        internal CoreGripper(STARCommand cmd)
        {
            _command = cmd;
        }

        #region setting
        /// <summary>
        /// Grip Force 0-9, default 5
        /// </summary>
        public GripperForceEnum GripForce { get; set; } = GripperForceEnum.Force5;//0-9
        /// <summary>
        /// X Acceleration Level 1-5, default 4
        /// </summary>
        public GripperXAccelerationLevel XAccelerationLevel { get; set; } = GripperXAccelerationLevel.Level4;
        /// <summary>
        /// Grip Speed (mm/s), default 277.8
        /// </summary>
        public double GripSpeed { get; set; } = 277.8;
        /// <summary>
        /// Z Speed (mm/s), default 128.7
        /// </summary>
        public double ZSpeed { get; set; } = 128.7;
        /// <summary>
        /// Front Channel Used to grip the plate, default channel 2
        /// </summary>
        public int FrontChannelUsed { get; set; } = 2;

        /// <summary>
        /// Collision control enabled
        /// </summary>
        public bool CollisionControl { get; set; } = false;
        #endregion

        void AddSequences(HxPars usedVariable, HxPars objBounds)
        {
            _command.AddSequences(usedVariable, objBounds);
        }
        string PlateSequenceName = "TransportPlateSourceSequence";
        string ToolSequenceName = "TransportToolSequence";


        internal static Rack FindCoreGripper(Rack rack, string property, int type)
        {
            if (rack.Properties.ContainsKey(property) && int.Parse(rack.Properties[property]) == type)
            {
                return rack;
            }
            foreach (var s in rack.Sites.Values)
            {
                foreach (var c in s.Racks)
                {
                    var r = FindCoreGripper(c, property, type);
                    if (r != null) return r;
                }
            }
            return null;
        }
        /// <summary>
        /// Get Plate of Core gripper
        /// </summary>
        /// <param name="plate">Plate to pick</param>
        /// <param name="gripHeight">grip heigh (from top)</param>
        /// <param name="gripWidth">grip width or plate width</param>
        /// <param name="openWidth">open width of gripper to access the plate</param>
        /// <param name="gripper">core gripper tool, if the rack is null, search for the default gripper rack</param>
        /// <param name="checkPlateExist">check plate existance with Z touch the plate</param>
        /// <param name="options">error recovery option</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Get(Rack plate, double gripHeight, double gripWidth, double openWidth, Rack gripper=null, bool checkPlateExist=false, ErrorRecoveryOptions options = null)
        {
            if (gripper == null)
                gripper = FindCoreGripper(_command.Deck, "MlStarTipRack", 14);
            if (plate == null)
                throw new Exception("Plate can not be null");
            if (gripper == null)
                throw new Exception("Can not find core gripper");
            if (!gripper.Properties.ContainsKey("MlStarTipRack") || int.Parse(gripper.Properties["MlStarTipRack"]) != 14)
                throw new Exception("gripper rack is not CORE gripper for 1000ul channel, or can not find core gripper rack");
            string stepBaseName = "ZSwapGetPlate";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;

            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();

            _command.GetRunParameter(usedVariable, instanceId);
            //Sequence
            IEditSequence2 seqPlate = new Sequence() as IEditSequence2;
            IEditSequence2 seqTool = new Sequence() as IEditSequence2;
            {//Plate Sequence
                Container c;
                if (plate.Containers.ContainsKey("1"))
                    c = plate.Containers["1"];
                else
                    c = plate.Containers["A1"];
                HxPars param = new HxPars();
                param.Add(0, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                seqPlate.AppendItem(param);
            }
            {
                //Tool Sequence
                Container c;
                for (int i = 2; i >= 1; i--)
                {
                    c = gripper.Containers[""+i];
                    HxPars param = new HxPars();
                    param.Add(0, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                    param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                    seqTool.AppendItem(param);
                }
            }
            (seqPlate as IIterateSequence3).index = 1;
            (seqTool as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PlateSequenceName, seqPlate);
            _command.HxInstrumentDeck.SaveSequence(ToolSequenceName, seqTool);


            usedVariable.Add(_command.InstrumentName + "." + PlateSequenceName, "PlateName");
            usedVariable.Add(_command.InstrumentName + "." + PlateSequenceName, "PlateObject");
            usedVariable.Add(6, _command.InstrumentName + "." + PlateSequenceName);
            usedVariable.Add(0, "SequenceCounting");
            usedVariable.Add("PlateObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PlateSequenceName, 0, 0);

            usedVariable.Add(_command.InstrumentName + "." + ToolSequenceName, "ToolSequenceName");
            usedVariable.Add(_command.InstrumentName + "." + ToolSequenceName, "ToolSequenceObject");
            usedVariable.Add(20, _command.InstrumentName + "." + ToolSequenceName);
            usedVariable.Add(0, "ToolSequenceCounting");
            usedVariable.Add("ToolSequenceObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + ToolSequenceName, 0, 0);


            usedVariable.Add("", "CapName");
            usedVariable.Add("", "CapObject");


            usedVariable.Add(gripHeight, "GripperAccessHeight");
            usedVariable.Add(checkPlateExist?1:0, "ZSwapPlateCheck");//TO DO
            usedVariable.Add(0.5, "ZSwapStrengthFactor");
            usedVariable.Add(FrontChannelUsed, "ToolUpperChannel");
            usedVariable.Add(0, "TransportMode");
            usedVariable.Add(2, "OverwriteGripWdth");
            usedVariable.Add(openWidth, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.openWidth);
            usedVariable.Add(gripWidth, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.plateWidth);
            usedVariable.Add((int)GripForce, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.gripStrength);
            usedVariable.Add(ZSpeed, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.zSpeed);
            usedVariable.Add(GripSpeed, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.zSwapGripSpeed);
            usedVariable.Add(gripHeight, "GripperAccessHeight");

            AddSequences(usedVariable, objBounds);

            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task gTask = null;
                if (_command.Simulator != null && _command.Simulator.CoreGripper1000 != null && _command.IsSimulation)
                    gTask = _command.Simulator.CoreGripper1000.Get(plate, gripper, FrontChannelUsed, gripHeight, gripWidth, openWidth);

                _command.ClearErrorForTask(coreGripperTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, coreGripperTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);

                if (gTask != null) gTask.Wait();
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                _command.HxInstrumentDeck.DeleteSequence(PlateSequenceName);
                _command.HxInstrumentDeck.DeleteSequence(ToolSequenceName);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqPlate);
                Util.ReleaseComObject(seqTool);
                ModuleErrors errors = _command.GetErrorTask(coreGripperTask);
                _command.ClearErrorForTask(coreGripperTask);
                throw new STARException(e.Message, e, errors);
            }
            _command.HxInstrumentDeck.DeleteSequence(PlateSequenceName);
            _command.HxInstrumentDeck.DeleteSequence(ToolSequenceName);
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqPlate);
            Util.ReleaseComObject(seqTool);
        }

        /// <summary>
        /// Move plate to position
        /// </summary>
        /// <param name="position">plate position</param>
        /// <param name="options">error recovery option</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Move(Rack position, ErrorRecoveryOptions options = null)
        {
            if (position == null)
                throw new Exception("Plate or Gripper can not be null");
            string stepBaseName = "ZSwapMovePlate";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;

            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();

            _command.GetRunParameter(usedVariable, instanceId);
            //Sequence
            IEditSequence2 seqPlate = new Sequence() as IEditSequence2;
            {//Plate Sequence
                Container c;
                if (position.Containers.ContainsKey("1"))
                    c = position.Containers["1"];
                else
                    c = position.Containers["A1"];
                HxPars param = new HxPars();
                param.Add(0, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                seqPlate.AppendItem(param);
            }
            (seqPlate as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PlateSequenceName, seqPlate);


            usedVariable.Add(_command.InstrumentName + "." + PlateSequenceName, "PlateName");
            usedVariable.Add(_command.InstrumentName + "." + PlateSequenceName, "PlateObject");
            usedVariable.Add(7, _command.InstrumentName + "." + PlateSequenceName);
            usedVariable.Add("PlateObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PlateSequenceName, 0, 0);

            usedVariable.Add("", "CapName");
            usedVariable.Add("", "CapObject");
            
            usedVariable.Add((int)XAccelerationLevel, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.xAccelerationFactor);

            AddSequences(usedVariable, objBounds);

            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task gTask = null;
                if (_command.Simulator != null && _command.Simulator.CoreGripper1000 != null && _command.IsSimulation)
                    gTask = _command.Simulator.CoreGripper1000.Move(position);

                _command.ClearErrorForTask(coreGripperTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, coreGripperTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);

                if (gTask != null) gTask.Wait();
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                _command.HxInstrumentDeck.DeleteSequence(PlateSequenceName);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqPlate);
                ModuleErrors errors = _command.GetErrorTask(coreGripperTask);
                _command.ClearErrorForTask(coreGripperTask);
                throw new STARException(e.Message, e, errors);
            }
            _command.HxInstrumentDeck.DeleteSequence(PlateSequenceName);
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqPlate);
        }
        /// <summary>
        /// Read plate barcode with autoload barcode reader
        /// </summary>
        /// <param name="track">track to move plate and read</param>
        /// <param name="minZPosition">min z postion when move plate down</param>
        /// <param name="options">error recovery option</param>
        /// <returns>barcode</returns>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public string ReadBarcode(int track, double minZPosition=219, ErrorRecoveryOptions options = null)
        {
            string stepBaseName = "ZSwapReadBarcode";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            string barcode = "";
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();

            _command.GetRunParameter(usedVariable, instanceId);

            usedVariable.Add(track, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.carrierPosition);
            usedVariable.Add(minZPosition, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.readBarcZpos);

            AddSequences(usedVariable, objBounds);

            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task gTask = null;
                if (_command.Simulator != null && _command.Simulator.CoreGripper1000 != null && _command.IsSimulation)
                    gTask = _command.Simulator.CoreGripper1000.ReadBarcode(track, minZPosition);

                _command.ClearErrorForTask(coreGripperTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, coreGripperTask, objBounds) as HxPars;
                //Util.TraceIHxPars(result);
                barcode = result.Item(HxCommandKeys.resultData, 4);
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);

                if (gTask != null) gTask.Wait();
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                ModuleErrors errors = _command.GetErrorTask(coreGripperTask);
                _command.ClearErrorForTask(coreGripperTask);
                throw new STARException(e.Message, e, errors);
            }
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            return barcode;
        }

        /// <summary>
        /// Place plate
        /// </summary>
        /// <param name="plate">plate position</param>
        /// <param name="ejectToolAfter">eject core gripper to picking position after operation</param>
        /// <param name="checkPlateExist">check plate existance with Z touch the plate</param>
        /// <param name="platePushDownDistance">pushing distance when placing plate</param>
        /// <param name="options">error recovery options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Place(Rack plate, bool ejectToolAfter=false, bool checkPlateExist=false, double platePushDownDistance=1, ErrorRecoveryOptions options = null)
        {
            if (plate == null)
                throw new Exception("Plate or Gripper can not be null");
            string stepBaseName = "ZSwapPlacePlate";
            StepRunData srd = _command.AllSteps[stepBaseName];
            string instanceId = srd.DataId;

            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();

            _command.GetRunParameter(usedVariable, instanceId);
            //Sequence
            IEditSequence2 seqPlate = new Sequence() as IEditSequence2;
            {//Plate Sequence
                Container c;
                if (plate.Containers.ContainsKey("1"))
                    c = plate.Containers["1"];
                else
                    c = plate.Containers["A1"];
                HxPars param = new HxPars();
                param.Add(0, "Labwr_Item", null, null, null, null, null, null, null, null, null);
                param.Add(c.Labware, "Labwr_Id", null, null, null, null, null, null, null, null, null);
                param.Add(c.Position, "Labwr_PosId", null, null, null, null, null, null, null, null, null);
                seqPlate.AppendItem(param);
            }
            (seqPlate as IIterateSequence3).index = 1;
            _command.HxInstrumentDeck.SaveSequence(PlateSequenceName, seqPlate);


            usedVariable.Add(_command.InstrumentName + "." + PlateSequenceName, "PlateName");
            usedVariable.Add(_command.InstrumentName + "." + PlateSequenceName, "PlateObject");
            usedVariable.Add(7, _command.InstrumentName + "." + PlateSequenceName);
            usedVariable.Add(0, "SequenceCounting");
            usedVariable.Add("PlateObject", "Variables", HxCommandKeys.sequenceNames, _command.InstrumentName + "." + PlateSequenceName, 0, 0);            

            usedVariable.Add("", "CapName");
            usedVariable.Add("", "CapObject");

            
            usedVariable.Add(checkPlateExist ? 1 : 0, "ZSwapPlateCheck");
            usedVariable.Add(0, "TransportMode");
            usedVariable.Add(ejectToolAfter ? 1 : 0, "ToolEject");
            
            usedVariable.Add(ZSpeed, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.zSpeed);
            usedVariable.Add((int)XAccelerationLevel, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.xAccelerationFactor);
            usedVariable.Add(platePushDownDistance, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.platePushDownDistance);

            AddSequences(usedVariable, objBounds);

            if (options != null)
                _command.SetupErrorRecovery(usedVariable, options);
            _command.SaveRunParameter(usedVariable, instanceId);
            try
            {
                Task gTask = null;
                if (_command.Simulator != null && _command.Simulator.CoreGripper1000 != null && _command.IsSimulation)
                    gTask = _command.Simulator.CoreGripper1000.Place(plate, ejectToolAfter);

                _command.ClearErrorForTask(coreGripperTask);
                HxPars result = srd.Command.Run(_command.InstrumentName, instanceId, coreGripperTask, objBounds) as HxPars;
                Util.ReleaseComObject(result);
                _command.DeleteRunParameter(instanceId);

                if (gTask != null) gTask.Wait();
            }
            catch (Exception e)
            {
                _command.DeleteRunParameter(instanceId);
                _command.HxInstrumentDeck.DeleteSequence(PlateSequenceName);
                Util.ReleaseComObject(usedVariable);
                Util.ReleaseComObject(objBounds);
                Util.ReleaseComObject(seqPlate);
                ModuleErrors errors = _command.GetErrorTask(coreGripperTask);
                _command.ClearErrorForTask(coreGripperTask);
                throw new STARException(e.Message, e, errors);
            }
            _command.HxInstrumentDeck.DeleteSequence(PlateSequenceName);
            Util.ReleaseComObject(usedVariable);
            Util.ReleaseComObject(objBounds);
            Util.ReleaseComObject(seqPlate);
        }
    }
}
