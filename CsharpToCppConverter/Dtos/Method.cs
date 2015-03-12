namespace Converters.Dtos
{
    using System;

    using Converters.ComInterfaces;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Metadata;

    public class Method : IFullName
    {
        public int Token { get; set; }

        public string FullName { get; set; }

        public CorMethodAttr Flags { get; set; }

        public byte[] SignatureBlob { get; set; }

        public int CodeRva { get; set; }

        public CorMethodImpl ImplementationFlags { get; set; }

        public Param[] Params { get; set; }

        public CorCallingConvention CallingConvention { get; private set; }

        /// <summary>
        /// Param count from Signature
        /// </summary>
        public int ParamCount { get; private set; }

        public TypeDescriptor ReturnTypeDescriptor { get; private set; }

        // todo: finish reading parameters types from Signature
        public void ReadSignature(MetadataReader reader)
        {
            if (this.SignatureBlob == null || this.SignatureBlob.Length == 0)
            {
                throw new ArgumentException("SignatureBlob is empty");
            }

            this.CallingConvention = (CorCallingConvention)(this.SignatureBlob[0] & 0x0f);

            int position = 1;
            this.ParamCount = (int)this.SignatureBlob.ReadCompressedUsigned(ref position);

            this.ReturnTypeDescriptor = this.SignatureBlob.ReadSignatureBlobType(reader, ref position);
        }
    }
}
