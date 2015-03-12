// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMetaDataAssemblyImport.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the ASSEMBLYMETADATA type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Converters.ComInterfaces
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ASSEMBLYMETADATA
    {
        public ushort usMajorVersion;

        public ushort usMinorVersion;

        public ushort usBuildNumber;

        public ushort usRevisionNumber;

        public IntPtr szLocale;

        public int cbLocale;

        public IntPtr rProcessor;

        public int ulProcessor;

        public IntPtr rOS;

        public int ulOS;
    }

    [ComImport()]
    [Guid("EE62470B-E94B-424e-9B7C-2F00C9249F93")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMetaDataAssemblyImport
    {
        int GetAssemblyProps(
            int mda, 
            ref byte[] ppbPublicKey, 
            ref int pcbPublicKey, 
            ref int pulHashAlgId, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] szName, 
            ref int cchName, 
            ref int pchName, 
            ref byte[] pMetaData, 
            ref int pdwAssemblyFlags);

        int GetAssemblyRefProps(
            int mdar, 
            ref IntPtr ppbPublicKeyOrToken, 
            ref int pcbPublicKeyOrToken, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] szName, 
            int cchName, 
            ref int pchName, 
            ref ASSEMBLYMETADATA pMetaData, 
            ref IntPtr ppbHashValue, 
            ref int pcbHashValue, 
            ref int pdwAssemblyRefFlags);

        int GetFileProps(
            int mdf, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szName, 
            int cchName, 
            ref int pchName, 
            ref IntPtr ppbHashValue, 
            ref int pcbHashValue, 
            ref int pdwFileFlags);

        int GetExportedTypeProps(
            int mdct, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szName, 
            int cchName, 
            ref int pchName, 
            ref int ptkImplementation, 
            ref int ptkTypeDef, 
            ref int pdwExportedTypeFlags);

        int GetManifestResourceProps(
            int mdmr, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szName, 
            int cchName, 
            ref int pchName, 
            ref int ptkImplementation, 
            ref int pdwOffset, 
            ref int pdwResourceFlags);

        int EnumAssemblyRefs(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rAssemblyRefs, int cMax, ref int pcTokens);

        int EnumFiles(ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rFiles, int cMax, ref int pcTokens);

        int EnumExportedTypes(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rExportedTypes, int cMax, ref int pcTokens);

        int EnumManifestResources(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rManifestResources, int cMax, ref int pcTokens);

        int GetAssemblyFromScope(ref int ptkAssembly);

        int FindExportedTypeByName([MarshalAs(UnmanagedType.LPWStr)] string szName, int mdtExportedType, ref int ptkExportedType);

        int FindManifestResourceByName([MarshalAs(UnmanagedType.LPWStr)] string szName, ref int ptkManifestResource);

        void CloseEnum(int hEnum);

        int FindAssembliesByName(
            [MarshalAs(UnmanagedType.LPWStr)] string szAppBase, 
            [MarshalAs(UnmanagedType.LPWStr)] string szPrivateBin, 
            [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, 
            object[] ppIUnk, 
            int cMax, 
            ref int pcAssemblies);
    }
}