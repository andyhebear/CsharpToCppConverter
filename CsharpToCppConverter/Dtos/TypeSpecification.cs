namespace Converters.Dtos
{
    using System;

    using Converters.ComInterfaces;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Metadata;

    public class TypeSpecification
    {
        public int Token { get; set; }
        
        public byte[] SignatureBlob { get; set; }

        // get info from Signature - http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf typeSpec
        public TypeDescriptor TypeDescriptor { get; private set; }

        public void ReadSignature(MetadataReader reader)
        {
            var position = 0;
            this.TypeDescriptor = this.SignatureBlob.ReadSignatureBlobType(reader, ref position);
        }
    }
}
