namespace Converters.Dtos
{
    using Converters.ComInterfaces.MetadataEnums;

    public class Member : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public int Flags { get; set; }

        public byte[] SignatureBlob { get; set; }

        public int CodeRva { get; set; }

        public CorMethodImpl ImplementationFlags { get; set; }

        public CorElementType CPlusTypeFlag { get; set; }

        public byte[] Value { get; set; }
    }
}
