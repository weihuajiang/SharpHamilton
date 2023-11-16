using Hamilton.Interop.HxCfgFil;
using Hamilton.Interop.HxLabwr3;
using Hamilton.Interop.HxParams;
using Hamilton.Interop.HxSysDeck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Media3D;

namespace Huarui.STARLine
{
    /// <summary>
    /// Template, Rack or Deck Layout
    /// All these labware are defined as Rack in SDK, you can easily handle the labware and decklayout with its methods
    /// </summary>
    /// <example>
    /// To add a labware into decklayout
    /// <code language="cs">
    /// ML_STAR.Deck.AddLabwareToDeckSite(@"C:\Program Files (x86)\HAMILTON\ML_STAR\TIP_CAR_480BC_HT_A00.tml", "6T-6", "ABC", "default");
    /// ML_STAR.Deck.AddLabware((@"C:\Program Files (x86)\HAMILTON\ML_STAR\TIP_CAR_480BC_HT_A00.tml", "ABC", x, y, z, rotation)
    /// </code>
    /// 
    /// delete a labware by its ID
    /// <code language="cs">
    /// ML_STAR.Deck.RemoveFromDeck(id);
    /// </code>
    /// 
    /// get the labware with its ID, and get the container from labware
    /// <code language="cs">
    /// var cts = ML_STAR.Deck["ht_l_0003"].Containers;
    /// Container[] cnts = new Container[8] { cts["6"], cts["7"], cts["8"], null, cts["10"], cts["11"], null, cts["13"], };
    /// </code>
    /// </example>
    public class Rack : IDisposable
    {
        IDeckLayout6 instrumentDeck;
        Rack()
        {
            Sites = new Dictionary<string, Site>();
        }
        Rack(IDeckLayout6 dl6)
        {
            ID = "default";
            instrumentDeck = dl6;
            IEditDeckLayoutEvents_Event dlEvent = dl6 as IEditDeckLayoutEvents_Event;
            dlEvent.LabwareAdded += LabwareAdded;
            dlEvent.LabwareDeleted += LabwareDeleted;
            dlEvent.LabwareMoved += LabwareMoved;
            dlEvent.LabwareRenamed += LabwareRenamed;
            Sites = new Dictionary<string, Site>();
        }
        /// <summary>
        /// Dispose function
        /// </summary>
        public void Dispose()
        {
            instrumentDeck = null;
            if (Sites != null)
            {
                foreach (var rs in Sites.Values)
                {
                    if (rs != null)
                    {
                        foreach (Rack r in rs.Racks)
                        {
                            r.Dispose();
                        }
                    }
                }
                Sites.Clear();
            }
            Sites = null;
            if (Containers != null)
                Containers.Clear();
            Containers = null;
        }
        /// <summary>
        /// Labware ID
        /// </summary>
        public string ID { get; internal set; } = "default";
        /// <summary>
        /// Rack Type
        /// </summary>
        public RackType Type { get; internal set; }
        /// <summary>
        /// Rack Barcode
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// Barcode Mask
        /// </summary>
        public string BarcodeMask { get; internal set; }
        /// <summary>
        /// barcode must be unique?
        /// </summary>
        public bool IsBarcodeUnique { get; private set; } = false;
        /// <summary>
        /// Containers in rack
        /// </summary>
        public Dictionary<string, Container> Containers { get; internal set; } = new Dictionary<string, Container>();
        /// <summary>
        /// dose container in this labware connected to others
        /// </summary>
        public bool IsContainerConnected { get; private set; } = false;
        /// <summary>
        /// Site and child racks in rack
        /// </summary>
        //public Dictionary<string, List<Rack>> Sites { get; internal set; } = new Dictionary<string, List<Rack>>();
        /// <summary>
        /// sites infomration on template and decklayout
        /// </summary>
        public Dictionary<string, Site> Sites { get; internal set; } = new Dictionary<string, Site>();
        /// <summary>
        /// Rack Position X
        /// </summary>
        public double X { get; internal set; }
        /// <summary>
        /// Rack postion Y (front)
        /// </summary>
        public double Y { get; internal set; }
        /// <summary>
        /// Rack Z base position
        /// </summary>
        public double Z { get;  set; }
        /// <summary>
        /// Rack width in x
        /// </summary>
        public double Width { get; internal set; }
        /// <summary>
        /// Rack depth in y
        /// </summary>
        public double Depth { get; internal set; }
        /// <summary>
        /// Rack height in z
        /// </summary>
        public double Height { get; internal set; }
        /// <summary>
        /// Rack rotation degree
        /// </summary>
        public double Rotation { get; internal set; }
        /// <summary>
        /// File of Rack
        /// </summary>
        public string File { get; internal set; }
        /// <summary>
        /// Image file of rack
        /// </summary>
        public string ImageFile { get; internal set; }

