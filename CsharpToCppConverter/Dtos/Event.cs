namespace Converters.Dtos
{
    using Converters.ComInterfaces.MetadataEnums;

    public class Event : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public CorEventAttr Flags { get; set; }

        /// <summary>
        /// ReturnTypeDefinition or TypeReference
        /// </summary>
        public object Type { get; set; }

        public Method AddOn { get; set; }

        public Method RemoveOn { get; set; }

        public Method Fire { get; set; }

        public Method[] Other { get; set; }
    }
}
