namespace Converters.Dtos
{
    using Converters.ComInterfaces.MetadataEnums;

    public class Param : IFullName
    {
        public int Token { get; set; }

        /// <summary>
        /// The sequence values in ulParamSeq begin with 1 for parameters. A return value has a sequence number of 0
        /// </summary>
        public int Sequence { get; set; }

        public string FullName { get; set; }

        public CorParamAttr Flags { get; set; }

        public CorElementType CPlusTypeFlag { get; set; }

        public byte[] Value { get; set; }
    }
}
