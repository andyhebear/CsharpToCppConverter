namespace Converters.Dtos
{
    using Converters.ComInterfaces.MetadataEnums;

    public class Field : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public CorFieldAttr Flags { get; set; }

        public byte[] SigBlob { get; set; }

        public int CodeRva { get; set; }

        public CorElementType CPlusTypeFlag { get; set; }

        public byte[] Value { get; set; }
    }
}
