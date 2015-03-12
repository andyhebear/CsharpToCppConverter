namespace Converters.Adapters
{
    using System;
    using System.Collections.Generic;

    using Converters.Dtos;
    using Converters.Metadata;

    public class FieldMetadataICodeElementAdapter : TypeDefinitionMetadataICodeElementAdapterBase
    {
        public FieldMetadataICodeElementAdapter(Field field, TypeDefinition typeDefinition, IList<TypeDescriptor> genericTypes, MetadataReader reader)
            : base(typeDefinition, genericTypes, reader)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            this.Field = field;
        }

        public Field Field { get; protected set; }

        public override string FullyQualifiedName
        {
            get { return string.Concat(base.FullyQualifiedName, '.', this.Field.FullName); }
        }
    }
}
