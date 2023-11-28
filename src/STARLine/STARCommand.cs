using Hamilton.HxSys3DView;
using Hamilton.HxVectorDb;
using Hamilton.Interop.HxCfgFil;
using Hamilton.Interop.HxCoreLiquid;
using Hamilton.Interop.HxGruCommand;
using Hamilton.Interop.HxLabwr3;
using Hamilton.Interop.HxParams;
using Hamilton.Interop.HxReg;
using Hamilton.Interop.HxSysDeck;
using Hamilton.Interop.HxTrace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Hamilton STAR line integration SDK
/// </summary>
namespace Huarui.STARLine
{
    /// <summary>
    /// STAR command wrapper
    /// </summary>
    public partial class STARCommand : IDisposable
    {
        internal IHxGruCommandRun6 MlSTAR;
        internal HxRegistry Registry;
        object dbTracking;
        HxTrace trace;
        string runId;
        //all the steps
        internal Dictionary<string, StepRunData> AllSteps = new Dictionary<string, StepRunData>();
        HxCfgFile StepParamCfg;
        HxCfgFile StepRunCfg;
        bool methodEnded = false;

        //trace file writer
        TextWriter logger;
        bool isHamiltonLogger = false;//default hamilton log file

        internal SystemDeck HxSystemDeck;
        internal DeckLayout HxInstrumentDeck { get; set; }
        internal Dictionary<RecoveryAction, int> titleToRecoveries = new Dictionary<RecoveryAction, int>();

        /// <summary>
        /// Log for ML_STAR. By default log will be written to hamilton log files. you can redirect to other destination
        /// </summary>
        public TextWriter Log
        {
            get { return logger; }
            set
            {
                logger = value; isHamiltonLogger = false;
            }
        }

        /// <summary>
        /// Instrumet name, e.q. ML_STAR
        /// </summary>
        public string InstrumentName
        {
            get; private set;
        } = "ML_STAR";
        /// <summary>
        /// Run Id of current run
        /// </summary>
        public string RunId
        {
            get { return runId; }
        }
        /// <summary>
        /// Name of current run
        /// </summary>
        public string RunName { get; internal set; } = "VenusWrapper";
        private int masterTask = 1;
        //台面布局，用来提供给操作者
        /// <summary>
        /// Deck layout
        /// </summary>
        public Rack Deck { get; private set; }

        /// <summary>
        /// simulator for STAR control
        /// </summary>
        public IProtoclSimulator Simulator { get; set; } = null;

        //Error handling to get the error
        internal Dictionary<int, ModuleErrors> Errors = new Dictionary<int, ModuleErrors>();
        internal void ClearErrorForTask(int task)
        {
            if (Errors.ContainsKey(task))
                Errors.Remove(task);
        }
        internal void SetErrorForTask(int task, ModuleErrors err)
        {
            Errors.Add(task, err);
        }
        internal ModuleErrors GetErrorTask(int task)
        {
            if (Errors.ContainsKey(task))
                return Errors[task];
            return null;
        }

