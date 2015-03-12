// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMetaDataImport.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the Guids type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Converters.ComInterfaces
{
    using System;
    using System.Runtime.InteropServices;

    public class Guids
    {
        #region Constants

        public const string IMetaDataImport = "7DAC8207-D3AE-4c75-9B67-92801A497D44";

        #endregion
    }

    [Guid(Guids.IMetaDataImport)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMetaDataImport
    {
        void CloseEnum(int hEnum);

        int CountEnum(int hEnum, ref int count);

        int ResetEnum(int hEnum, int ulPos);

        int EnumTypeDefs(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rTypeDefs, int cMax, ref int pcTypeDefs);

        int EnumInterfaceImpls(
            ref int phEnum, int td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rImpls, int cMax, ref int pcImpls);

        int EnumTypeRefs(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rTypeDefs, int cMax, ref int pcTypeRefs);

        int FindTypeDefByName([MarshalAs(UnmanagedType.LPWStr)] string szTypeDef, int tkEnclosingClass, ref int ptd);

        int GetScopeProps(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName, int cchName, ref int pchName, ref Guid pmvid);

        int GetModuleFromScope(ref int pmd);

        int GetTypeDefProps(
            int td, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szTypeDef, 
            int cchTypeDef, 
            ref int pchTypeDef, 
            ref int pdwTypeDefFlags, 
            ref int ptkExtends);

        int GetInterfaceImplProps(int iiImpl, ref int pClass, ref int ptkIface);

        int GetTypeRefProps(
            int tr, 
            ref int ptkResolutionScope, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] char[] szName, 
            int cchName, 
            ref int pchName);

        int ResolveTypeRef(int tr, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] ref object ppIScope, ref int ptd);

        int EnumMembers(
            ref int phEnum, int cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rMembers, int cMax, ref int pcTokens);

        int EnumMembersWithName(
            ref int phEnum, 
            int cl, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] int[] rMembers, 
            int cMax, 
            ref int pcTokens);

        int EnumMethods(
            ref int phEnum, int cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rMethods, int cMax, ref int pcTokens);

        int EnumMethodsWithName(
            ref int phEnum, 
            int cl, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] int[] rMethods, 
            int cMax, 
            ref int pcTokens);

        int EnumFields(
            ref int phEnum, int cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rFields, int cMax, ref int pcTokens);

        int EnumFieldsWithName(
            ref int phEnum, 
            int cl, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] int[] rFields, 
            int cMax, 
            ref int pcTokens);

        int EnumParams(
            ref int phEnum, int mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rParams, int cMax, ref int pcTokens);

        int EnumMemberRefs(
            ref int phEnum, 
            int tkParent, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rMemberRefs, 
            int cMax, 
            ref int pcTokens);

        int EnumMethodImpls(
            ref int phEnum, 
            int td, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rMethodBody, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rMethodDecl, 
            int cMax, 
            ref int pcTokens);

        int EnumPermissionSets(
            ref int phEnum, 
            int tk, 
            int dwActions, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rPermission, 
            int cMax, 
            ref int pcTokens);

        int FindMember(
            int td, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pvSigBlob, 
            int cbSigBlob, 
            ref int pmb);

        int FindMethod(
            int td, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pvSigBlob, 
            int cbSigBlob, 
            ref int pmb);

        int FindField(
            int td, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pvSigBlob, 
            int cbSigBlob, 
            ref int pmb);

        int FindMemberRef(
            int td, 
            [MarshalAs(UnmanagedType.LPWStr)] string szName, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pvSigBlob, 
            int cbSigBlob, 
            ref int pmr);

        int GetMethodProps(
            int mb, 
            ref int pClass, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szMethod, 
            int cchMethod, 
            ref int pchMethod, 
            ref int pdwAttr, 
            ref IntPtr ppvSigBlob, 
            ref int pcbSigBlob, 
            ref int pulCodeRVA, 
            ref int pdwImplFlags);

        int GetMemberRefProps(
            int mr, 
            ref int ptk, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szMember, 
            int cchMember, 
            ref int pchMember, 
            ref IntPtr ppvSigBlob, 
            ref int pbSigBlob);

        int EnumProperties(
            ref int phEnum, int td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rProperties, int cMax, ref int pcProperties);

        int EnumEvents(
            ref int phEnum, int td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rEvents, int cMax, ref int pcEvents);

        int GetEventProps(
            int ev, 
            ref int pClass, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szEvent, 
            int cchEvent, 
            ref int pchEvent, 
            ref int pdwEventFlags, 
            ref int ptkEventType, 
            ref int pmdAddOn, 
            ref int pmdRemoveOn, 
            ref int pmdFire, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 10)] int[] rmdOtherMethod, 
            int cMax, 
            ref int pcOtherMethod);

        int EnumMethodSemantics(
            ref int phEnum, int mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] rEventProp, int cMax, ref int pcEventProp);

        int GetMethodSemantics(int mb, int tkEventProp, ref int pdwSemanticsFlags);

        int GetClassLayout(
            int td, 
            ref int pdwPackSize, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] long[] rFieldOffset, 
            int cMax, 
            ref int pcFieldOffset, 
            ref int pulClassSize);

        int GetFieldMarshal(int tk, ref IntPtr ppvNativeType, ref int pcbNativeType);

        int GetRVA(int tk, ref int pulCodeRVA, ref int pdwImplFlags);

        int GetPermissionSetProps(int pm, ref int pdwAction, ref IntPtr ppvPermission, ref int pcbPermission);

        int GetSigFromToken(int mdSig, ref IntPtr ppvSig, ref int pcbSig);

        int GetModuleRefProps(int mur, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] char[] szName, int cchName, ref int pchName);

        int EnumModuleRefs(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] rModuleRefs, int cmax, ref int pcModuleRefs);

        int GetTypeSpecFromToken(int typespec, ref IntPtr ppvSig, ref int pcbSig);

        int GetNameFromToken(int tk, ref IntPtr pszUtf8NamePtr);

        int EnumUnresolvedMethods(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] rMethods, int cMax, ref int pcTokens);

        int GetUserString(int stk, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] char[] szString, int cchString, ref int pchString);

        int GetPinvokeMap(
            int tk, 
            ref int pdwMappingFlags, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szImportName, 
            int cchImportName, 
            ref int pchImportName, 
            ref int pmrImportDLL);

        int EnumSignatures(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] rSignatures, int cmax, ref int pcSignatures);

        int EnumTypeSpecs(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] rTypeSpecs, int cmax, ref int pcTypeSpecs);

        int EnumUserStrings(
            ref int phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] rStrings, int cmax, ref int pcStrings);

        int GetParamForMethodIndex(int md, int ulParamSeq, ref int ppd);

        int EnumCustomAttributes(
            ref int phEnum, 
            int tk, 
            int tkType, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] rCustomAttributes, 
            int cMax, 
            ref int pcCustomAttributes);

        int GetCustomAttributeProps(int cv, ref int ptkObj, ref int ptkType, ref IntPtr ppBlob, ref int pcbSize);

        int FindTypeRef(int tkResolutionScope, [MarshalAs(UnmanagedType.LPWStr)] string szName, ref int ptr);

        int GetMemberProps(
            int mb, 
            ref int pClass, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szMember, 
            int cchMember, 
            ref int pchMember, 
            ref int pdwAttr, 
            ref IntPtr ppvSigBlob, 
            ref int pcbSigBlob, 
            ref int pulCodeRVA, 
            ref int pdwImplFlags, 
            ref int pdwCPlusTypeFlag, 
            ref IntPtr ppValue, 
            ref int pcchValue);

        int GetFieldProps(
            int mb, 
            ref int pClass, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szField, 
            int cchField, 
            ref int pchField, 
            ref int pdwAttr, 
            ref IntPtr ppvSigBlob, 
            ref int pcbSigBlob, 
            ref int pdwCPlusTypeFlag, 
            ref IntPtr ppValue, 
            ref int pcchValue);

        int GetPropertyProps(
            int prop, 
            ref int pClass, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szProperty, 
            int cchProperty, 
            ref int pchProperty, 
            ref int pdwPropFlags, 
            ref IntPtr ppvSig, 
            ref int pbSig, 
            ref int pdwCPlusTypeFlag, 
            ref IntPtr ppDefaultValue, 
            ref int pcchDefaultValue, 
            ref int pmdSetter, 
            ref int pmdGetter, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 13)] int[] rmdOtherMethod, 
            int cMax, 
            ref int pcOtherMethod);

        int GetParamProps(
            int tk, 
            ref int pmd, 
            ref int pulSequence, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] char[] szName, 
            int cchName, 
            ref int pchName, 
            ref int pdwAttr, 
            ref int pdwCPlusTypeFlag, 
            ref IntPtr ppValue, 
            ref int pcchValue);

        int GetCustomAttributeByName(int tkObj, [MarshalAs(UnmanagedType.LPWStr)] string szName, ref IntPtr ppData, ref int pcbData);

        bool IsValidToken(int tk);

        int GetNestedClassProps(int tdNestedClass, ref int ptdEnclosingClass);

        int GetNativeCallConvFromSig(IntPtr pvSig, int cbSig, ref int pCallConv);

        int IsGlobal(int pd, ref int pbGlobal);
    }
}