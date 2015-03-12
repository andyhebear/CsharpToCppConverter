namespace Converters.Adapters
{
    using System;
    using System.Collections.Generic;

    using Converters.Dtos;
    using Converters.Metadata;

    public class PropertyMetadataICodeElementAdapter : TypeDefinitionMetadataICodeElementAdapterBase
    {
        public PropertyMetadataICodeElementAdapter(Property property, TypeDefinition typeDefinition, IList<TypeDescriptor> genericTypes, MetadataReader reader)
            : base(typeDefinition, genericTypes, reader)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            this.Property = property;
        }

        public Property Property { get; protected set; }

        public override bool IsValueType
        {
            get
            {
                if (this.Property.TypeDescriptor.ElementType == ComInterfaces.MetadataEnums.CorElementType.ELEMENT_TYPE_VALUETYPE)
                {
                    return true;
                }

                return base.IsValueType;
            }
        }

        public override string FullyQualifiedName
        {
            get { return string.Concat(base.FullyQualifiedName, '.', this.Property.FullName); }
        }
    }
}
