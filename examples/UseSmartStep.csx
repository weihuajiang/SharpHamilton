using Huarui.STARLine.SmartStep;

var ML_STAR = new STARCommand();
ML_STAR.Init(@"STAR.lay", 0, true);
ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();

var tip = ML_STAR.Deck.GetSequence("Tip1");
var rgt = ML_STAR.Deck.GetSequence("Reagent1");
var plate1 = ML_STAR.Deck.GetSequence("Plate1");
var plate2 = ML_STAR.Deck.GetSequence("Plate2");
plate1.End = 37;

string liquidClass = "StandardVolume_Water_DispenseJet_Part";
LLDsParameter parameter = new LLDsParameter()
{
    cLLDSensitivity = LLDSensitivity.LabwareDefinition,
    SubmergeDepth = 2,
    LiquidClassParameter = new LiquidClassParameter()
    {
        TipType = CoreTipType.StandardVolumeTipFiltered,
        DispenseMode = DispenseMode.JetPart,
        LiquidClass = liquidClass
    }
};
FixHeightParameter dparameter = new FixHeightParameter()
{
    FixHeight = 15,
    RetractDistanceForAirTransport = 5,
    LiquidClassParameter = new LiquidClassParameter()
    {
        TipType = CoreTipType.StandardVolumeTipFiltered,
        DispenseMode = DispenseMode.JetPart,
        LiquidClass = liquidClass
    }
};
ML_STAR.Aliquot(rgt, plate1, 10, parameter, dparameter, tip, "1");
plate1.Current = 1;
ML_STAR.Simple(plate1, plate2, 100, parameter, dparameter, tip);
ML_STAR.End();