namespace Converters.ComInterfaces.MetadataEnums
{
    using System;

    [Flags]
    public enum CorEventAttr
    {
        SpecialName = 0x0200,

        // ReservedMask = 0x0400,
        RuntimeSpecialName = 0x0400,
    }
}
