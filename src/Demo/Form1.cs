using Huarui.STARLine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();


        STARCommand ML_STAR;
        public Form1()
        {
            AllocConsole();
            InitializeComponent();
        }

        private void OnTest(object sender, EventArgs e)
        {
            bool simulation = checkBox1.Checked;
            button1.Enabled = false;
            button2.Enabled = false;
            ML_STAR = new STARCommand();
            ML_STAR.Log = Console.Out;
            var labPath = STARRegistry.LabwarePath;
            ML_STAR.Init(this.Handle.ToInt32(), simulation);
            //ML_STAR.Init3DSystemView(panel1.Handle.ToInt32());
            ML_STAR.Show3DSystemView();
            Task.Run(() =>
            {
                ML_STAR.Start();
                //deck handling
                ML_STAR.Deck.AddLabwareToDeckSite(Path.Combine(labPath, @"ML_STAR\TIP_CAR_480BC_HT_A00.tml"), "6T-1", "Tip");
                ML_STAR.Deck.AddLabwareToDeckSite(Path.Combine(labPath, @"ML_STAR\SMP_CAR_32_12x75_A00.rck"), "1T-7", "Smp");
                var tip = ML_STAR.Deck["ht_l_0001"].Containers;
                var smp = ML_STAR.Deck["Smp"].Containers;
                //get container from rack, used for sequence
                var tips = new Container[] { tip["1"], tip["2"], tip["3"], tip["4"], tip["5"], tip["6"], tip["7"], tip["8"],
                    tip["9"], tip["10"]};
                var smples = new Container[] { smp["1"], smp["2"], smp["3"], smp["4"], smp["5"], smp["6"], smp["7"], smp["8"] };

                ML_STAR.Initialize();

                ML_STAR.Channel.PickupTip(tips);

                //aspiration
                var aspParam = new LLDsParameter() { cLLDSensitivity = LLDSensitivity.LabwareDefinition, SubmergeDepth = 2 };
                aspParam.LiquidClassParameter.TipType = CoreTipType.HighVolumeTip;
                aspParam.LiquidClassParameter.DispenseMode = DispenseMode.JetEmpty;
                aspParam.LiquidClassParameter.LiquidClass = "HighVolume_Water_DispenseJet_Empty";
                //venus-styled error handling
                ErrorRecoveryOptions options = new ErrorRecoveryOptions();
                options.Add(MainErrorEnum.InsufficientLiquidError, new ErrorRecoveryOption() { Recovery = RecoveryAction.Bottom });
                ML_STAR.Channel.Aspirate(smples, 150, aspParam, AspirateMode.Aspiration, options);
                //dispense
                var dispParam = new FixHeightParameter() { FixHeight = 30 };
                dispParam.LiquidClassParameter.TipType = CoreTipType.HighVolumeTip;
                dispParam.LiquidClassParameter.DispenseMode = DispenseMode.JetEmpty;
                dispParam.LiquidClassParameter.LiquidClass = "HighVolume_Water_DispenseJet_Empty";
                ML_STAR.Channel.Dispense(smples, 150, dispParam);

                ML_STAR.Channel.EjectTip();
                ML_STAR.End();
                ML_STAR.Dispose();
                ML_STAR = null;
                button1.Enabled = true;
                button2.Enabled = true;
            });
        }

        private void OnTest2(object sender, EventArgs e)
        {
            bool simulation = checkBox1.Checked;
            button1.Enabled = false;
            button2.Enabled = false;
            ML_STAR = new STARCommand();
            ML_STAR.Log = Console.Out;
            var labPath = STARRegistry.LabwarePath;
            ML_STAR.Init(this.Handle.ToInt32(), simulation);
            ML_STAR.Init3DSystemView(panel1.Handle.ToInt32());
            Task.Run(() =>
            {
                try
                {
                    ML_STAR.Start();
                    ML_STAR.Initialize();
                    //deck handling
                    ML_STAR.Deck.AddLabwareToDeckSite(Path.Combine(labPath, @"ML_STAR\TIP_CAR_480BC_HT_A00.tml"), "6T-1", "Tip");
                    ML_STAR.Deck.AddLabwareToDeckSite(Path.Combine(labPath, @"ML_STAR\SMP_CAR_32_12x75_A00.rck"), "1T-7", "Smp");
                    ML_STAR.Deck.AddLabwareToDeckSite(Path.Combine(labPath, @"ML_STAR\SMP_CAR_32_12x75_A00.rck"), "1T-8", "Pool");
                    var tips = ML_STAR.Deck["ht_l_0001"].Containers.Values.ToList();
                    var samples = ML_STAR.Deck["Smp"].Containers.Values.ToList();
                    tips.Sort((a, b) =>
                    {
                        if (b.X != a.X)
                            return a.X > b.X ? 1 : -1;
                        if (a.Y > b.Y)
                            return a.Y > b.Y ? -1 : 1;
                        return 0;

                    });
                    samples.Sort((a, b) =>
                    {
                        if (a.Y > b.Y)
                            return a.Y > b.Y ? -1 : 1;
                        return 0;
                    });
                    int numOfChannel = ML_STAR.Channel.Count;
                    Container[] usedTips = new Container[numOfChannel];
                    Container[] sourceSamples = new Container[numOfChannel];
                    Container[] poolSamples = new Container[numOfChannel];
                    for (int i = 1; i <= numOfChannel; i++)
                        poolSamples[i - 1] = ML_STAR.Deck["Pool"].Containers[i + ""];
                    int pooled = 0;
                    while (pooled < samples.Count)
                    {
                        for (int i = 0; i < numOfChannel; i++)
                        {
                            sourceSamples[i] = samples[pooled + i];
                            usedTips[i] = tips[pooled + i];
                        }
                        ML_STAR.Channel.PickupTip(usedTips);
                        //aspiration
                        var aspParam = new LLDsParameter() { cLLDSensitivity = LLDSensitivity.LabwareDefinition, SubmergeDepth = 2 };
                        aspParam.LiquidClassParameter.TipType = CoreTipType.HighVolumeTip;
                        aspParam.LiquidClassParameter.DispenseMode = DispenseMode.JetEmpty;
                        aspParam.LiquidClassParameter.LiquidClass = "HighVolume_Water_DispenseJet_Empty";
                        ML_STAR.Channel.Aspirate(sourceSamples, 150, aspParam);
                        //dispense
                        var dispParam = new FixHeightParameter() { FixHeight = 30 };
                        dispParam.LiquidClassParameter.TipType = CoreTipType.HighVolumeTip;
                        dispParam.LiquidClassParameter.DispenseMode = DispenseMode.JetEmpty;
                        dispParam.LiquidClassParameter.LiquidClass = "HighVolume_Water_DispenseJet_Empty";
                        ML_STAR.Channel.Dispense(poolSamples, 150, dispParam);
                        ML_STAR.Channel.EjectTip();
                        pooled += numOfChannel;
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                ML_STAR.End();
                ML_STAR.Dispose();
                ML_STAR = null;
                button1.Enabled = true;
                button2.Enabled = true;
            });
        }
    }
}