        public ModelData Model { get; set; }
        /// <summary>
        /// background color of rack
        /// </summary>
        public int Color { get; internal set; }
        /// <summary>
        /// Is rack visible in the deck layout
        /// </summary>
        public bool IsVisible { get; set; } = true;
        /// <summary>
        /// Property of labware
        /// </summary>
        public Dictionary<string, string> Properties { get; internal set; } = new Dictionary<string, string>();
        /// <summary>
        /// object for developer to store information
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// template file of deck layout
        /// </summary>
        public string DeckFileName { get; private set; } = null;
        /// <summary>
        /// find rack or child rack with labware id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Rack this[string id]
        {
            get
            {
                if (id.Equals(ID))
                    return this;
                foreach (var rs in Sites.Values)
                {
                    if (rs != null)
                    {
                        foreach (Rack r in rs.Racks)
                        {
                            if (id.Equals(r.ID))
                                return r;
                            if (r.Sites != null && r.Sites.Count > 0)
                            {
                                Rack ret = r[id];
                                if (ret != null)
                                    return ret;
                            }
                        }
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Get track position for rack in the decklayout
        /// </summary>
        /// <param name="rack"></param>
        /// <param name="widthNumOfTrack">width of rack in traks</param>
        /// <returns></returns>
        public int GetTrack(Rack rack, out int widthNumOfTrack)
        {
            widthNumOfTrack = 0;
            if (!this.ID.Equals("default"))
                return -1;
            foreach(var id in Sites.Keys)
            {
                Site site = Sites[id];
                if (site.Racks.Contains(rack))
                {
                    try
                    {
                        widthNumOfTrack = int.Parse(id.Substring(0, id.IndexOf("T-")));
                        return int.Parse(id.Substring(id.IndexOf("-") + 1));
                    }
                    catch { }
                }
            }
            return -1;
        }
        private void LabwareRenamed(string fromId, string labwareId)
        {
            Rack r = this[fromId];
            if (r == null)
                return;
            r.ID = labwareId;
            if(r.Containers!=null && r.Containers.Count > 0)
            {
                foreach (Container c in r.Containers.Values)
                    c.Labware = labwareId;
            }
        }

        private void LabwareMoved(string labwareId)
        {
            Rack r = this[labwareId];
            if (r == null)
                return;
            string plabwId;
            string psiteId;
            if (GetParentInfo(this, labwareId, out plabwId, out psiteId))
            {
                this[plabwId].Sites[psiteId].Racks.Remove(r);
            }
            string nplabwId;
            string npsiteId;
            if (GetHxParenetInof(labwareId, out nplabwId, out npsiteId))
            {
                AddLabware(this, instrumentDeck, nplabwId, npsiteId, labwareId);
            }
        }

        internal void LabwareDeleted(string labwareId)
        {
            Rack r = this[labwareId];
            if (r == null)
                return;
            string siteId = "";
            foreach (var key in Sites.Keys)
            {
                if (Sites[key].Racks.Contains(r))
                    siteId = key;
            }
            if (!string.IsNullOrEmpty(siteId))
            {
                Sites[siteId].Racks.Remove(r);
                return;
            }
            foreach (var s in Sites.Values)
            {
                foreach (var cr in s.Racks)
                {
                    cr.LabwareDeleted(labwareId);
                }
            }
            //TO-DO remove labware on other template
        }

        private void LabwareAdded(string labwareId)
        {
            IEditLabware5 labware = instrumentDeck.Labware[labwareId];
            string plabwId;
            string psiteId;
            if (GetHxParenetInof(labwareId, out plabwId, out psiteId))
            {
                AddLabware(this, instrumentDeck, plabwId, psiteId, labwareId);
            }
            else
            {
                Console.WriteLine("Can not find parent for " + labwareId + " ......................");
            }
            ReleaseComObject(labware);
        }
        static bool GetParentInfo(Rack parent, string labwareId, out string plabwareId, out string psiteId)
        {
            if (parent.Sites != null)
            {
                foreach(string s in parent.Sites.Keys)
                {
                    foreach(Rack r in parent.Sites[s].Racks)
                    {
                        if (r.ID.Equals(labwareId))
                        {
                            psiteId = s;
                            plabwareId = parent.ID;
                            return true;
                        }
                        if (GetParentInfo(r, labwareId, out plabwareId, out psiteId))
                            return true;
                    }
                }
            }
            plabwareId = "";
            psiteId = "";
            return false;
        }
        private bool GetHxParenetInof(string labwareId, out string plabwareId, out string psiteId)
        {
            plabwareId = "";
            psiteId = "";
            HxPars pars = new HxPars();
            instrumentDeck.TemplateLabwareNames(pars);
            for (int i = 1; i <= pars.Count; i++)
            {
                string siteId = pars.Item(i, "Labwr_DkSiteId");
                string templateId = pars.Item(i, "Labwr_TemplateId");
                string labwId = pars.Item(i, "Labwr_Id");
                if (labwareId.Equals(labwId))
                {
                    psiteId = siteId;
                    plabwareId = templateId;
                    ReleaseComObject(pars);
                    return true;
                }
            }
            ReleaseComObject(pars);
            return false;
        }
        /// <summary>
        /// add labware to defined position
        /// This can only be call in Deck property in STARCommand
        /// </summary>
        /// <param name="file">labware file path</param>
        /// <param name="labwareId">labware id</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <param name="rotation">rotation</param>
        public void AddLabware(string file, string labwareId, float x, float y, float z, float rotation)
        {
            if (instrumentDeck == null)
                throw new Exception("you can only operate the labware in decklayout");
            string extension = file.Substring(file.Length - 3);
            IEditLabware5 labware;
            if ("rck".Equals(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                labware = new RectRack() as IEditLabware5;
            }
            else if ("tml".Equals(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                labware = new Template() as IEditLabware5;
            }
            else
            {
                throw new Exception("Invalid file type");
            }
            IHxCfgFile cfg = new HxCfgFile() as IHxCfgFile;
            cfg.LoadFile(file);
            labware.InitFromCfgFil(cfg, labware);
            HxPars pos = new HxPars();
            pos.Add(true, "Labwr_UseGrid");
            pos.Add(labwareId, "Labwr_Id");
            pos.Add(x, "Labwr_XCoord");
            pos.Add(y, "Labwr_YCoord");
            pos.Add(z, "Labwr_ZCoord");
            pos.Add(rotation, "Labwr_Rotation");
            lock (this)
            {
                instrumentDeck.AddLabwareToDeck(labware, pos);
            }
            //wait labware added
            ReleaseComObject(pos);
            ReleaseComObject(labware);
            ReleaseComObject(cfg);
        }
        /// <summary>
        /// add labware to site of template
        /// This can only be call in Deck property in STARCommand
        /// </summary>
        /// <param name="file">labware file</param>
        /// <param name="siteId">parent labware site</param>
        /// <param name="labwareId">labware id</param>
        /// <param name="templateId">parent template id</param>
        /// <param name="bCkSiteFit">check site fit</param>
        public void AddLabwareToDeckSite(string file, string siteId, string labwareId, string templateId="default", int bCkSiteFit = 1)
        {
            if (instrumentDeck == null)
                throw new Exception("you can only operate the labware in decklayout");
            string extension = file.Substring(file.Length - 3);
            IEditLabware5 labware;
            if ("rck".Equals(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                labware = new RectRack() as IEditLabware5;
            }
            else if ("tml".Equals(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                labware = new Template() as IEditLabware5;
            }
            else
            {
                throw new Exception("Invalid file type");
            }
            IHxCfgFile cfg = new HxCfgFile() as IHxCfgFile;
            cfg.LoadFile(file);
            labware.InitFromCfgFil(cfg, labware);
            lock (this)
            {
                instrumentDeck.AddLabwareToDeckSite(labware, siteId, labwareId, templateId, bCkSiteFit);
            }
            //wait labwareAdded
            ReleaseComObject(labware);
            ReleaseComObject(cfg);
        }
        /// <summary>
        /// Remove labware from deck
        /// This can only be call in Deck property in STARCommand
        /// </summary>
        /// <param name="id">labware id</param>
        public void RemoveFromDeck(string id)
        {
            if (instrumentDeck == null)
                throw new Exception("you can only operate the labware in decklayout");

            string plabwId;
            string psiteId;
            if (GetHxParenetInof(id, out plabwId, out psiteId))
            {
            }

            lock (this)
            {
                instrumentDeck.RemoveFromDeck(id);

                Rack r = this[id];
                if (r == null)
                    return;
                if ("default".Equals(plabwId))
                    Sites[psiteId].Racks.Remove(r);
                else
                    this[plabwId].Sites[psiteId].Racks.Remove(r);
            }
        }
        /// <summary>
        /// Rename labware Id
        /// This can only be call in Deck property in STARCommand
        /// </summary>
        /// <param name="newId">new id</param>
        /// <param name="oldId">old id</param>
        public void RenameLabware(string newId, string oldId)
        {
            if (instrumentDeck == null)
                throw new Exception("you can only operate the labware in decklayout");
            instrumentDeck.RenameLabware(newId, oldId);
        }
        //TO-DO
        /// <summary>
        /// Replaces a container on a rectangular pre-loaded rack
        /// </summary>
        /// <param name="rackId">The name of the rack (labware id) where to replace the container (string, e.g. "SMP_CAR_24_0001".)</param>
        /// <param name="positionId">The name of the position (position id) on the rack where to replace the container (string, e.g. "1").</param>
        /// <param name="configFile">The configuration file name for the container to be replaced (string, e.g. "ml_star\\cup_15x75.ctr"). </param>
        /// <param name="xOffset">The offsets (x, y, z) of the container relative to the container position posId (float).</param>
        /// <param name="yOffset">The offsets (x, y, z) of the container relative to the container position posId (float).</param>
        /// <param name="zOffset">The offsets (x, y, z) of the container relative to the container position posId (float).</param>
        public void AddContainerToRack(
            string rackId,
            string positionId,
            string configFile,
            double xOffset,
            double yOffset,
            double zOffset)
        {
            if (instrumentDeck == null)
                throw new Exception("you can only operate the labware in decklayout");
            IRectRack5 rr5= instrumentDeck.Labware[rackId];
            if (rr5 == null)
                throw new Exception("There is no rack with ID " + rackId);
            HxPars pars = new HxPars();
            
            pars.Add(positionId, "Labwr_PosId");
            pars.Add(configFile, "Labwr_File");
            pars.Add(yOffset, "Labwr_YOffset");
            pars.Add(xOffset, "Labwr_XOffset");
            pars.Add(zOffset, "Labwr_ZOffset");
            rr5.AddContainerToRack(pars);
            Util.ReleaseComObject(pars);

            HxPars pos = new HxPars();
            rr5.GetRackPositionData(pos);
            object p = new object();
            if(pos.LookupItem2("Labwr_PosData", positionId, out p))
            {
                IHxPars3 cnt = p as IHxPars3;
                double x = cnt.Item("Labwr_XCoord");
                double y = cnt.Item("Labwr_YCoord");
                double z = cnt.Item("Labwr_ZCoord");
                double width = cnt.Item("Labwr_XDim");
                double height = cnt.Item("Labwr_YDim");
                double diameter = cnt.Item("Labwr_Diam");
                double liquidSeeking = 0;
                try { liquidSeeking = cnt.Item("Labwr_ZLiquidSeek"); } catch (Exception e) { }
                double clearance = 0;
                try { clearance = cnt.Item("Labwr_ZClearance"); } catch (Exception e) { }
                int shape = cnt.Item("Labwr_Shape");
                string file = cnt.Item("Labwr_File");
                //Util.TraceIHxPars(cnt);
                Container c=new Container()
                {
                    Position = positionId,
                    Labware = rackId,
                    X = x,
                    Y = y,
                    Z = z,
                    XDim = width,
                    YDim = height,
                    Diameter = diameter,
                    Shape = (Shape)shape,
                    File = file,
                    LiquidSeekingStartPosition = liquidSeeking,
                    Clearance = clearance
                };
                LoadSegments(c, file);
                this[rackId].Containers[positionId] = c;
                ReleaseComObject(cnt);
            }
            else
            {
            }
            Util.ReleaseComObject(pos);
            Util.ReleaseComObject(rr5);
        }
        /// <summary>
        /// Updates the list of loaded labware and updates the view
        /// </summary>
        /// <param name="racks">array of labware ids for update</param>
        /// <param name="states">load state will be applied to the associated labware </param>
        /// <param name="description">description will be displayed in the first section on the status bar in the deck view ; an empty string will do nothing.</param>
        public void UpdateLoadedLabware(Rack[] racks, LabwareState[] states, string description)
        {
            if (instrumentDeck == null)
                throw new Exception("Deck visiblization handling can be used only done in deck layout");
            HxPars pars = new HxPars();
            for (int i = 0; i < racks.Length; i++)
            {
                Rack r = racks[i];
                pars.Add(states[i], r.ID);
            }
            instrumentDeck.UpdateLoadedLabware(description, 0, pars);
            ReleaseComObject(pars);
        }
        /// <summary>
        /// Update labware status in deck layout visibilization
        /// </summary>
        /// <param name="racks">racks</param>
        /// <param name="actions">labware actions</param>
        /// <param name="description">description will be displayed in the first section on the status bar in the deck view ; an empty string will do nothing.</param>
        public void UpdateUsedLabware(Rack[] racks, LabwareAction[] actions, string description)
        {
            if (instrumentDeck == null)
                throw new Exception("Deck visiblization handling can be used only done in deck layout");
            HxPars pars = new HxPars();
            for(int i=0;i<racks.Length;i++)
            {
                Rack r = racks[i];
                pars.Add(actions[i], r.ID);
            }
            instrumentDeck.UpdateUsedLabware(description, 0, pars);
            ReleaseComObject(pars);
        }
        /// <summary>
        /// Updates the list of used positions and updates the view
        /// </summary>
        /// <param name="cnts">containers for update</param>
        /// <param name="action">the same action will be applied to all positions in the given list</param>
        /// <param name="description">description will be displayed in the first section on the status bar in the deck view ; an empty string will do nothing.</param>
        public void UpdateUsedPositions(Container[] cnts, LabwareAction action, string description)
        {
            if (instrumentDeck == null)
                throw new Exception("Deck visiblization handling can be used only done in deck layout");
            HxPars pars = new HxPars();
            foreach(Container c in cnts)
            {
                if (c == null)
                    continue;
                pars.Add((int)action, c.Labware, c.Position);
            }
            instrumentDeck.UpdateUsedPositions(description, 0, pars);
            ReleaseComObject(pars);
        }
        static void LoadSegments(Container c, string file)
        {
            IContainerData2 cntData = new Hamilton.Interop.HxLabwr3.Container();
            HxCfgFile cfg = new HxCfgFile();
            cfg.LoadFile(file);
            cntData.InitFromCfgFil(cfg);
            HxPars segment = new HxPars();
            cntData.GetContainerSegments(segment);

            c.LiquidSeekValid = (cntData.LiquidSeekValid == 1);
            if (c.LiquidSeekValid)
                c.CLLD = (LLDSensitivity)cntData.cLLD;
            c.Depth = cntData.Depth;
            c.MaxPipettingDepth = cntData.MaxDepth;

            for (int i = 1; i <= segment.Count; i++)
            {
                Segment seg = new Segment();
                seg.Shape = (Shape)segment.Item(i, "Labwr_Seq_Type");
                seg.MaxHeight = segment.Item(i, "Labwr_Seq_Max_Ht");
                seg.MinHeight = segment.Item(i, "Labwr_Seq_Min_Ht");
                seg.Dimension1 = segment.Item(i, "Labwr_Seq_Dim1");
                seg.Dimension2 = segment.Item(i, "Labwr_Seq_Dim2");
                seg.Diameter = segment.Item(i, "Labwr_Seq_Diameter");
                c.Segments.Add(seg);
            }
            Util.ReleaseComObject(segment);
            Util.ReleaseComObject(cntData);
            Util.ReleaseComObject(cfg);
        }
        static void AddLabware(Rack InstrumentLayout, IDeckLayout6 dl6, string templateId, string siteId, string labwareId)
        {
            lock (InstrumentLayout)
            {
                Rack r = new Rack();
                r.ID = labwareId;
                object labware = dl6.Labware[labwareId];
                if(labware is ILabware7 labw7)
                {
                    string model="";
                    double x=0, y=0, z=0;
                    labw7.Get3DModel(ref model, ref x, ref y, ref z);
                    r.Model = new ModelData() { File = model, Offset = new Point3D(x, y, z) };
                    r.ImageFile = labw7.BitmapFile;
                }
                ITemplate tmp = labware as ITemplate;
                if (tmp != null)
                {
                    HxPars sites = new HxPars();
                    tmp.GetSites(sites);
                    for (int i = 1; i <= sites.Count; i++)
                    {
                        Site s = new Site();
                        HxPars v = sites.Item1(i);
                        s.X = v.Item1("Labwr_XCoord");
                        s.Y = v.Item1("Labwr_YCoord");
                        s.Z = v.Item1("Labwr_ZCoord");
                        s.Width = v.Item1("Labwr_DkSiteDimX");
                        s.Depth = v.Item1("Labwr_DkSiteDimY");
                        s.Id = v.Item1("Labwr_DkSiteId");
                        s.Visible = v.Item1("Labwr_Visible") == 1;
                        s.ShowId = v.Item1("Labwr_ShowId") == 1;
                        r.Sites.Add(s.Id, s);
                        ReleaseComObject(v);
                    }
                    ReleaseComObject(sites);
                    Rack prack = InstrumentLayout[templateId];
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        Site site = prack.Sites[siteId];
                        r.X = site.X;
                        r.Y = site.Y;
                        r.Z = site.Z;
                        r.Width = site.Width;
                        r.Depth = site.Depth;
                    }
                    else
                    {
                        var obj = labware as ILabware7;
                        HxPars position = new HxPars();
                        obj.GetDeckPosition("", position);
                        r.X = position.Item1("Labwr_XCoord");
                        r.Y = position.Item1("Labwr_YCoord");
                        r.Z = position.Item1("Labwr_ZCoord");
                        var eobj = labware as IEditLabware6;
                        double w = 0, h = 0;
                        eobj.GetExtent(ref w, ref h, "");
                        r.Width = w;
                        r.Depth = h;
                        r.Height = 0;
                    }
                    //ITemplateDeckData.GetLabwareData 可以获取template上的rack
                }
                
                IRectRack5 rr5 = labware as IRectRack5;
                if (rr5 != null)
                {
                    r.Type = RackType.Rack;
                    r.IsContainerConnected = (rr5.ConnectedCtr == 1);
                    //load containers
                    HxPars pos = new HxPars();
                    Dictionary<string, Container> file2Cnt = new Dictionary<string, Container>();
                    rr5.GetRackPositionData(pos);
                    foreach (IHxPar p in (pos.Item("Labwr_PosData") as IHxPars))
                    {
                        string poistion = p.Key;
                        IHxPars3 cnt = p.Value as IHxPars3;
                        double offsetx = cnt.Item("Labwr_XOffset");
                        double offsety = cnt.Item("Labwr_YOffset");
                        double x = cnt.Item("Labwr_XCoord");
                        double y = cnt.Item("Labwr_YCoord");
                        double z = cnt.Item("Labwr_ZCoord");
                        double width = cnt.Item("Labwr_XDim");
                        double height = cnt.Item("Labwr_YDim");
                        double diameter = cnt.Item("Labwr_Diam");
                        double liquidSeeking = 0;
                        try { liquidSeeking = cnt.Item("Labwr_ZLiquidSeek"); } catch (Exception e) { }
                        double clearance = 0;
                        try { clearance = cnt.Item("Labwr_ZClearance"); } catch (Exception e) { }
                        int shape = cnt.Item("Labwr_Shape");
                        string file = cnt.Item("Labwr_File");
                        string model = "";
                        Point3D offset = new Point3D();
                        try
                        {
                            model = cnt.Item("Labwr_3DModelFile");
                            offset = new Point3D(cnt.Item("Labwr_3DxOffset"), cnt.Item("Labwr_3DyOffset"), cnt.Item("Labwr_3DzOffset"));
                        }
                        catch { }
                        //load container segment
                        Container c = new Container()
                        {
                            Position = poistion,
                            Labware = r.ID,
                            X = x,
                            Y = y,
                            Z = z,
                            XDim = width,
                            YDim = height,
                            Diameter = diameter,
                            Shape = (Shape)shape,
                            File = file,
                            LiquidSeekingStartPosition = liquidSeeking,
                            Clearance = clearance,
                            Model=new ModelData() { File=model, Offset=offset}
                        };
                        if (!file2Cnt.ContainsKey(file))
                        {
                            LoadSegments(c, file);
                            file2Cnt.Add(file, c);
                        }
                        else
                        {
                            foreach (Segment s in file2Cnt[file].Segments)
                            {
                                c.Segments.Add(s);
                            }
                            c.LiquidSeekValid = file2Cnt[file].LiquidSeekValid;
                            if (c.LiquidSeekValid)
                                c.CLLD = file2Cnt[file].CLLD;
                            c.Depth = file2Cnt[file].Depth;
                            c.MaxPipettingDepth = file2Cnt[file].MaxPipettingDepth;
                        }
                        r.Containers.Add(poistion, c);
                        ReleaseComObject(cnt);
                    }
                    ReleaseComObject(pos);
                    //load rack data
                    HxPars rect = new HxPars();
                    rr5.GetRackData(rect);
                    double rx = rect.Item("Labwr_Bndry1X");
                    double ry = rect.Item("Labwr_Bndry1Y");
                    double rz = rect.Item("Labwr_ZBase");
                    double rwith = rect.Item("Labwr_XDim");
                    double rdepth = rect.Item("Labwr_YDim");
                    double rheight = rect.Item("Labwr_ZDim");
                    double rx2 = rect.Item("Labwr_Bndry2X")-rx;
                    double ry2 = rect.Item("Labwr_Bndry2Y")-ry;
                    double rrotation = 0;
                    rrotation = -Math.Atan2(ry2, rx2) * 180 / Math.PI;
                    //Console.WriteLine(r.ID + ": " + rx2 + ", " + ry2+" = "+rrotation);
                    r.X = rx;
                    r.Y = ry;
                    r.Z = rz;
                    r.Width = rwith;
                    r.Depth = rdepth;
                    r.Height = rheight;
                    r.Rotation = rrotation;
                    r.ImageFile = rect.Item("Labwr_BitmapFile");
                    r.Color = rect.Item("Labwr_Color");
                    //load 3D data
                    r.Model = new ModelData();
                    r.Model.File = rect.Item("Labwr_3DModelFile");
                    r.Model.Offset = new Point3D(rect.Item("Labwr_3DxOffset"), rect.Item("Labwr_3DyOffset"), rect.Item("Labwr_3DzOffset"));
                    ReleaseComObject(rect);
                }
                else
                {
                    r.Type = RackType.Template;
                }
                //get labware properties
                {
                    HxPars prop = new HxPars();
                    HxPars bMsk = new HxPars();
                    bMsk.Add("", "Labwr_Barcode");
                    if (labware is IEditObject3)
                    {
                        ((IEditObject3)labware).GetDefaultLabwrProperties(prop);
                        r.File = ((IEditObject3)labware).FileName;
                        if (r.Type == RackType.Template)
                        {
                            //Load template backgroud color
                            HxCfgFile cfg = new HxCfgFile();
                            cfg.LoadFile(r.File);
                            int color = cfg.GetDataDefValueAsLong("TEMPLATE", "default", "BackgrndClr");
                            r.Color = color;
                            string image = "";
                            if (cfg.LookupDataDefValueAsString("TEMPLATE", "default", "Image", out image) == 1)
                            {
                                r.ImageFile = image;
                                if (!string.IsNullOrEmpty(image))
                                {
                                    FileInfo f = new FileInfo(r.File);
                                    r.ImageFile = Path.GetFullPath(f.Directory.FullName + "\\" + image);
                                }
                            }
                            r.Height = cfg.GetDataDefValueAsDouble("TEMPLATE", "default", "Dim.Dz");
                            ReleaseComObject(cfg);
                        }
                    }
                    if (labware is IEditLabware3)
                    {
                        ((IEditLabware3)labware).GetLabwrProperties(prop);
                        ((IEditLabware3)labware).GetBarcodeProperties(bMsk);
                    }
                    else if (labware is IEditLabware4)
                    {
                        ((IEditLabware4)labware).GetLabwrProperties(prop);
                        ((IEditLabware4)labware).GetBarcodeProperties(bMsk);
                    }

                    object value = new object();
                    object[] keys = (object[])prop.GetKeys();
                    foreach (object key in keys)
                    {
                        r.Properties.Add(key + "", prop.Item(key));
                    }
                    keys = (object[])bMsk.GetKeys();
                    r.BarcodeMask = bMsk.Item("Labwr_Barcode");
                    r.IsBarcodeUnique = (bMsk.Item("Labwr_BC_Unique") == 1);
                    ReleaseComObject(prop);
                    ReleaseComObject(bMsk);
                }
                Rack parent = InstrumentLayout[templateId];
                if (!parent.Sites.ContainsKey(siteId))
                {
                    //parent.Sites.Add(siteId, new List<Rack>());
                    Site site = new Site();
                    site.ShowId = false;
                    site.Id = siteId;
                    site.Visible = false;
                    parent.Sites.Add(siteId, site);
                }
                 parent.Sites[siteId].Racks.Add(r);
                ReleaseComObject(labware);
                
            }
        }
        public static Rack GetLayout(string decklayoutFile, string instrumentName)
        {

            var systemDeck = new SystemDeck();
            systemDeck.InitSystemFromFile(decklayoutFile);
            var layout = systemDeck.GetInstrumentLayout(instrumentName, null);
            return GetLayout(layout);
        }
        public static Rack GetLayout(IDeckLayout6 dl6)
        {
            Rack InstrumentLayout = new Rack(dl6);
            InstrumentLayout.DeckFileName = dl6.DeckFileName;

            IDeckData td = dl6 as IDeckData;
            HxPars obj = new HxPars();
            td.GetTemplateData("default", obj);

            InstrumentLayout.X = obj.Item1("Labwr_XCoord");
            InstrumentLayout.Y = obj.Item1("Labwr_YCoord");
            InstrumentLayout.Z = obj.Item1("Labwr_ZCoord");
            InstrumentLayout.Width = obj.Item1("Labwr_XDim");
            InstrumentLayout.Depth = obj.Item1("Labwr_YDim");
            InstrumentLayout.Height = obj.Item1("Labwr_ZDim");
            int color = obj.Item1("Labwr_Color");
            string bitmap = obj.Item1("Labwr_BitmapFile");

            HxPars pars2 = obj.Item1("Labwr_SiteData");
            object[] keys = pars2.GetKeys();
            for (int i = 0; i < keys.Length; i++)
            {
                HxPars v = pars2.Item1(keys[i]);
                Site s = new Site();
                s.X = v.Item1("Labwr_XCoord");
                s.Y = v.Item1("Labwr_YCoord");
                s.Z = v.Item1("Labwr_ZCoord");
                s.Width = v.Item1("Labwr_DkSiteDimX");
                s.Depth = v.Item1("Labwr_DkSiteDimY");
                s.Id = v.Item1("Labwr_DkSiteId");
                s.Visible = v.Item1("Labwr_Visible")==1;
                s.ShowId = v.Item1("Labwr_ShowId")==1;

                InstrumentLayout.Sites.Add(s.Id, s);
                ReleaseComObject(v);
            }
            ReleaseComObject(pars2);
            ReleaseComObject(obj);

            HxPars pars = new HxPars();
            dl6.TemplateLabwareNames(pars);
            for (int i = 1; i <= pars.Count; i++)
            {
                string siteId = pars.Item(i, "Labwr_DkSiteId");
                string templateId = pars.Item(i, "Labwr_TemplateId");
                string labwareId = pars.Item(i, "Labwr_Id");
                AddLabware(InstrumentLayout, dl6, templateId, siteId, labwareId);
            }
            ReleaseComObject(pars);
            return InstrumentLayout;
        }
        static void ReleaseComObject(object obj)
        {
            Util.ReleaseComObject(obj);
        }
    }
}
