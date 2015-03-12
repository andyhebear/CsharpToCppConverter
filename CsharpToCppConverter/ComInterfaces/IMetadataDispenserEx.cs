namespace Converters.ComInterfaces
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport(), GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMetadataDispenserEx
    {
        int DefineScope(ref Guid rclsid, int dwCreateFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] ref object ppIUnk);

        int OpenScope([MarshalAs(UnmanagedType.LPWStr)] string szScope, int dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] ref object ppIUnk);
        
        int OpenScopeOnMemory(IntPtr pData, int cbData, int dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] ref object ppIUnk);
        
        int SetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)] object value);
        
        int GetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)] ref object pvalue);
        
        int OpenScopeOnITypeInfo([MarshalAs(UnmanagedType.Interface)] object pITI, int dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] ref object ppIUnk);
        
        int GetCORSystemDirectory([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szBuffer, int cchBuffer, ref int pchBuffer);
        
        int FindAssembly([MarshalAs(UnmanagedType.LPWStr)] string szAppBase, [MarshalAs(UnmanagedType.LPWStr)] string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)] string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] szName, int cchName, ref int pcName);
        
        int FindAssemblyModule([MarshalAs(UnmanagedType.LPWStr)] string szAppBase, [MarshalAs(UnmanagedType.LPWStr)] string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)] string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, [MarshalAs(UnmanagedType.LPWStr)] string szModuleName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] char[] szName, int cchName, ref int pcName);
    }

    [ComImport(), GuidAttribute("E5CB7A31-7512-11D2-89CE-0080C792E5D8")]
    public class CorMetaDataDispenserExClass
    {
    }

    [ComImport(), GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"), CoClass(typeof(CorMetaDataDispenserExClass))]
    public interface MetaDataDispenserEx : IMetadataDispenserEx
    {
    }
}