namespace Converters.ComInterfaces.MetadataEnums
{
    using System;

    [Flags]
    public enum CorPropertyAttr
    {
        SpecialName = 0x0200,

        // ReservedMask = 0xf400,
        RunTimeSpecialName = 0x0400,

        HasDefault = 0x1000,

        Unused = 0xe9ff
    }
}
