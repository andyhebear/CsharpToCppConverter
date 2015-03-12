namespace Converters.Dtos
{
    using System;

    public class TypeReference : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public ModuleReference Module { get; set; }
    }
}
