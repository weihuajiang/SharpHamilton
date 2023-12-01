#load "SelectLayoutFile.csx"
var ML_STAR = new STARCommand();
ML_STAR.Init(GetSystemLayoutFile(), 0, true);
ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();
//write your code here
ML_STAR.End();