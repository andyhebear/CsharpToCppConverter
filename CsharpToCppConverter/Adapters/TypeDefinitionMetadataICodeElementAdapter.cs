namespace Converters.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Dtos;
    using Converters.Metadata;

    using StyleCop;

    public class TypeDefinitionMetadataICodeElementAdapter : TypeDefinitionMetadataICodeElementAdapterBase
    {
        public TypeDefinitionMetadataICodeElementAdapter(TypeDescriptor typeDescriptor, MetadataReader reader)
            : base(typeDescriptor.TypeDefinition, typeDescriptor.GenericTypes, reader)
        {
            this.TypeDescriptor = typeDescriptor;
        }

        public TypeDefinitionMetadataICodeElementAdapter(TypeDefinition typeDefinition, IList<TypeDescriptor> genericTypes, MetadataReader reader)
            : base(typeDefinition, genericTypes, reader)
        {
        }

        public TypeDescriptor TypeDescriptor { get; private set; }

        public override bool IsClassName
        {
            get
            {
                return TypeDefinition.Type == CorTypeAttr.Class || TypeDefinition.Type.HasFlag(CorTypeAttr.AutoClass)
                       || TypeDefinition.Type.HasFlag(CorTypeAttr.AnsiClass);
            }
        }

        public override IEnumerable<ICodeElement> ChildCodeElements
        {
            get
            {
                var genericTypes = this.TypeDescriptor != null ? TypeDescriptor.GenericTypes : null;
                foreach (var field in Reader.EnumerateFields(TypeDefinition))
                {
                    yield return new FieldMetadataICodeElementAdapter(field, TypeDefinition, genericTypes, Reader);
                }

                foreach (var @event in Reader.EnumerateEvents(TypeDefinition))
                {
                    yield return new EventMetadataICodeElementAdapter(@event, TypeDefinition, genericTypes, Reader);
                }

                foreach (var property in Reader.EnumerateProperties(TypeDefinition))
                {
                    yield return new PropertyMetadataICodeElementAdapter(property, TypeDefinition, genericTypes, Reader);
                }

                foreach (var method in Reader.EnumerateMethods(TypeDefinition))
                {
                    yield return new MethodMetadataICodeElementAdapter(method, TypeDefinition, genericTypes, Reader);
                }
            }
        }
    }
}
