namespace Converters.Adapters
{
    using System;
    using System.Collections.Generic;

    using Converters.Dtos;
    using Converters.Metadata;

    public class MethodMetadataICodeElementAdapter : TypeDefinitionMetadataICodeElementAdapterBase
    {
        public MethodMetadataICodeElementAdapter(Method method, TypeDefinition typeDefinition, IList<TypeDescriptor> genericTypes, MetadataReader reader)
            : base(typeDefinition, genericTypes, reader)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            this.Method = method;
        }

        public Method Method { get; protected set; }

        public override string FullyQualifiedName
        {
            get { return string.Concat(base.FullyQualifiedName, '.', this.Method.FullName); }
        }
    }
}
