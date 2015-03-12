namespace Converters.Adapters
{
    using System;
    using System.Collections.Generic;

    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Dtos;
    using Converters.Metadata;

    public class TypeDefinitionMetadataICodeElementAdapterBase : MetadataICodeElementAdapter
    {
        public TypeDefinitionMetadataICodeElementAdapterBase(TypeDefinition typeDefinition, IList<TypeDescriptor> genericTypes, MetadataReader reader)
            : base(reader)
        {
            if (typeDefinition == null)
            {
                throw new ArgumentNullException("typeDefinition");
            }

            this.TypeDefinition = typeDefinition;
            this.GenericTypes = genericTypes;
        }

        public TypeDefinition TypeDefinition { get; protected set; }

        public IList<TypeDescriptor> GenericTypes { get; protected set; }

        public override string FullyQualifiedName
        {
            get { return string.Concat("Root.", TypeDefinition.FullName); }
        }

        public override bool IsValueType
        {
            get
            {
                return this.BaseIsValueTypeRecurse(this.TypeDefinition);
            }
        }

        private bool BaseIsValueTypeRecurse(TypeDefinition @base)
        {
            if (@base == null)
            {
                return false;
            }

            string fullName = @base.FullName;
            if (fullName.Equals("Platform.ValueType") || fullName.Equals("System.Enum"))
            {
                return true;
            }

            var baseReference = @base.BaseReference;
            if (baseReference != null 
                && (baseReference.FullName.Equals("Platform.ValueType") 
                    || baseReference.FullName.Equals("System.Enum")))
            {
                return true;
            }

            return this.BaseIsValueTypeRecurse(@base.Base);
        }
    }
}