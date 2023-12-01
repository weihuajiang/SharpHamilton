#r "System.Windows.Forms"
#r "System.Drawing"
#r "System.IO"
using System.Drawing;
using System.Windows.Forms;

var form = new Form() { Text = "Hello World", Width = 500, Height = 170, MinimizeBox = false, MaximizeBox = false };
var label = new Label() { Text = "Please input deck layout file:", Location = new Point(10, 10), Size = new Size(300, 20) };
var file = new TextBox() { Location = new Point(10, 40), Size = new Size(340, 30), Text = @"C:\Program Files (x86)\HAMILTON\Methods\Test\SystemEditor3d.lay" };
var browse = new Button() { Location = new Point(360, 37), Size = new Size(100, 30), Text = "Browse" };
var button = new Button() { Location = new Point(200, 80), Size = new Size(100, 30), Text = "OK" };
button.Click += (s, e) =>
  {
      if(string.IsNullOrEmpty(file.Text)) return;
      if(!File.Exists(file.Text)) return;
      form.Close();
  };
browse.Click += (s, e) =>
  {
      var dlg = new OpenFileDialog
      {
          Filter = "*system layout file *.lay|*.lay",
          InitialDirectory = @"C:\Program Files (x86)\HAMILTON\Methods"
      };
      if (dlg.ShowDialog(form) == DialogResult.OK)
          file.Text = dlg.FileName;
  };
form.Controls.Add(label);
form.Controls.Add(file);
form.Controls.Add(browse);
form.Controls.Add(button);

Thread t = new Thread(() =>
{
    form.ShowDialog();
});
t.SetApartmentState(ApartmentState.STA);
t.Start();
t.Join();

var ML_STAR = new STARCommand();
ML_STAR.Init(file.Text, 0, true);
ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();
//write your code here
ML_STAR.End();