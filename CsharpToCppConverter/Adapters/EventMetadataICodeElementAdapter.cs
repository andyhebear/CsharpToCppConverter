namespace Converters.Adapters
{
    using System;
    using System.Collections.Generic;

    using Converters.Dtos;
    using Converters.Metadata;

    public class EventMetadataICodeElementAdapter : TypeDefinitionMetadataICodeElementAdapterBase
    {
        public EventMetadataICodeElementAdapter(Event @event, TypeDefinition typeDefinition, IList<TypeDescriptor> genericTypes, MetadataReader reader)
            : base(typeDefinition, genericTypes, reader)
        {
            if (@event == null)
            {
                throw new ArgumentNullException("event");
            }

            this.Event = @event;
        }

        public Event Event { get; protected set; }

        public override string FullyQualifiedName
        {
            get { return string.Concat(base.FullyQualifiedName, '.', this.Event.FullName); }
        }
    }
}
