var ML_STAR=new STARCommand();
var labware = STARRegistry.LabwarePath;
ML_STAR.Init(true);
ML_STAR.Deck.AddLabwareToDeckSite(labware + @"\ML_STAR\TIP_CAR_480BC_A00.tml", "6T-1", "TIP");
ML_STAR.Deck.AddLabwareToDeckSite(labware + @"\ML_STAR\HT_L.rck", "1", "TIP1", "TIP");
ML_STAR.Deck.AddLabwareToDeckSite(labware + @"\ML_STAR\HT_L.rck", "2", "TIP2", "TIP");
//ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();
var tip = ML_STAR.Deck["TIP1"];
var tip2 = ML_STAR.Deck["TIP2"];
for(int i = 0; i < 12; i++)
{
    ML_STAR.Channel.PickupTip(tip.Columns(i));
    ML_STAR.Channel.EjectTip(tip2.Columns(i));
}
ML_STAR.End();