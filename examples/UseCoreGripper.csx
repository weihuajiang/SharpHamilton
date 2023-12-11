var ML_STAR=new STARCommand();
ML_STAR.Init(@"STAR.lay", 0, true);
ML_STAR.Show3DSystemView();//show 3D deck layout
//ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();
ML_STAR.CoreGripper.FrontChannelUsed=2;
ML_STAR.CoreGripper.Get(ML_STAR.Deck["Plate1"], 3, 80, 85);
ML_STAR.CoreGripper.Place(ML_STAR.Deck["Plate2"], true);
ML_STAR.End();