namespace Converters.Dtos
{
    using Converters.ComInterfaces.MetadataEnums;

    public class TypeDefinition : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public CorTypeAttr Type { get; set; }

        public TypeDefinition Base { get; set; }

        public TypeReference BaseReference { get; set; }

        public object[] Interfaces { get; set; }
    }
}
