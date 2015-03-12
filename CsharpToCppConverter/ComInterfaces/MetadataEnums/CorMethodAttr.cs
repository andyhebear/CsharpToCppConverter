namespace Converters.ComInterfaces.MetadataEnums
{
    using System;

    [Flags]
    public enum CorMethodAttr
    {
        // MemberAccessMask = 0x0007,
        PrivateScope = 0x0000,

        Private = 0x0001,

        FamAndAssem = 0x0002,

        Assem = 0x0003,

        Family = 0x0004,

        FamOrAssem = 0x0005,

        Public = 0x0006,

        Static = 0x0010,

        Final = 0x0020,

        Virtual = 0x0040,

        HideBySig = 0x0080,

        VtableLayoutMask = 0x0100,

        ReuseSlot = 0x0000,

        NewSlot = 0x0100,

        CheckAccessOnOverride = 0x0200,

        Abstract = 0x0400,

        SpecialName = 0x0800,

        PinvokeImpl = 0x2000,

        UnmanagedExport = 0x0008,

        ReservedMask = 0xd000,

        RuntimeSpecialName = 0x1000,

        HasSecurity = 0x4000,

        RequireSecObject = 0x8000,
    }
}
