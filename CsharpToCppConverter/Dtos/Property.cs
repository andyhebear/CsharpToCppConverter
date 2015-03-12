namespace Converters.Dtos
{
    using System;

    using Converters.ComInterfaces;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Metadata;

    public class Property : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public CorPropertyAttr Flags { get; set; }

        public byte[] SignatureBlob { get; set; }

        public CorElementType CPlusTypeFlag { get; set; }

        /// <summary>
        /// if pdwCPlusTypeFlag is ELEMENT_TYPE_STRING
        /// </summary>
        public byte[] DefaultValue { get; set; }

        public Method Getter { get; set; }

        public Method Setter { get; set; }

        public Method[] Other { get; set; }

        // get info from Signature - http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf propertySig
        public CorCallingConvention CallingConvention { get; private set; }

        public int ParamCount { get; private set; }

        public TypeDescriptor TypeDescriptor { get; private set; }

        public void ReadSignature(MetadataReader reader)
        {
            if (this.SignatureBlob == null || this.SignatureBlob.Length == 0)
            {
                throw new ArgumentException("SignatureBlob is empty");
            }

            this.CallingConvention = (CorCallingConvention)(this.SignatureBlob[0] & 0x0f);

            int position = 1;
            this.ParamCount = (int)this.SignatureBlob.ReadCompressedUsigned(ref position);

            this.TypeDescriptor = this.SignatureBlob.ReadSignatureBlobType(reader, ref position);
        }
    }
}
