var ML_STAR = new STARCommand();
ML_STAR.Init(@"STAR.lay", 0, true);
ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();

var tip = ML_STAR.Deck["Tip1"];
var plate1 = ML_STAR.Deck["Plate1"];
var plate2 = ML_STAR.Deck["Plate2"];

string liquidClass = "StandardVolume_Water_DispenseJet_Empty";
LLDsParameter parameter = new LLDsParameter() { CLLDSensitivity = LLDSensitivity.LabwareDefinition, SubmergeDepth = 2 };
parameter.LiquidClassParameter.TipType = CoreTipType.StandardVolumeTip;
parameter.LiquidClassParameter.DispenseMode = DispenseMode.JetEmpty;
parameter.LiquidClassParameter.LiquidClass = liquidClass;

FixHeightParameter dparameter = new FixHeightParameter() { FixHeight = 15, RetractDistanceForAirTransport = 5 };
dparameter.LiquidClassParameter.TipType = CoreTipType.StandardVolumeTip;
dparameter.LiquidClassParameter.DispenseMode = DispenseMode.JetEmpty;
dparameter.LiquidClassParameter.LiquidClass = liquidClass;

for (var i = 0; i < 12; i++)
{
    ML_STAR.Channel.PickupTip(tip.Columns(i));
    ML_STAR.Channel.Aspirate(plate1.Columns(i), 100, parameter);
    ML_STAR.Channel.Dispense(plate2.Columns(i),100, dparameter);
    ML_STAR.Channel.EjectTip();
}
ML_STAR.End();