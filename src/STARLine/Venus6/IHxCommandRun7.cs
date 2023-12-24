using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hamilton.Interop.Venus6
{
    [ComImport]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    [Guid("9C787C57-A65F-415A-BF68-A878A002769E")]
    internal interface IHxCommandRun7
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1)]
        void InitCommandRun([In][MarshalAs(UnmanagedType.BStr)] string runName, [In][MarshalAs(UnmanagedType.BStr)] string uniqueRunId, [In][MarshalAs(UnmanagedType.Struct)] object cmdRunCfgFil, [In][MarshalAs(UnmanagedType.IDispatch)] object cmdRunTrace, [In][MarshalAs(UnmanagedType.IDispatch)] object cmdRunDeckLayout, [In][MarshalAs(UnmanagedType.IDispatch)] object cmdRunSampleTracker, [In] int cmdRunHWnd, [In][MarshalAs(UnmanagedType.BStr)] string instrumentKey, [In] Hamilton.Interop.HxGruCommand.HxSimulationModes simulationMode, [In][MarshalAs(UnmanagedType.IDispatch)] object collisionControl);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(2)]
        void GetCommandRunParameters([MarshalAs(UnmanagedType.BStr)] out string runName, [MarshalAs(UnmanagedType.BStr)] out string uniqueRunId, [MarshalAs(UnmanagedType.Struct)] out object cmdRunCfgFil, [MarshalAs(UnmanagedType.IDispatch)] out object cmdRunTrace, [MarshalAs(UnmanagedType.IDispatch)] out object cmdRunDeckLayout, [MarshalAs(UnmanagedType.IDispatch)] out object cmdRunSampleTracker, out int cmdRunHWnd, [MarshalAs(UnmanagedType.BStr)] out string instrumentKey, out Hamilton.Interop.HxGruCommand.HxSimulationModes simulationMode, [MarshalAs(UnmanagedType.IDispatch)] out object collisionControl);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(3)]
        void SetEventIdentifier([In] int cmdId);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(4)]
        void Abort();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(5)]
        void Pause();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(6)]
        void Resume();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(7)]
        void ShowControlPanel([In] int hwnd, [In] Hamilton.Interop.HxGruCommand.ControlPanelServices ControlPanelServices);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(8)]
        Hamilton.Interop.HxGruCommand.ControlPanelServices GetControlPanelServices();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(9)]
        void StartMethod();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(10)]
        void EndMethod();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(11)]
        void AbortRequested();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(12)]
        int GetEventIdentifier();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(13)]
        void StartMethodAbortHandler();
    }
}
