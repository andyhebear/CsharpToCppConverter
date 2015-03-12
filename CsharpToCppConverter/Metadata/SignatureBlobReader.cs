namespace Converters.Metadata
{
    using System;
    using System.Collections.Generic;

    using Converters.ComInterfaces;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Dtos;

    public static class SignatureBlobReader
    {
        public static IEnumerator<ulong> DecodeBlobAsUnsigned(this byte[] signatureBlob, int startPosition = 0)
        {
            for (var position = startPosition; position < signatureBlob.Length; )
            {
                yield return signatureBlob.ReadCompressedUsigned(ref position);
            }
        }

        public static ulong ReadCompressedUsigned(this byte[] signatureBlob, ref int position)
        {
            var @byte = signatureBlob[position];

            if ((@byte & 0x80) == 0)
            {
                position++;
                return @byte;
            }

            if ((@byte & 0xc0) == 0x80)
            {
                var val = (@byte & ~0xc0) << 8;
                val += signatureBlob[++position];

                position++;
                return (ulong)val;
            }

            if ((@byte & 0xe0) == 0xc0)
            {
                var val = (@byte & ~0xe0) << 8;
                val += signatureBlob[++position];
                val <<= 8;
                val += signatureBlob[++position];
                val <<= 8;
                val += signatureBlob[++position];

                position++;
                return (ulong)val;
            }

            throw new Exception("Could not decode comressed values");
        }

        public static int DecodeTypeDefOrRefOrSpecEncoded(this int encoded)
        {
            return ((encoded & 0x3) << 24) + (encoded >> 2);
        }

        public static int DecodeTypeDefOrRefOrSpecLowTokenOnly(this int encoded)
        {
            return encoded >> 2;
        }

        /// <summary>
        /// The bit values to use are 0, 1 and 2, specifying the target table is the TypeDef, TypeRef or TypeSpec table, respectively 
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns>return 0 for TypeDef, 1 - TypeRef, 2 - TypeSpec</returns>
        public static int TypeDefOrRefOrSpec(this int encoded)
        {
            return encoded & 0x3;
        }

        public static TypeDescriptor ReadSignatureBlobType(this byte[] signatureBlob, MetadataReader reader, ref int position)
        {
            var type = (CorElementType)signatureBlob.ReadCompressedUsigned(ref position);
            if (type == CorElementType.ELEMENT_TYPE_CMOD_OPT || type == CorElementType.ELEMENT_TYPE_CMOD_REQD)
            {
                // does not support
                throw new NotImplementedException();
            }

            return signatureBlob.DecodeTypeAndTypeDefOrRefOrSpecEncoded(type, ref position, reader);
        }

        private static TypeDescriptor DecodeTypeAndTypeDefOrRefOrSpecEncoded(this byte[] signatureBlob, CorElementType type, ref int position, MetadataReader reader)
        {
            var typeDescriptor = new Dtos.TypeDescriptor { ElementType = type, GenericParamNumber = 0, GenericParametersCount = 0 };

            if (type == CorElementType.ELEMENT_TYPE_CLASS)
            {
                // read Token ReturnElementType Def / ReturnElementType Ref / ReturnElementType Spec
                typeDescriptor.TypeDefinition = ReadDecodeTypeDefOrRefOrSpecEncoded(signatureBlob, reader, ref position);
            }
            else if (type == CorElementType.ELEMENT_TYPE_VALUETYPE)
            {
                // read Token ReturnElementType Def / ReturnElementType Ref / ReturnElementType Spec
                typeDescriptor.TypeDefinition = ReadDecodeTypeDefOrRefOrSpecEncoded(signatureBlob, reader, ref position);
            }
            else if (type == CorElementType.ELEMENT_TYPE_GENERICINST)
            {
                var baseGeneticType = (CorElementType)signatureBlob.ReadCompressedUsigned(ref position);
                typeDescriptor = signatureBlob.DecodeTypeAndTypeDefOrRefOrSpecEncoded(baseGeneticType, ref position, reader);
                typeDescriptor.GenericParametersCount = signatureBlob.ReadCompressedUsigned(ref position);

                var genericTypes = new List<TypeDescriptor>();
                for (var i = 0; i < (int) typeDescriptor.GenericParametersCount; i++)
                {
                    var elementType = (CorElementType)signatureBlob.ReadCompressedUsigned(ref position);
                    var genericTypeDescriptor = signatureBlob.DecodeTypeAndTypeDefOrRefOrSpecEncoded(elementType, ref position, reader);
                    genericTypes.Add(genericTypeDescriptor);
                }

                typeDescriptor.GenericTypes = genericTypes;
            }
            else if (type == CorElementType.ELEMENT_TYPE_I
                || type == CorElementType.ELEMENT_TYPE_I1
                || type == CorElementType.ELEMENT_TYPE_I2
                || type == CorElementType.ELEMENT_TYPE_I4
                || type == CorElementType.ELEMENT_TYPE_I8
                || type == CorElementType.ELEMENT_TYPE_U
                || type == CorElementType.ELEMENT_TYPE_U1
                || type == CorElementType.ELEMENT_TYPE_U2
                || type == CorElementType.ELEMENT_TYPE_U4
                || type == CorElementType.ELEMENT_TYPE_U8
                || type == CorElementType.ELEMENT_TYPE_R4
                || type == CorElementType.ELEMENT_TYPE_R8
                || type == CorElementType.ELEMENT_TYPE_BOOLEAN
                || type == CorElementType.ELEMENT_TYPE_OBJECT
                || type == CorElementType.ELEMENT_TYPE_STRING
                || type == CorElementType.ELEMENT_TYPE_VOID)
            {
                // ignore it is value type
                // todo: do I need to read it from Plaform.winmd?
                // todo: what is ELEMENT_TYPE_VAR <number>?
            }
            else if (type == CorElementType.ELEMENT_TYPE_VAR ||
                type == CorElementType.ELEMENT_TYPE_MVAR)
            {
                typeDescriptor.GenericParamNumber = signatureBlob.ReadCompressedUsigned(ref position);
            }
            else
            {
                throw new NotImplementedException();
            }

            return typeDescriptor;
        }

        private static TypeDefinition ReadDecodeTypeDefOrRefOrSpecEncoded(
            this byte[] signatureBlob, MetadataReader reader, ref int position)
        {
            var encoded = (int)signatureBlob.ReadCompressedUsigned(ref position);
            var tokenTable = encoded.TypeDefOrRefOrSpec();
            var classTokenType = encoded.DecodeTypeDefOrRefOrSpecLowTokenOnly();
            
            switch (tokenTable)
            {
                case 0:
                    return reader.GetTypeDefinitionProperties(classTokenType | (int)CorTokenType.TypeDef);
                case 1:
                    return reader.GetTypeDefByTypeRef(reader.GetTypeReferenceProperties(classTokenType | (int)CorTokenType.TypeRef));
                case 2:
                    throw new NotImplementedException();
            }

            throw new IndexOutOfRangeException("tokenTable");
        }
    }
}
