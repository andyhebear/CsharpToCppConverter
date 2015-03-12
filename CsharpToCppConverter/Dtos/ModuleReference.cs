namespace Converters.Dtos
{
    using System;

    public class ModuleReference : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }
    }
}
