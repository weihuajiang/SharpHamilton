[![NuGet](https://img.shields.io/badge/nuget-v0.1.0-blue)](https://www.nuget.org/packages/SharpHamilton/)
# SharpHamilton

C# library to control Hamilton STAR line. 

With SharpHamilton, application which used Hamilton STAR line (STAR/STARlet/STAR plus/STAR vantage) can be developed with totally C# code, instead of method/hsl and C# code of application software. Which can make programming and maintenance much easier.

SharpHamilton support Venus 4.4 to 6, it also support Vantage in Venus 6.

>**Disclaimer**
>SharpHamilton is not officially endorsed or supported by the Hamilton Company. Please used it for test only.

## Test it in RoslynPad

We integrated this tool to [RoslynPad](https://github.com/roslynpad/roslynpad), you can write C# script to control STAR/STARlet/STARplus, please download it from following [link](https://github.com/weihuajiang/SharpHamilton/releases/download/v0.3.0/Release.zip).

![image](https://github.com/weihuajiang/SharpHamilton/assets/12489873/be7b9ebe-9753-45d1-b208-dbb90d9f0520)


## Why did we developed it?

To simplify application development with Hamilton STAR line. [Read more](WHY.md)

## Document

[Document online available](https://weihuajiang.github.io/SharpHamiltonDoc/)

## Features

* Control STARlet/STAR/STAR plus/Vantage with pure C# code
* No dependency without method or hsl file
* Support venus-styled error handling
* Support fully manipulation of deck layout, rack and container
* Low cost and high performance for no HxRun running and no inter process communiation between application and COM in HxRun
* Support sequence and smart step, you can write venus styled script
* Support venus from 4.4 to 6
* Support windows xp/7/10/11

## Progress
* **Deck layout**: labware manipulation and volume computation
* **Sequence**: use sequence in single step and smart step
* **Master Module**: Initialize, Door, Firmware, Query carrier presence on deck
* **Arm**: movemet of dual arm
* **Channel**: Tip pickup, tip eject, aspirate, dispense, move, get last liquid level, set tip tracking speed, get exclude state, wait for TADM upload, ADC/MAD control. Smart steps like venus were finished
* **CORE-gripper** 
* **Autoload** (under development)
* **5ml Channel** (under development)
* **5ml CORE-gripper** (under development)
* **iSWAP** (under development)
* **MPH 96** (under development)
* **MPH 384** (under development)
* **Heater Shaker** (under development)
* **Centrifuge** (under development)

## Example

monitoring instrument connection status

```csharp
STARDiscover discover = new STARDiscover();
discover.OnConnect += Discover_OnConnect;
discover.OnDisconnect += Discover_OnDisconnect;
discover.OnReceive += Discover_OnReceive;
discover.Start();

discover.Stop();
discover.OnConnect -= Discover_OnConnect;
discover.OnDisconnect -= Discover_OnDisconnect;
discover.OnReceive -= Discover_OnReceive;
discover.Dispose();
discover = null;
```

manipulation of deck layout, use array of containers instead of sequence
```csharp
var labwPath = STARRegistry.LabwarePath;
ML_STAR.Deck.AddLabwareToDeckSite(Path.Combine(labwPath, @"ML_STAR\SMP_CAR_32_12x75_A00.rck"), "1T-7", "Smp");
var smp = ML_STAR.Deck["Smp"].Containers;
var samples = new Container[] { smp["1"], smp["2"], smp["3"], smp["4"], smp["5"], smp["6"], smp["7"], smp["8"] };
```

You can still use sequence defined in deck layout, or generate sequence from rack.

```csharp
var samples=ML_STAR.GetSequence("Sample");
```

Both sequence or array of containers can be used for pipetting or tip handling. So you can programe with sequence like in Venus to control STAR,

```csharp
ML_STAR = new STARCommand();
ML_STAR.Init(deck, hwnd, false);
ML_STAR.Start();
ML_STAR.Initialize();
while(samples.Current!=-1){
    ML_STAR.Channel.PickupTip(tips);
    ML_STAR.Channel.Aspirate(samples, 150, aspParam);
    ML_STAR.Channel.Dispense(samples2, 150, dispParam);
    ML_STAR.Channel.EjectTip();
}
ML_STAR.End();
```

or you can write script with smart step, only one step to finish the works above

```csharp
using Huarui.STARLine.SmartStep;
ML_STAR.Simple(samples, samples2, 150, parameter, dparameter, tip);
```
and you can use array of container for single step.

venus-styled error handling. If recovery was set to cancel, the error will be throwed as STARException
```csharp
ErrorRecoveryOptions options = new ErrorRecoveryOptions();
options.Add(MainErrorEnum.InsufficientLiquidError, new ErrorRecoveryOption() { Recovery = RecoveryAction.Bottom });
ML_STAR.Channel.Aspirate(smples, 150, aspParam, AspirateMode.Aspiration, options);
```
get last liquid level and compute its volume
```csharp
var levels = ML_STAR.Channel.GetLastLiquidLevel();
for(int i = 0; i < ML_STAR.Channel.Count; i++)
{
    var volume = samples[i].ComputeVolume(levels[i] - samples[i].Z);
    Console.WriteLine($"volume for sample {samples[i].Position} is {volume}");
}
```
set tip tracking speed
```csharp
ML_STAR.Channel.SetTipTrackSpeed(2);
```
turn on anti-droplet control and clot detection with MAD
```csharp
ML_STAR.Channel.AntiDropletControl = true;
ML_STAR.Channel.AspirationMonitoring = true;
```

## Fix WinForm control size changed when using SharpHamilton

disable the auto-resizing feature, add following code to app.confg
```
<configuration>
  <!-- ... other xml settings ... -->

  <System.Windows.Forms.ApplicationConfigurationSection>
    <add key="DpiAwareness" value="PerMonitorV2" />
    <add key="EnableWindowsFormsHighDpiAutoResizing" value="false" />
  </System.Windows.Forms.ApplicationConfigurationSection>

</configuration>
```

## 3D Simulation (under development)


https://github.com/weihuajiang/SharpHamilton/assets/12489873/c9377af2-715e-4119-9844-c55088f86ba5

