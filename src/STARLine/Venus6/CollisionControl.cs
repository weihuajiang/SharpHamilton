using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hamilton.Interop.Venus6
{
    [ComVisible(true)]
    public class CollisionAvoidanceSystemCallback : ICollisionAvoidanceSystemCallback
    {
        public void Abort()
        {
        }
    }
    [ComImport]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    [Guid("57834D8B-F508-430C-A89E-CCA57E7C0566")]
    [DefaultMember("Abort")]
    internal interface ICollisionAvoidanceSystemCallback
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        void Abort();
    }
    [ComImport]
    [DefaultMember("ResetCollisionControl")]
    [Guid("7189BB4C-BF46-400E-91DD-F5B4A9D0D0CF")]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    internal interface ICollisionControlResetter
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        void ResetCollisionControl();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1)]
        void InitializeCollisionControl([In][MarshalAs(UnmanagedType.IDispatch)] object trace, [In][MarshalAs(UnmanagedType.IDispatch)] object callback);
    }
    [ComImport]
    [CoClass(typeof(CollisionControlResetterClass))]
    [Guid("7189BB4C-BF46-400E-91DD-F5B4A9D0D0CF")]
    internal interface CollisionControlResetter : ICollisionControlResetter
    {
    }
    [ComImport]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [Guid("4FD2F32E-62A6-4892-8483-4D63741C5569")]
    [DefaultMember("ResetCollisionControl")]
    [ClassInterface(ClassInterfaceType.None)]
    internal class CollisionControlResetterClass : ICollisionControlResetter, CollisionControlResetter
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        public virtual extern void ResetCollisionControl();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1)]
        public virtual extern void InitializeCollisionControl([In][MarshalAs(UnmanagedType.IDispatch)] object trace, [In][MarshalAs(UnmanagedType.IDispatch)] object callback);
    }
}