        /// <summary>
        /// TADDM feature installed 
        /// </summary>
        public bool IsTADMActive
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// Sample Tracking function enabled
        /// </summary>
        public bool IsSampleTrackingEnabled
        {
            get
            {
                if (dbTracking == null)
                    return false;
                return (dbTracking as IHxVectorDbConfiguration).Enabled;
            }
        }
        #region Components
        /// <summary>
        /// Pipetting channel 1000ul
        /// </summary>
        public Channel Channel
        {
            private set;
            get;
        }
        /// <summary>
        /// Left Arm
        /// </summary>
        public Arm LeftArm
        {
            private set;
            get;
        }
        /// <summary>
        /// Right Arm
        /// </summary>
        public Arm RightArm
        {
            private set;
            get;
        }
        #endregion
        /// <summary>
        /// Star command construction
        /// </summary>
        public STARCommand()
        {
            Type t = typeof(RecoveryAction);
            foreach(FieldInfo f in t.GetFields())
            {
                if (f.IsLiteral)
                {
                    ReocveryTitleAttribute rt = f.GetCustomAttributes(typeof(ReocveryTitleAttribute), false)[0] as ReocveryTitleAttribute;
                    titleToRecoveries.Add((RecoveryAction)f.GetValue(t), rt.Title);

                }
            }
            //load tadm feature installation status
            try
            {
                HxCoreLiquidClass liquid = new HxCoreLiquidClass();
                IsTADMActive = ((IHxCoreTadmFeature)liquid).IsTadmActive(InstrumentName);
                Util.ReleaseComObject(liquid);
            }catch(Exception ex) { }
        }
        /// <summary>
        /// Wait instrument for connection
        /// </summary>
        public static void WaitConnection()
        {
            STARDiscover discover = new STARDiscover();
            
            discover.Start();
            while (!discover.IsConnect)
            {
                Thread.Sleep(100);
            }
            discover.Stop();
            discover.Dispose();
            discover = null;
        }
        void GetCLSID2Editors(IHxPars3 steps)
        {
            object nextCollection = new object();
            if (steps.LookupItem1(HxCommandKeys.nextCollectionLevel, out nextCollection))
            {
                IHxPars3 collection = steps.Item1(HxCommandKeys.nextCollectionLevel) as IHxPars3;
                GetCLSID2Editors(collection);
            }
            else
            {
                for (int i = 1; i <= steps.Count; i++)
                {
                    IHxPars3 step = steps.Item1(i) as IHxPars3;
                    object nc = new object();
                    if (step.LookupItem1(HxCommandKeys.nextCollectionLevel, out nc))
                        GetCLSID2Editors(step);
                    else
                    {
                        object stepBitmap = step.Item1(HxCommandKeys.stepBitmap);
                        string stepCLSID = step.Item1(HxCommandKeys.stepCLSID).ToString();
                        string stepShowName = step.Item1(HxCommandKeys.stepShowName).ToString();
                        string stepBaseName = step.Item1(HxCommandKeys.stepBaseName).ToString();
                        int propVisibility = 0;
                        try
                        {
                            propVisibility = int.Parse(step.Item2(HxCommandKeys.stepProperty, HxCommandKeys.propVisibility).ToString());
                        }
                        catch (Exception e) { }
                        //if (propVisibility == 0)
                        //    return;

                        Type t = Type.GetTypeFromCLSID(new Guid(stepCLSID));
                        IHxCommandStepRun5 editor = Activator.CreateInstance(t) as IHxCommandStepRun5;
                        StepRunData srd = new StepRunData();
                        srd.Command = editor;
                        srd.StepBaseName = stepBaseName;
                        AllSteps.Add(stepBaseName, srd);
                    }
                    ReleaseComObject(step);
                }
            }
        }
        /// <summary>
        /// Start instrument control
        /// </summary>
        public void Start()
        {
            if (logger == null)
            {
                logger = new StreamWriter(new FileStream(STARRegistry.LogFilesPath + @"\" + RunName + "_" + RunId + "_Trace.trc", FileMode.CreateNew));
                isHamiltonLogger = true;
            }
            methodEnded = false;
            MlSTAR.StartMethod();
            //get arm information
            string reply = SendFirmware("C0UA","");
            string[] values = reply.Substring(reply.IndexOf("ua") + 2).Split(' ');
            int v1 = int.Parse(values[0]);
            int v2 = int.Parse(values[1]);
            LeftArm.IsInstalled = (v1 > 0);
            RightArm.IsInstalled = (v2 > 0);
            LeftArm.WrapSize = (v1 / 10.0);
            RightArm.WrapSize = v2 / 10.0;

            reply = SendFirmware("C0RU", "");
            string[] positions = reply.Substring(reply.IndexOf("ru") + 2).Split(' ');
            LeftArm.MinimalPosition = int.Parse(positions[0]) / 10.0;
            LeftArm.MaximalPosition = int.Parse(positions[1]) / 10.0;
            RightArm.MinimalPosition = int.Parse(positions[2]) / 10.0;
            RightArm.MaximalPosition = int.Parse(positions[3]) / 10.0;
            if (IsSimulation) Simulator?.Start(this);
        }
        /// <summary>
        /// End instrument control
        /// </summary>
        public void End()
        {
            if (IsSimulation) Simulator?.End();
            MlSTAR.EndMethod();
            DateTime timeout = DateTime.Now.AddSeconds(20);
            while (!methodEnded)
            {
                if (timeout < DateTime.Now)
                    return;
                Thread.Sleep(50);
            }
        }
        /// <summary>
        /// free all the resource
        /// </summary>
        public void Dispose()
        {
            try
            {
                Channel = null;
                LeftArm = null;
                RightArm = null;
                ReleaseComObject(trace);
                ReleaseComObject(HxSystemDeck);
                ReleaseComObject(HxInstrumentDeck);
                ReleaseComObject(Registry);
                foreach (StepRunData srd in AllSteps.Values)
                {
                    ReleaseComObject(srd.Command);
                }
                AllSteps.Clear();
                ReleaseComObject(StepParamCfg);
                ReleaseComObject(StepRunCfg);
                ReleaseComObject(MlSTAR);
                if (Deck != null)
                    Deck.Dispose();
                trace = null;
                HxSystemDeck = null;
                HxInstrumentDeck = null;
                dbTracking = null;

                Registry = null;
                StepParamCfg = null;
                MlSTAR = null;
                Deck = null;
            }catch(Exception e) { 
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            GC.Collect();
        }
        /// <summary>
        /// Pause instrument control
        /// </summary>
        public void Pause()
        {
            MlSTAR.Pause();
        }
        /// <summary>
        /// Resume instrument control after pausing
        /// </summary>
        public void Resume()
        {
            MlSTAR.Resume();
        }
        /// <summary>
        /// Abort instrument control
        /// </summary>
        public void Abort()
        {
            MlSTAR.Abort();
        }
        /// <summary>
        /// Initialize the instrument control
        /// </summary>
        /// <param name="simulation">simulation mode</param>
        /// <param name="runName">run name</param>
        public void Init(bool simulation = false, string runName= "VenusWrapper")
        {
            Init(0, simulation, runName);
        }
        /// <summary>
        /// Initialize the instrument control
        /// </summary>
        /// <param name="cmdRunHWnd">parent window handler</param>
        /// <param name="simulation">simulation mode</param>
        public void Init(int cmdRunHWnd, bool simulation = false, string runName = "VenusWrapper")
        {
            if (MlSTAR != null)
                Dispose();
            HxSystemDeck = new SystemDeck();
            HxSystemDeck.InitSystemFromInstrument(InstrumentName);
            OnSystemDeckCreated?.Invoke(HxSystemDeck);
            HxSimulationModes mode = simulation ? HxSimulationModes.csmFullSimulation : HxSimulationModes.csmSimulationOff;
            InitImpl(HxSystemDeck.GetInstrumentLayout(InstrumentName, null), cmdRunHWnd, mode, runName);
        }
        /// <summary>
        /// Initialize the instrument control
        /// </summary>
        /// <param name="cmdRunDeckLayoutFile">deck layout file path</param>
        /// <param name="cmdRunHWnd">parent window handler</param>
        /// <param name="simulation">simulation mode</param>
        public void Init(string cmdRunDeckLayoutFile, int cmdRunHWnd=0, bool simulation = false, string runName = "VenusWrapper")
        {
            if (MlSTAR != null)
                Dispose();
            HxSystemDeck = new SystemDeck();
            HxSystemDeck.InitSystemFromFile(cmdRunDeckLayoutFile);
            OnSystemDeckCreated?.Invoke(HxSystemDeck);
            HxSimulationModes mode = simulation ? HxSimulationModes.csmFullSimulation : HxSimulationModes.csmSimulationOff;
            InitImpl(HxSystemDeck.GetInstrumentLayout(InstrumentName, null), cmdRunHWnd, mode, runName);
        }

        object GetVectorDbTracking()
        {
            /*
            AppDomainSetup tracking = new AppDomainSetup();
            tracking.ApplicationBase = Registry.BinPath;
            tracking.ShadowCopyFiles = "false";
            tracking.ShadowCopyDirectories = "false";
            tracking.ApplicationName = "Dynamics";
            Evidence evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
            appDomain = AppDomain.CreateDomain("newDomain", evidence, tracking);
            Assembly ambly = appDomain.Load("Hamilton.HxVectorDB");
            Type t = ambly.GetType("Hamilton.HxVectorDb.HxVectorDbTracking");
            object dbTrack = Activator.CreateInstance(t);
            MethodInfo minfo= t.GetMethod("Init");
            minfo.Invoke(dbTrack, new object[2] { runId, Registry.ConfigPath + "\\HxVectorDb.cfg" });
            
            return dbTrack;
            */

            HxVectorDbTracking obj = new HxVectorDbTracking();
            obj.Init(runId, Registry.ConfigPath + "\\HxVectorDb.cfg");
            //if (obj.Enabled)
            //    obj.StartRun("VenusWrapper");
            return obj;
        }
        /// <summary>
        /// show 3d System view in a window
        /// </summary>
        public void Show3DSystemView()
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                Show3DSystemViewForm().Show();
            }
            else
            {
                Thread t = new Thread(() =>
                {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                    System.Windows.Forms.Application.Run(Show3DSystemViewForm());
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                while (true)
                {
                    var forms = System.Windows.Forms.Application.OpenForms;
                    if (forms == null || forms.Count == 0)
                        Thread.Sleep(100);
                    else
                        break;
                }
            }
        }
        System.Windows.Forms.Form Show3DSystemViewForm()
        {
            var form = new System.Windows.Forms.Form();
            form.Width = 800;
            form.Height = 600;
            form.Text = "3D system view";

            HxInstrument3DView view = null;
            view = new HxInstrument3DView();
            view.Initialize(form.Handle.ToInt32(), HxSystemDeck, InstrumentName);
            view.Mode = HxSys3DViewMode.RunVisualization;
            view.ModifyEnable = false;
            return form;
        }
        /// <summary>
        /// Initialize the 3D System layout viewer
        /// </summary>
        /// <param name="handler">control handler that hold the viewer</param>
        /// <returns>viewer object, type of Hamilton.HxSys3DView.HxInstrument3DView</returns>
        public object Init3DSystemView(int handler)
        {
            /*
            Assembly ambly = appDomain.Load("Hamilton.HxSys3DView");
            Type t = ambly.GetType("Hamilton.HxSys3DView.HxInstrument3DView");
            Type t2 = ambly.GetType("Hamilton.HxSys3DView.HxSys3DViewMode");
            object view = Activator.CreateInstance(t);
            MethodInfo minfo = t.GetMethod("Initialize");
            minfo.Invoke(view, new object[3] { handler, HxSystemDeck, InstrumentName });
            PropertyInfo pinfo = t.GetProperty("Mode");
            FieldInfo finfo = t2.GetField("RunVisualization");
            pinfo.SetValue(view, finfo.GetValue(t2),null);
            return view;
            */
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                HxInstrument3DView view = null;
                view = new HxInstrument3DView();
                view.Initialize(handler, HxSystemDeck, InstrumentName);
                view.Mode = HxSys3DViewMode.RunVisualization;
                view.ModifyEnable = false;
                return view;
            }
            else{
                HxInstrument3DView view = null;
                Thread t = new Thread(() =>
                {
                    view = new HxInstrument3DView();
                    view.Initialize(handler, HxSystemDeck, InstrumentName);
                    view.Mode = HxSys3DViewMode.RunVisualization;
                    view.ModifyEnable = false;
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                return view;
            }
        }
        private void InitImpl(DeckLayout deck, int cmdRunHwnd, HxSimulationModes mode, string runName = "VenusWrapper")
        {
            if (cmdRunHwnd <= 0)
                cmdRunHwnd = Util.GetForegroundWindow();
            if(cmdRunHwnd<=0)
                cmdRunHwnd = 0x100a4;
            RunName = runName;
            OnInstrumentDeckCreated?.Invoke(deck, InstrumentName);
            MlSTAR = new HxGruCommand() as IHxGruCommandRun6;
            Registry = new HxRegistry();
            StepParamCfg = new HxCfgFile();
            StepRunCfg = new HxCfgFile();
            //init ML_STAR
            string cmdRunCfgFil = "";
            Registry.InstCfgFile(InstrumentName, ref cmdRunCfgFil);
            cmdRunCfgFil = Registry.ConfigPath + "\\" + cmdRunCfgFil;
            HxCfgFile configFile = new HxCfgFile();
            configFile.LoadFile(cmdRunCfgFil);
            runId = Guid.NewGuid().ToString().Replace("-", "");
            dbTracking = GetVectorDbTracking();
            trace = new HxTrace();
            _IHxTraceViewEvents2_Event traceEvent = trace as _IHxTraceViewEvents2_Event;
            HxInstrumentDeck = deck;
            Deck = Rack.GetLayout(HxInstrumentDeck as IDeckLayout6);
            IHxTraceEvents2_Event traceStar = MlSTAR as IHxTraceEvents2_Event;
            traceStar.DisplayString += TraceStar_DisplayString;
            traceStar.DisplayError += TraceStar_DisplayError;
            traceStar.FormatString += TraceStar_FormatString;
            _IHxCommandEvents3_Event starEvent = MlSTAR as _IHxCommandEvents3_Event;
            starEvent.OnAborted += StarEvent_OnAborted;
            starEvent.OnPaused += StarEvent_OnPaused;
            starEvent.OnPausing += StarEvent_OnPausing;
            starEvent.OnResumed += StarEvent_OnResumed;
            starEvent.OnMethodStarted += StarEvent_OnMethodStarted;
            methodEnded = false;
            starEvent.OnMethodEnded += StarEvent_OnMethodEnded;
            starEvent.OnControlPanelComplete += StarEvent_OnControlPanelComplete;

            if (dbTracking != null && (dbTracking as HxVectorDbTracking).Enabled)
                (dbTracking as HxVectorDbTracking).StartRun(RunName);
            MlSTAR.InitCommandRun(RunName, runId, cmdRunCfgFil, trace, HxInstrumentDeck, dbTracking,
                    cmdRunHwnd, InstrumentName, mode);
            MlSTAR.SetEventIdentifier(1);
            //Load All steps
            IHxParsPersist stepList = new HxPars() as IHxParsPersist;
            stepList.InitFromCfgFil(configFile, "CommandStepsDefinition");
            GetCLSID2Editors(stepList as IHxPars3);

            object dNames = new object();
            object iNames = new object();
            configFile.GetDataDefInstanceNames(out dNames, out iNames);
            object[] defNames = dNames as object[];
            object[] instNames = iNames as object[];
            for (int i = 0; i < defNames.Length; i++)
            {
                if (defNames[i].Equals("HxPars"))
                {
                    IHxParsPersist var = new HxPars() as IHxParsPersist;
                    var.InitFromCfgFil(configFile, instNames[i] as string);
                    object stepName = "";
                    if ((var as IHxPars2).LookupItem1("StepName", out stepName))
                    {
                        if (AllSteps.ContainsKey(stepName as string))
                        {
                            StepRunData srd = AllSteps[stepName as string];
                            srd.DataId = Guid.NewGuid().ToString().Replace("-", "_");
                            var.SaveInCfgFil(StepParamCfg, srd.DataId);
                        }
                    }
                    ReleaseComObject(var);
                }
            }
            defNames = null;
            instNames = null;
            dNames = null;
            instNames = null;
            ReleaseComObject(configFile);
            ReleaseComObject(stepList);
            foreach (StepRunData srd in AllSteps.Values)
            {
                srd.Command.InitCommandStepRun(StepRunCfg, MlSTAR);
            }
            //initize the component value
            Channel = new Channel(this);
            LeftArm = new LeftArm(this);
            RightArm = new RightArm(this);
            OnDeviceCreated?.Invoke(MlSTAR, InstrumentName);
        }
        void ResetRecovery(IHxPars stepv)
        {
            int NrOfRecovery = stepv.Item1("NbrOfErrors");
            for (int i = 1; i <= NrOfRecovery; i++)
            {
                stepv.Add(0, "Errors", i, "UseDefault");
                stepv.Add(0, "Errors", i, "Infinite");
                int NbrOfRocery2 = stepv.Item3("Errors", i, "NbrOfRecovery");
                for (int j = 1; j <= NbrOfRocery2; j++)//1 for cancel
                {
                    stepv.Add(1,"Errors", i, "Recoveries", j, "RecoveryVisible");
                    stepv.Add(j == 1 ? 1 : 0, "Errors", i, "Recoveries", j, "RecoveryDefault");
                    int rtitle = stepv.Item("Errors", i, "Recoveries", j, "RecoveryTitle");
                }
            }
        }
        private void StarEvent_OnControlPanelComplete(int commandId)
        {
            OnControlPanelComplete?.Invoke();
        }

        private void StarEvent_OnMethodEnded(int commandId, object pErrorInfo)
        {
            methodEnded = true;
            if (isHamiltonLogger && logger != null)
            {
                try
                {
                    logger.Close();
                }
                catch (Exception e) { }
            }
            OnEnded?.Invoke();
        }

        private void StarEvent_OnMethodStarted(int commandId, object pErrorInfo)
        {
            OnStarted?.Invoke();
        }

        private void StarEvent_OnResumed(int commandId)
        {
            OnResumed?.Invoke();
        }

        private void StarEvent_OnPausing(int commandId)
        {
            OnPausing?.Invoke();
        }

        private void StarEvent_OnPaused(int commandId)
        {
            OnPaused?.Invoke();
        }

        private void StarEvent_OnAborted(int commandId)
        {
            if (isHamiltonLogger && logger != null)
            {
                try
                {
                    logger.Close();
                }
                catch (Exception e) { }
            }
            OnAborted?.Invoke();
        }
        #region event handler
        /// <summary>
        /// Insrument control abort event
        /// </summary>
        public event STAREventHandler OnAborted;
        /// <summary>
        /// Instrument control Pausing event
        /// </summary>
        public event STAREventHandler OnPausing;
        /// <summary>
        /// Instrument control paused event
        /// </summary>
        public event STAREventHandler OnPaused;
        /// <summary>
        /// Instrument control resued event
        /// </summary>
        public event STAREventHandler OnResumed;
        /// <summary>
        /// Instrument control panel show completed
        /// </summary>
        public event STAREventHandler OnControlPanelComplete;
        /// <summary>
        /// Instrument control started event
        /// </summary>
        public event STAREventHandler OnStarted;
        /// <summary>
        /// Instrument control Complete event
        /// </summary>
        public event STAREventHandler OnEnded;
        /// <summary>
        /// System deck created event, use can create 3DViewer after this event
        /// </summary>
        public event STARSystemDeckCreatedEventHandler OnSystemDeckCreated;
        /// <summary>
        /// Instrument Deck layout created event
        /// </summary>
        public event STARInstrumentDeckCreatedEventHandler OnInstrumentDeckCreated;
        /// <summary>
        /// Device created event, use can call all the funcation after this event
        /// </summary>
        public event STARDeviceCreatedEventHandler OnDeviceCreated;
        /// <summary>
        /// Device error event handler, for problem reporting only
        /// </summary>
        public event STARDeviceErrorOccuredEventHandler OnDeviceErrorOccured;
        #endregion
        /// <summary>
        /// is instrument initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                int inited = (MlSTAR as IHxCommandEdit5).GetValueWithKey(23);
                return inited == 1;
            }
        }
        bool? _isSimulation;
        /// <summary>
        /// Is instrument controlled in simulation
        /// </summary>
        public bool IsSimulation
        {
            get
            {
                if (MlSTAR != null)
                {
                    if(_isSimulation==null)
                     _isSimulation= (MlSTAR as IHxCommandEdit5).GetValueWithKey(18) == 1;
                    return _isSimulation.Value;
                }
                return false;
            }
        }
        internal void GetRunParameter(HxPars usedVariable, string instanceId)
        {
            (usedVariable as IHxParsPersist2).InitFromCfgFil(
                StepParamCfg, instanceId);
        }
        internal void SaveRunParameter(HxPars usedVariable, string instanceId)
        {
            (usedVariable as IHxParsPersist2).SaveInCfgFil(StepRunCfg, instanceId);
        }
        internal void DeleteRunParameter(string instanceId)
        {
            StepRunCfg.DeleteDataDef("HxPars", instanceId);
        }
        internal void AddSequences(HxPars usedVariable, HxPars objBounds)
        {
            object variables = new object();
            if (usedVariable.LookupItem2("Variables", HxCommandKeys.sequenceNames, out variables))
            {
                object pNames = new object();
                HxInstrumentDeck.SequenceNames(ref pNames);
                
                object[] usedSeqs = (variables as IHxPars3).GetKeys() as object[];
                for (int i = 0; i < usedSeqs.Length; i++)
                {
                    string current = usedSeqs[i].ToString();
                    if (current.IndexOf(".")>=0)
                        current = current.Substring(current.IndexOf(".")+1);
                    object seq = new object();
                    HxInstrumentDeck.Sequence(current, ref seq);
                    IIterateSequence3 iseq = seq as IIterateSequence3;
                    if(iseq!=null)
                        objBounds.Add(seq, HxCommandKeys.sequenceNames, usedSeqs[i]);
                }
                Util.ReleaseComObject(variables);
                usedSeqs = null;
            }

        }
        /// <summary>
        /// Setup error handling for step data
        /// </summary>
        /// <param name="stepv">step data pars</param>
        /// <param name="options">error handling options</param>
        internal void SetupErrorRecovery(IHxPars stepv, ErrorRecoveryOptions options)
        {
            int NrOfRecovery = stepv.Item1("NbrOfErrors");
            for (int i = 1; i <= NrOfRecovery; i++)
            {
                int errorNum = stepv.Item("Errors", i, "ErrorNumber");
                MainErrorEnum mainError = (MainErrorEnum)errorNum;
                if (mainError == MainErrorEnum.NoError)
                    continue;
                if (!options.ContainsKey(mainError)){
                    continue;
                }
                RecoveryAction recovery = options[mainError].Recovery;
                RecoveryAction addRecovery = options[mainError].SecondRecovery;
                if (recovery == RecoveryAction.None)
                    continue;
                int title = titleToRecoveries[recovery];
                stepv.Add(0, "Errors", i, "UseDefault");
                stepv.Add(0, "Errors", i, "Infinite");
                if (addRecovery != RecoveryAction.None)
                {
                    stepv.Add(options[mainError].Repeatition, "Errors", i, "RepeatCount");
                    foreach (RecoveryAction k in titleToRecoveries.Keys)
                    {
                        if (k == addRecovery)
                        {
                            stepv.Add(titleToRecoveries[k], "Errors", i, "AddRecovery");
                        }
                    }
                }
                int NbrOfRocery2 = stepv.Item3("Errors", i, "NbrOfRecovery");
                for (int j = 1; j <= NbrOfRocery2; j++)//1 for cancel
                {
                    int rtitle = stepv.Item("Errors", i, "Recoveries", j, "RecoveryTitle");
                    RecoveryAction rcy=RecoveryAction.Cancel;
                    foreach(RecoveryAction k in titleToRecoveries.Keys)
                    {
                        if (rtitle == titleToRecoveries[k])
                            rcy = k;
                    }
                    bool visible = true;
                    if (options.RecoveryVisibility.ContainsKey(rcy))
                    {
                        visible = options.RecoveryVisibility[rcy];
                    }
                    stepv.Add(visible?1:0, "Errors", i, "Recoveries", j, "RecoveryVisible");
                    if (title == rtitle)
                    {
                        stepv.Add(1, "Errors", i, "Recoveries", j, "RecoveryDefault");
                    }
                    else
                    {
                        stepv.Add(0, "Errors", i, "Recoveries", j, "RecoveryDefault");
                    }
                }
            }
        }
        /// <summary>
        /// Intialize the instrument
        /// </summary>
        /// <param name="alwayInitialize">allways intialize</param>
        /// <param name="options">Error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void Initialize(bool alwayInitialize = false, ErrorRecoveryOptions options = null)
        {
            /*
            IHxPars stepv = (MlSTAR as IHxGruCommandEdit4).GetStepDefaults("MlStarDefaultInitializeRunStep") as IHxPars;
            stepv.Add(alwayInitialize ? 1 : 0, "AlwaysInitialize");
            HxPars result = new HxPars();
            MlSTAR.Initialize(stepv, result);
            string fwResult = result.Item2(HxCommandKeys.resultData, 3);
            ReleaseComObject(stepv);
            ReleaseComObject(result);
            Console.WriteLine(fwResult);
            */
            string stepBaseName = "Initialize";
            StepRunData srd = AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            GetRunParameter(usedVariable, instanceId);
            object variables = new object();
            usedVariable.Add(alwayInitialize ? 1 : 0, "AlwaysInitialize");
            AddSequences(usedVariable, objBounds);

            //设置错误处理
            if (options != null)
                SetupErrorRecovery(usedVariable, options);
            SaveRunParameter(usedVariable, instanceId);
            try
            {
                ClearErrorForTask(masterTask);
                Task iniTask = null;
                if (Simulator!=null && IsSimulation) iniTask=Simulator.Initialize();
                HxPars result = srd.Command.Run(InstrumentName, instanceId, masterTask, objBounds) as HxPars;
                if (iniTask != null) iniTask.Wait();
                ReleaseComObject(result);
                StepRunCfg.DeleteDataDef("HxPars", instanceId);
            }
            catch(Exception e)
            {
                StepRunCfg.DeleteDataDef("HxPars", instanceId);
                ReleaseComObject(usedVariable);
                ReleaseComObject(objBounds);
                ModuleErrors errors = GetErrorTask(masterTask);
                ClearErrorForTask(masterTask);
                throw new STARException(e.Message, e, errors);
            }
            ReleaseComObject(usedVariable);
            ReleaseComObject(objBounds);
        }
        /// <summary>
        /// Send firmware command
        /// </summary>
        /// <param name="cmd">command</param>
        /// <param name="parameter">parameter</param>
        /// <param name="taskId">unique task id for get error from trace</param>
        /// <returns>command response</returns>
        public string SendFirmware(string cmd, string parameter, int taskId = 1)
        {
            string stepBaseName = "FirmwareCommand";
            StepRunData srd = AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            GetRunParameter(usedVariable, instanceId);
            object variables = new object();
            usedVariable.Add(cmd, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.order);
            usedVariable.Add(parameter, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.parameter);
            //instanceId = Guid.NewGuid().ToString().Replace("-", "");
            //SaveRunParameter(usedVariable, instanceId);
            string fwResult;
            try
            {
                Task fwTask = null;
                if (Simulator != null && IsSimulation) fwTask = Simulator.SendFirmware(cmd, parameter);
                ClearErrorForTask(taskId);
                HxPars result = new HxPars();
                MlSTAR.FirmwareCommand(usedVariable, result);
                fwResult = result.Item2(HxCommandKeys.resultData, 4);
                ReleaseComObject(result);
                //StepRunCfg.DeleteDataDef("HxPars", instanceId);
                if (fwTask != null) fwTask.Wait();
            }
            catch (Exception e)
            {
                //StepRunCfg.DeleteDataDef("HxPars", instanceId);
                ReleaseComObject(usedVariable);
                ReleaseComObject(objBounds);
                ModuleErrors errors = GetErrorTask(taskId);
                ClearErrorForTask(taskId);
                throw new STARException(e.Message, e, errors);
            }
            ReleaseComObject(usedVariable);
            ReleaseComObject(objBounds);
            return fwResult;
        }
        /// <summary>
        /// Send firmware command
        /// </summary>
        /// <param name="cmd">command</param>
        /// <param name="parameter">parameter</param>
        /// <param name="taskId">unique task id for get error from trace</param>
        /// <returns>command response</returns>
        internal string SendFirmware2(string cmd, string parameter, int taskId=1)
        {
            string stepBaseName = "FirmwareCommand";
            StepRunData srd = AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            GetRunParameter(usedVariable, instanceId);
            object variables = new object();
            usedVariable.Add(cmd, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.order);
            usedVariable.Add(parameter, HxAtsInstrumentParsKeys.global, HxAtsInstrumentParsKeys.parameter);
            instanceId = Guid.NewGuid().ToString().Replace("-", "");
            SaveRunParameter(usedVariable, instanceId);
            string fwResult;
            try
            {
                ClearErrorForTask(taskId);
                HxPars result = srd.Command.Run(InstrumentName, instanceId, taskId, objBounds) as HxPars;
                fwResult = result.Item2(HxCommandKeys.resultData, 4);
                ReleaseComObject(result);
                StepRunCfg.DeleteDataDef("HxPars", instanceId);
            }
            catch(Exception e)
            {
                StepRunCfg.DeleteDataDef("HxPars", instanceId);
                ReleaseComObject(usedVariable);
                ReleaseComObject(objBounds);
                ModuleErrors errors = GetErrorTask(taskId);
                ClearErrorForTask(taskId);
                throw new STARException(e.Message, e, errors);
            }
            ReleaseComObject(usedVariable);
            ReleaseComObject(objBounds);
            return fwResult;
        }
        /// <summary>
        /// Check the front cover, if it is locked
        /// </summary>
        public bool IsFrontCoverLocked
        {
            get
            {
                string reply = SendFirmware("C0QC", "");
                if (reply.Contains("qc1"))
                    return true;
                return false;
            }
        }
        /// <summary>
        /// Query Carrier Presence on Deck
        /// </summary>
        /// <returns>carrier presence array of boolean</returns>
        /// <exception cref="STARException">STARException if error found</exception>
        public bool[] QueryCarrierPresenceOnDeck()
        {
            string ret = SendFirmware("C0RC", "");
            int index = ret.IndexOf("cd");
            string presence = ret.Substring(index + 2, 14);
            return GetLoadingResult(presence);
        }
        bool[] GetLoadingResult(string param)
        {
            bool[] v = new bool[55];
            for (int i = 0; i < v.Length; i++)
                v[i] = false;
            if (!param.StartsWith("0x"))
                param = "0x" + param;
            try
            {
                ulong p = Convert.ToUInt64(param, 16);
                for (int i = 0; i < v.Length; i++)
                {
                    if ((p & 0x1) > 0)
                        v[i] = true;
                    p = (p >> 1);
                }
            }
            catch (Exception e) { }
            return v;
        }
        /// <summary>
        /// Lock the front cover
        /// </summary>
        /// <param name="locking">true for lock, false for unlock</param>
        /// <param name="options">error handling options</param>
        /// <exception cref="STARException">device error will be throwed with STARException</exception>
        public void LockFrontCover(bool locking, ErrorRecoveryOptions options = null)
        {
            string stepBaseName = "LockFrontCover";
            StepRunData srd = AllSteps[stepBaseName];
            string instanceId = srd.DataId;
            HxPars usedVariable = new HxPars();
            HxPars objBounds = new HxPars();
            GetRunParameter(usedVariable, instanceId);
            object variables = new object();
            usedVariable.Add(locking ? 1 : 0, "LockFrontCoverState");
            //设置错误处理
            if (options != null)
                SetupErrorRecovery(usedVariable, options);
            SaveRunParameter(usedVariable, instanceId);
            try
            {
                HxPars result = srd.Command.Run(InstrumentName, instanceId, masterTask, objBounds) as HxPars;
                ReleaseComObject(result);
                StepRunCfg.DeleteDataDef("HxPars", instanceId);
            }
            catch(Exception e)
            {
                StepRunCfg.DeleteDataDef("HxPars", instanceId);
                ReleaseComObject(usedVariable);
                ReleaseComObject(objBounds);
                ModuleErrors errors = GetErrorTask(masterTask);
                ClearErrorForTask(masterTask);
                throw new STARException(e.Message, e, errors);
            }
            ReleaseComObject(usedVariable);
            ReleaseComObject(objBounds);
        }
       
        private void TraceStar_FormatString(object pHxPars)
        {
            IHxPars2 pars = pHxPars as IHxPars2;
            //object obj = new object();
            //Util.TraceIHxPars(pars);
            object error = "";
            int stepStatus = pars.Item(HxTraceFormatKeys.stepStatus);
            string stepName = pars.Item(HxTraceFormatKeys.stepName);
            object taskId = -1;
            pars.LookupItem1(HxTraceFormatKeys.currentTaskId, out taskId);
            string instrumentName = pars.Item(HxTraceFormatKeys.instrumentName);
            object details = "";
            
            if(!pars.LookupItem1(HxTraceFormatKeys.details, out details))
            {
                object tipStatus = "";
                //解析出现的错误，并保存到对应的task中
                if(pars.LookupItem1(HxTraceFormatKeys.tipStatus, out tipStatus))
                {
                    IHxPars3 tipPars = tipStatus as IHxPars3;
                    object[] okeys = (object[])tipPars.GetKeys();
                    int[] keys = new int[okeys.Length];
                    for (int i = 0; i < okeys.Length; i++)
                        keys[i] = int.Parse(okeys[i]+"");
                    //sort the key
                    for(int i = 0; i < keys.Length - 1; i++)
                    {
                        for(int j = i + 1; j < keys.Length; j++)
                        {
                            if (keys[i] > keys[j])
                            {
                                int a = keys[i];
                                keys[i] = keys[j];
                                keys[j] = a;
                            }
                        }
                    }
                    ModuleErrors errors = null;
                    for(int i = 0; i < keys.Length; i++)
                    {
                        int pos = keys[i];
                        string tipName = tipPars.Item2(pos, HxTraceFormatKeys.tipName);
                        object tipErrDesc = "";
                        object tipDetails = "";
                        if(tipPars.LookupItem2(pos, HxTraceFormatKeys.tipErrorDescription, out tipErrDesc))
                        {
                            details += tipName;
                            ModuleError tpErr = new ModuleError() {Name = tipName, ErrorDetail = tipErrDesc+"" };
                            tpErr.Index = pos;
                            string[] errDetail = (tipErrDesc + "").Split(new string[] { ":","/" }, StringSplitOptions.RemoveEmptyEntries);
                            tpErr.Module = errDetail[1];
                            tpErr.MainError = int.Parse(errDetail[2]);
                            tpErr.SlaveError = int.Parse(errDetail[3]);
                            string recovery = errDetail[4];
                            try
                            {
                                tpErr.Recovery = (RecoveryAction)typeof(RecoveryAction).GetField(recovery).GetValue(typeof(RecoveryAction));
                            }
                            catch (Exception e) { }
                            object labwareId = new object();
                            if (tipPars.LookupItem2(pos, HxTraceFormatKeys.labwareId, out labwareId))
                            {
                                tpErr.LabwareId = labwareId + "";
                                details +=": "+ tpErr.LabwareId;
                            }
                            object positionId = new object();
                            if(tipPars.LookupItem2(pos, HxTraceFormatKeys.positionId, out positionId))
                            {
                                tpErr.PositionId = positionId + "";
                                details += "," + tpErr.PositionId;
                            }
                            tpErr.PositionId = positionId + "";
                            details += " " + tipErrDesc+";";
                            if (errors == null)
                                errors = new ModuleErrors() { StepName = stepName, TaskId = int.Parse(taskId+"") };
                            errors.Add(tpErr);
                        }
                        else if(tipPars.LookupItem2(pos, HxTraceFormatKeys.tipDetails, out tipDetails))
                        {
                            details += tipName+", "+tipDetails;
                        }
                        else
                        {
                            details +=  tipName;
                        }
                    }
                    if (errors != null)
                    {
                        SetErrorForTask(errors.TaskId, errors);
                        OnDeviceErrorOccured?.Invoke(errors, InstrumentName);
                    }
                }
                Util.ReleaseComObject(tipStatus);
            }

            FormatTrace(instrumentName, stepName, (StepStatusEnum)(stepStatus - 1), details);
            
            object obj = new object();
            if(pars.LookupItem1(HxTraceFormatKeys.errorObject, out obj))
            {
                if ((obj as IErrorInfo) != null)
                {
                    IErrorInfo eobj = obj as IErrorInfo;
                    string dess = "";
                    eobj.GetDescription(out dess);
                    throw new Exception(dess);
                }
            }
            ReleaseComObject(pHxPars);
        }

        string[] statusStrings = new string[5] { "start", "complete", "error", "progress", "completed with error" };
        /// <summary>
        /// Logging
        /// </summary>
        /// <param name="source">message source</param>
        /// <param name="action">command action</param>
        /// <param name="status">status</param>
        /// <param name="details">message detail</param>
        public void FormatTrace(string source, string action, StepStatusEnum status, object details)
        {
            try
            {
                if (logger != null)
                {
                    logger.WriteLine("{4}> {0} : {1} - {2}; {3}", source, action, statusStrings[(int)status], details, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    logger.Flush();
                }
                //Console.WriteLine("{4}> {0} : {1} - {2}; {3}", source, action, statusStrings[(int)status], details, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            }
            catch (Exception e) { }
        }

        private void TraceStar_DisplayError(int scode, object pIErrorInfo)
        {
            Console.WriteLine("TraceStar_DisplayError");
        }

        private void TraceStar_DisplayString(string msg, int scode = 0)
        {
            Console.WriteLine(msg);
        }
        static void ReleaseComObject(object obj)
        {
            Util.ReleaseComObject(obj);
        }
    }
}
