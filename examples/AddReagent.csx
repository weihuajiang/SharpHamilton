var ML_STAR = new STARCommand();
ML_STAR.Init(@"STAR.lay", 0, true);
ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();

var tip = ML_STAR.Deck["Tip1"];
var plate1 = ML_STAR.Deck["Plate1"];
var reagent = ML_STAR.Deck["Reagent1"];

string liquidClass = "StandardVolume_Water_AliquotDispenseJet_Part";
LLDsParameter parameter = new LLDsParameter() { cLLDSensitivity = LLDSensitivity.LabwareDefinition, SubmergeDepth = 2 };
parameter.LiquidClassParameter.TipType = CoreTipType.StandardVolumeTip;
parameter.LiquidClassParameter.DispenseMode = DispenseMode.JetPart;
parameter.LiquidClassParameter.LiquidClass = liquidClass;

FixHeightParameter dparameter = new FixHeightParameter() { FixHeight = 15, RetractDistanceForAirTransport = 5 };
dparameter.LiquidClassParameter.TipType = CoreTipType.StandardVolumeTip;
dparameter.LiquidClassParameter.DispenseMode = DispenseMode.JetPart;
dparameter.LiquidClassParameter.LiquidClass = liquidClass;

ML_STAR.Channel.PickupTip(tip.Columns(0));
ML_STAR.Channel.Aspirate(reagent.All(), 300, parameter);
for (var i = 0; i < 12; i++)
{
    ML_STAR.Channel.Dispense(plate1.Columns(i), 25, dparameter);
}
ML_STAR.Channel.EjectTip();
ML_STAR.End();