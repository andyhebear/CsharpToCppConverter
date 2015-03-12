namespace Converters.ComInterfaces.MetadataEnums
{
    using System;

    [Flags]
    public enum CorParamAttr
    {
        In = 0x0001,

        Out = 0x0002,

        Optional = 0x0010,

        // ReservedMask = 0xf000,
        HasDefault = 0x1000,

        HasFieldMarshal = 0x2000,

        Unused = 0xcfe0,
    }
}