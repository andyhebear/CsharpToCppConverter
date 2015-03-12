namespace Converters.Dtos
{
    using System;
    using System.Collections.Generic;

    using Converters.ComInterfaces.MetadataEnums;

    public class TypeDescriptor
    {
        public CorElementType ElementType { get; set; }

        public TypeDefinition TypeDefinition { get; set; }

        public ulong GenericParamNumber { get; set; }

        public ulong GenericParametersCount { get; set; }

        public IList<TypeDescriptor> GenericTypes { get; set; }
    }
}
