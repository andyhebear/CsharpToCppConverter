namespace Converters.ComInterfaces.MetadataEnums
{
    using System;

    [Flags]
    public enum CorPinvokeMap
    {
        NoMangle = 0x1,

        CharSetAnsi = 0x2,

        CharSetUnicode = 0x4,

        CharSetAuto = 0x6,

        BestFitEnabled = 0x10,

        BestFitDisabled = 0x20,

        ThrowOnUnmappableCharEnabled = 0x1000,

        ThrowOnUnmappableCharDisabled = 0x2000,

        SupportsLastError = 0x40,

        CallConvWinapi = 0x100,

        CallConvCdecl = 0x200,

        CallConvStdcall = 0x300,

        CallConvThiscall = 0x400,

        CallConvFastcall = 0x500
    }
}
