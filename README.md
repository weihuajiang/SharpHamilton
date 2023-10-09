[![NuGet](https://img.shields.io/badge/nuget-v0.1.0-blue)](https://www.nuget.org/packages/SharpHamilton/)
# SharpHamilton

C# library to control Hamilton STAR line. 

With SharpHamilton, application which used Hamilton STAR line can be developed with totally C# code, instead of method/hsl and C# code of application software. Which can make programming and maintenance much easier.


>**Limitation**
>This library was only tested in Venus 4.4.

>**Disclaimer**
>SharpHamilton is not officially endorsed or supported by the Hamilton Company. Please used it for test only.

## Document

[Document online available](https://weihuajiang.github.io/SharpHamiltonDoc/)

## Features

* Control STARlet/STAR/STAR plus with pure C# code
* No dependency without method or hsl file
* Support venus-styled error handling
* Support fully manipulation of deck layout, rack and container
* Low cost and high performance for no HxRun running and no RPC communiation between application and COM in HxRun
* Run in windows xp/7/10/11

## Progress
* **Deck layout**: labware manipulation and volume computation
* **Master Module**: Initialize, Door, Firmware, Query carrier presence on deck
* **Arm**: movemet of dual arm
* **Channel**: Tip pickup, tip eject, aspirate, dispense, move, get last liquid level, set tip tracking speed, get exclude state, wait for TADM upload, ADC/MAD control
* **CORE-gripper** (under development)
* **Autoload** (under development)
* **CORE-gripper** (under development)
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

instrument control
```csharp
ML_STAR = new STARCommand();
ML_STAR.Init(deck, hwnd, false);
ML_STAR.Start();
ML_STAR.Initialize();
ML_STAR.Channel.PickupTip(tips);
ML_STAR.Channel.Aspirate(samples, 150, aspParam);
ML_STAR.Channel.Dispense(samples, 150, dispParam);
ML_STAR.Channel.EjectTip();
ML_STAR.End();
```

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
