namespace Converters.ComInterfaces.MetadataEnums
{
    using System;

    [Flags]
    public enum CorCallingConvention
    {
        Default = 0x0,

        C = 0x1,

        StdCall = 0x2,

        ThisCall = 0x3,

        FastCall = 0x4,

        VarArg = 0x5,

        Field = 0x6,

        LocalSig = 0x7,

        Property = 0x8,

        Unmgd = 0x9,

        GenericInst = 0xa,

        NativeVarArg = 0xb,

        Max = 0xc,

        Mask = 0x0f,

        Generic = 0x10,

        HasThis = 0x20,

        ExplicitThis = 0x40,

        Sentinel = 0x41,
    }
}
