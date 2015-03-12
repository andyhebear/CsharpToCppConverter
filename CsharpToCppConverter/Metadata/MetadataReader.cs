// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataReader.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the MetadataReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Converters.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Converters.ComInterfaces;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Dictionaries;
    using Converters.Dtos;

    public class MetadataReader
    {
        #region Fields

        private static readonly IDictionary<string, string> mapOfTypes = new SortedDictionary<string, string>();

        private readonly IDictionary<int, object> metadataObjectsCache = new SortedDictionary<int, object>();

        private IMetaDataImport import;

        #endregion

        #region Constructors and Destructors

        static MetadataReader()
        {
        }

        public MetadataReader(string winmdFilePath)
        {
            this.InitializeMetadataInterface(winmdFilePath);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Enumerates EventDef tokens representing Events of the specified type.
        /// 
        /// To enumerate inherited Events, the caller must explicitly walk the inheritance chain. 
        /// </summary>
        /// <param name="typeDefinition"> A ReturnTypeDefinition representing the type whose Events are to be enumerated</param>
        /// <returns>the enumerator</returns>
        public IEnumerable<Event> EnumerateEvents(TypeDefinition typeDefinition)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Events at once which will be stored in this array. 
            var events = new int[10];

            // Number of read Events. 
            var count = 0;
            var hresult = this.import.EnumEvents(ref enumHandle, typeDefinition.Token, events, events.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading Events' while the Events array contains any new Event. 
            while (count > 0)
            {
                for (uint eventsIndex = 0; eventsIndex < count; eventsIndex++)
                {
                    yield return this.GetEventProperties(events[eventsIndex]);
                }

                hresult = this.import.EnumEvents(ref enumHandle, typeDefinition.Token, events, events.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        /// <summary>
        /// Enumerates Field tokens representing Fields of the specified type.
        /// 
        /// To enumerate inherited Fields, the caller must explicitly walk the inheritance chain. 
        /// </summary>
        /// <param name="typeDefinition"> A ReturnTypeDefinition representing the type whose Fields are to be enumerated</param>
        /// <returns>the enumerator</returns>
        public IEnumerable<Field> EnumerateFields(TypeDefinition typeDefinition)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Fields at once which will be stored in this array. 
            var fields = new int[10];

            // Number of read Fields. 
            var count = 0;
            var hresult = this.import.EnumFields(ref enumHandle, typeDefinition.Token, fields, fields.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading Fields' while the Fields array contains any new Field. 
            while (count > 0)
            {
                for (uint fieldsIndex = 0; fieldsIndex < count; fieldsIndex++)
                {
                    yield return this.GetFieldProperties(fields[fieldsIndex]);
                }

                hresult = this.import.EnumFields(ref enumHandle, typeDefinition.Token, fields, fields.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        public IEnumerable<object> EnumerateInterfaceImplementations(int typeDefToken)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 TypeDefs at once which will be stored in this array. 
            var interfaceImpls = new int[10];

            // Number of read TypeDefs. 
            var count = 0;
            var hresult = this.import.EnumInterfaceImpls(ref enumHandle, typeDefToken, interfaceImpls, interfaceImpls.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading InterfaceImpl's while he InterfaceImpls array contains any new InterfaceImpl. 
            while (count > 0)
            {
                for (uint interfaceImplsIndex = 0; interfaceImplsIndex < count; interfaceImplsIndex++)
                {
                    yield return this.GetInterfaceImplementationProperties(interfaceImpls[interfaceImplsIndex]);
                }

                hresult = this.import.EnumInterfaceImpls(ref enumHandle, typeDefToken, interfaceImpls, interfaceImpls.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        /// <summary>
        /// Enumerates MemberDef tokens representing members of the specified type.
        /// 
        /// To enumerate inherited members, the caller must explicitly walk the inheritance chain. 
        /// </summary>
        /// <param name="typeDefinition"> A ReturnTypeDefinition representing the type whose members are to be enumerated</param>
        /// <returns>the enumerator</returns>
        public IEnumerable<Member> EnumerateMembers(TypeDefinition typeDefinition)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Members at once which will be stored in this array. 
            var members = new int[10];

            // Number of read Members. 
            var count = 0;
            var hresult = this.import.EnumMembers(ref enumHandle, typeDefinition.Token, members, members.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading Members' while the members array contains any new Member. 
            while (count > 0)
            {
                for (uint membersIndex = 0; membersIndex < count; membersIndex++)
                {
                    yield return this.GetMemberProperties(members[membersIndex]);
                }

                hresult = this.import.EnumMembers(ref enumHandle, typeDefinition.Token, members, members.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        /// <summary>
        /// Enumerates Method tokens representing Methods of the specified type.
        /// 
        /// To enumerate inherited Methods, the caller must explicitly walk the inheritance chain. 
        /// </summary>
        /// <param name="typeDefinition"> A ReturnTypeDefinition representing the type whose Methods are to be enumerated</param>
        /// <returns>the enumerator</returns>
        public IEnumerable<Method> EnumerateMethods(TypeDefinition typeDefinition)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Methods at once which will be stored in this array. 
            var methods = new int[10];

            // Number of read Methods. 
            var count = 0;
            var hresult = this.import.EnumMethods(ref enumHandle, typeDefinition.Token, methods, methods.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading Methods' while the Methods array contains any new Method. 
            while (count > 0)
            {
                for (uint methodsIndex = 0; methodsIndex < count; methodsIndex++)
                {
                    yield return this.GetMethodProperties(methods[methodsIndex]);
                }

                hresult = this.import.EnumMethods(ref enumHandle, typeDefinition.Token, methods, methods.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        [Obsolete(".winmd seems using only AsseblyRef, not ModuleRef")]
        public IEnumerable<ModuleReference> EnumerateModuleReferences()
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 TypeDefs at once which will be stored in this array. 
            var moduleRefs = new int[10];

            // Number of read ModuleReferences. 
            var count = 0;
            var hresult = this.import.EnumModuleRefs(ref enumHandle, moduleRefs, moduleRefs.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading TypeDef's while the typeDefs array contains any new TypeDef. 
            while (count > 0)
            {
                for (uint moduleRefsIndex = 0; moduleRefsIndex < count; moduleRefsIndex++)
                {
                    yield return this.GetModuleReferenceProperties(moduleRefs[moduleRefsIndex]);
                }

                hresult = this.import.EnumModuleRefs(ref enumHandle, moduleRefs, moduleRefs.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        /// <summary>
        /// Enumerates ParamDef tokens representing Params of the specified type.
        /// 
        /// To enumerate inherited Params, the caller must explicitly walk the inheritance chain. 
        /// </summary>
        /// <param name="method"> A ReturnTypeDefinition representing the type whose Params are to be enumerated</param>
        /// <returns>the enumerator</returns>
        public IEnumerable<Param> EnumerateParams(Method method)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Params at once which will be stored in this array. 
            var @params = new int[10];

            // Number of read Params. 
            var count = 0;
            var hresult = this.import.EnumParams(ref enumHandle, method.Token, @params, @params.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading Params' while the Params array contains any new Param. 
            while (count > 0)
            {
                for (uint paramsIndex = 0; paramsIndex < count; paramsIndex++)
                {
                    yield return this.GetParamProperties(@params[paramsIndex]);
                }

                hresult = this.import.EnumParams(ref enumHandle, method.Token, @params, @params.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        /// <summary>
        /// Enumerates PropertyDef tokens representing Properties of the specified type.
        /// 
        /// To enumerate inherited Properties, the caller must explicitly walk the inheritance chain. 
        /// </summary>
        /// <param name="typeDefinition"> A ReturnTypeDefinition representing the type whose Properties are to be enumerated</param>
        /// <returns>the enumerator</returns>
        public IEnumerable<Property> EnumerateProperties(TypeDefinition typeDefinition)
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Properties at once which will be stored in this array. 
            var properties = new int[10];

            // Number of read Properties. 
            var count = 0;
            var hresult = this.import.EnumProperties(ref enumHandle, typeDefinition.Token, properties, properties.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading Properties' while the Properties array contains any new Property. 
            while (count > 0)
            {
                for (uint propertiesIndex = 0; propertiesIndex < count; propertiesIndex++)
                {
                    yield return this.GetPropertyProperties(properties[propertiesIndex]);
                }

                hresult = this.import.EnumProperties(ref enumHandle, typeDefinition.Token, properties, properties.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        public IEnumerable<TypeDefinition> EnumerateTypeDefinitions()
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 TypeDefs at once which will be stored in this array. 
            var typeDefs = new int[10];

            // Number of read TypeDefs. 
            var count = 0;
            var hresult = this.import.EnumTypeDefs(ref enumHandle, typeDefs, typeDefs.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading TypeDef's while he typeDefs array contains any new TypeDef. 
            while (count > 0)
            {
                for (uint typeDefsIndex = 0; typeDefsIndex < count; typeDefsIndex++)
                {
                    yield return this.GetTypeDefinitionProperties(typeDefs[typeDefsIndex]);
                }

                hresult = this.import.EnumTypeDefs(ref enumHandle, typeDefs, typeDefs.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        public IEnumerable<TypeReference> EnumerateTypeReferences()
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 TypeDefs at once which will be stored in this array. 
            var typeRefs = new int[10];

            // Number of read TypeRefs. 
            var count = 0;
            var hresult = this.import.EnumTypeRefs(ref enumHandle, typeRefs, typeRefs.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading TypeRef's while the typeRefs array contains any new TypeRef. 
            while (count > 0)
            {
                for (uint typeRefsIndex = 0; typeRefsIndex < count; typeRefsIndex++)
                {
                    yield return this.GetTypeReferenceProperties(typeRefs[typeRefsIndex]);
                }

                hresult = this.import.EnumTypeRefs(ref enumHandle, typeRefs, typeRefs.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        public IEnumerable<TypeSpecification> EnumerateTypeSpecifications()
        {
            // Handle of the enumeration. 
            var enumHandle = 0;

            // We will read maximum 10 Events at once which will be stored in this array. 
            var typeSpecs = new int[10];

            // Number of read Events. 
            var count = 0;
            var hresult = this.import.EnumTypeSpecs(ref enumHandle, typeSpecs, typeSpecs.Length, ref count);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // Continue reading TypeSpec' while the TypeSpecs array contains any new TypeSpec. 
            while (count > 0)
            {
                for (uint eventsIndex = 0; eventsIndex < count; eventsIndex++)
                {
                    yield return this.GetTypeSpecificationProperties(typeSpecs[eventsIndex]);
                }

                hresult = this.import.EnumTypeSpecs(ref enumHandle, typeSpecs, typeSpecs.Length, ref count);
                if (hresult != 0)
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }
            }

            this.import.CloseEnum(enumHandle);
        }

        public Event GetEventProperties(int token)
        {
            // The Event's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var eventName = new char[1024];

            var typeDefToken = 0;

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // Event's flags. 
            var eventFlags = 0;

            // A pointer to a TypeRef or TypeDef metadata token representing the Delegate type of the event.
            var eventTypeToken = 0;

            // method token AddOn
            var methodTokenAddOn = 0;

            // method token RemoveOn
            var methodTokenRemoveOn = 0;

            // method token Fire
            var methodTokenFire = 0;

            var otherMethodTokens = new int[1024];

            var otherMethodTokensCount = 0;

            // Get the Event's properties. 
            var hresult = this.import.GetEventProps(
                token, 
                ref typeDefToken, 
                eventName, 
                eventName.Length, 
                ref nameLength, 
                ref eventFlags, 
                ref eventTypeToken, 
                ref methodTokenAddOn, 
                ref methodTokenRemoveOn, 
                ref methodTokenFire, 
                otherMethodTokens, 
                1024, 
                ref otherMethodTokensCount);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the Event's name. 
            var fullTypeName = new string(eventName, 0, nameLength - 1);

            var otherMethods = new Method[otherMethodTokensCount];
            for (var otherMethodTokenIndex = 0; otherMethodTokenIndex < otherMethodTokensCount; otherMethodTokenIndex++)
            {
                otherMethods[otherMethodTokenIndex] = this.GetMethodProperties(otherMethodTokens[otherMethodTokenIndex]);
            }

            var eventProperties = new Event
                {
                    Token = token, 
                    FullName = fullTypeName, 
                    Flags = (CorEventAttr)eventFlags, 
                    Type =
                        eventTypeToken.Is(CorTokenType.TypeDef)
                            ? (object)this.GetTypeDefinitionProperties(eventTypeToken)
                            : (object)this.GetTypeReferenceProperties(eventTypeToken), 
                    AddOn = this.GetMethodProperties(methodTokenAddOn), 
                    RemoveOn = this.GetMethodProperties(methodTokenRemoveOn), 
                    Fire = methodTokenFire.IsNotEmpty(CorTokenType.MethodDef) ? this.GetMethodProperties(methodTokenFire) : null, 
                    Other = otherMethods
                };

            return eventProperties;
        }

        public Field GetFieldProperties(int token)
        {
            // The Field's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var fieldName = new char[1024];

            var typeDefToken = 0;

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // Field's flags. 
            var attr = 0;

            // A pointer to the binary metadata signature of the Field.
            var sigBlob = new IntPtr(0);

            var sigBlobLength = 0;

            // A pointer to the relative virtual address of the Field.
            var codeRva = 0;

            // A constant string value returned by this member.
            var value = new IntPtr(0);

            var valueLength = 0;

            // Get the Field's properties. 
            var hresult = this.import.GetFieldProps(
                token, 
                ref typeDefToken, 
                fieldName, 
                fieldName.Length, 
                ref nameLength, 
                ref attr, 
                ref sigBlob, 
                ref sigBlobLength, 
                ref codeRva, 
                ref value, 
                ref valueLength);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the Field's name. 
            var fullTypeName = new string(fieldName, 0, nameLength - 1);

            var sigBlobBytes = new byte[sigBlobLength];
            for (var byteIndex = 0; byteIndex < sigBlobLength; byteIndex++)
            {
                sigBlobBytes[byteIndex] = Marshal.ReadByte(sigBlob, byteIndex);
            }

            var valueBytes = new byte[valueLength];
            for (var byteIndex = 0; byteIndex < valueLength; byteIndex++)
            {
                valueBytes[byteIndex] = Marshal.ReadByte(value, byteIndex);
            }

            var fieldProperties = new Field
                {
                    Token = token, 
                    FullName = fullTypeName, 
                    Flags = (CorFieldAttr)attr, 
                    SigBlob = sigBlobBytes, 
                    CodeRva = codeRva, 
                    Value = valueBytes
                };

            return fieldProperties;
        }

        public object GetInterfaceImplementationProperties(int token)
        {
            // The metadata token representing the class that implements the method.
            var typeToken = 0;

            // The metadata token representing the interface that defines the implemented method.
            // pointing to TypeSpec
            var interfaceToken = 0;

            // Get the InterfaceImpl's properties. 
            var hresult = this.import.GetInterfaceImplProps(token, ref typeToken, ref interfaceToken);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            if (interfaceToken.Is(CorTokenType.TypeDef))
            {
                return this.GetTypeDefinitionProperties(interfaceToken);
            }
            
            if (interfaceToken.Is(CorTokenType.TypeRef))
            {
                var typeRef = this.GetTypeReferenceProperties(interfaceToken);
                return (object) this.GetTypeDefByTypeRef(typeRef) ?? typeRef;
            }

            if (interfaceToken.Is(CorTokenType.TypeSpec))
            {
                return this.GetTypeSpecificationProperties(interfaceToken);
            }

            throw new NotImplementedException();
        }

        public Member GetMemberProperties(int token)
        {
            // The Member's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var memberName = new char[1024];

            var typeDefToken = 0;

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // Member's flags. 
            var memberFlags = 0;

            // A pointer to the binary metadata signature of the member.
            var sigBlob = new IntPtr(0);

            var sigBlobLength = 0;

            // A pointer to the relative virtual address of the member.
            var codeRva = 0;

            var methodImplementationFlags = 0;

            // A flag that marks a ValueType
            var cplusTypeFlag = 0;

            // A constant string value returned by this member.
            var value = new IntPtr(0);

            var valueLength = 0;

            // Get the Member's properties. 
            var hresult = this.import.GetMemberProps(
                token, 
                ref typeDefToken, 
                memberName, 
                memberName.Length, 
                ref nameLength, 
                ref memberFlags, 
                ref sigBlob, 
                ref sigBlobLength, 
                ref codeRva, 
                ref methodImplementationFlags, 
                ref cplusTypeFlag, 
                ref value, 
                ref valueLength);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the Member's name. 
            var fullTypeName = new string(memberName, 0, nameLength - 1);

            var sigBlobBytes = new byte[sigBlobLength];
            for (var byteIndex = 0; byteIndex < sigBlobLength; byteIndex++)
            {
                sigBlobBytes[byteIndex] = Marshal.ReadByte(sigBlob, byteIndex);
            }

            var valueBytes = new byte[valueLength];
            for (var byteIndex = 0; byteIndex < valueLength; byteIndex++)
            {
                valueBytes[byteIndex] = Marshal.ReadByte(value, byteIndex);
            }

            var memberProperties = new Member
                {
                    Token = token, 
                    FullName = fullTypeName, 
                    Flags = memberFlags, 
                    SignatureBlob = sigBlobBytes, 
                    CodeRva = codeRva, 
                    ImplementationFlags = (CorMethodImpl)methodImplementationFlags, 
                    CPlusTypeFlag = (CorElementType)cplusTypeFlag, 
                    Value = valueBytes
                };

            this.metadataObjectsCache[token] = memberProperties;

            return memberProperties;
        }

        public Method GetMethodProperties(int token)
        {
            object metadataObject = null;
            if (this.metadataObjectsCache.TryGetValue(token, out metadataObject))
            {
                return (Method)metadataObject;
            }

            // The Method's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var methodName = new char[1024];

            var typeDefToken = 0;

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // Method's flags. 
            var attr = 0;

            // A pointer to the binary metadata signature of the Method.
            var sigBlob = new IntPtr(0);

            var sigBlobLength = 0;

            // A pointer to the relative virtual address of the Method.
            var codeRva = 0;

            var methodImplementationFlags = 0;

            // Get the Method's properties. 
            var hresult = this.import.GetMethodProps(
                token, 
                ref typeDefToken, 
                methodName, 
                methodName.Length, 
                ref nameLength, 
                ref attr, 
                ref sigBlob, 
                ref sigBlobLength, 
                ref codeRva, 
                ref methodImplementationFlags);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the Method's name. 
            var fullTypeName = new string(methodName, 0, nameLength - 1);

            var sigBlobBytes = new byte[sigBlobLength];
            for (var byteIndex = 0; byteIndex < sigBlobLength; byteIndex++)
            {
                sigBlobBytes[byteIndex] = Marshal.ReadByte(sigBlob, byteIndex);
            }

            var methodProperties = new Method
                {
                    Token = token, 
                    FullName = fullTypeName, 
                    Flags = (CorMethodAttr)attr, 
                    SignatureBlob = sigBlobBytes, 
                    CodeRva = codeRva, 
                    ImplementationFlags = (CorMethodImpl)methodImplementationFlags
                };

            // read method params
            methodProperties.Params = this.EnumerateParams(methodProperties).ToArray();

            methodProperties.ReadSignature(this);

            return methodProperties;
        }

        public ModuleReference GetModuleReferenceProperties(int token)
        {
            // The ModuleRef's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var moduleName = new char[1024];

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // Get A pointer to the scope in which the reference is made. This value is an AssemblyRef or ModuleRef token.
            var hresult = this.import.GetModuleRefProps(token, moduleName, moduleName.Length, ref nameLength);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the TypeRef's name. 
            var fullModuleName = new string(moduleName, 0, nameLength - 1);

            var moduleRefProp = new ModuleReference() { Token = token, FullName = fullModuleName };

            return moduleRefProp;
        }

        public Param GetParamProperties(int token)
        {
            // The Param's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var paramName = new char[1024];

            var methodDefToken = 0;

            // Param's sequence. 
            var sequence = 0;

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // Param's flags. 
            var attr = 0;

            var cplusTypeFlag = 0;

            // A constant string value returned by this member.
            var value = new IntPtr(0);

            var valueLength = 0;

            // Get the Param's properties. 
            var hresult = this.import.GetParamProps(
                token, 
                ref methodDefToken, 
                ref sequence, 
                paramName, 
                paramName.Length, 
                ref nameLength, 
                ref attr, 
                ref cplusTypeFlag, 
                ref value, 
                ref valueLength);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the Param's name. 
            var fullTypeName = new string(paramName, 0, nameLength - 1);

            var valueBytes = new byte[valueLength];
            for (var byteIndex = 0; byteIndex < valueLength; byteIndex++)
            {
                valueBytes[byteIndex] = Marshal.ReadByte(value, byteIndex);
            }

            var paramProperties = new Param
                {
                    Token = token, 
                    Sequence = sequence, 
                    FullName = fullTypeName, 
                    Flags = (CorParamAttr)attr, 
                    CPlusTypeFlag = (CorElementType)cplusTypeFlag, 
                    Value = valueBytes
                };

            return paramProperties;
        }

        public Property GetPropertyProperties(int token)
        {
            // The Property's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var propertyName = new char[1024];

            var typeDefToken = 0;

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // A pointer to the binary metadata signature of the property.
            var sigBlob = new IntPtr(0);

            var sigBlobLength = 0;

            // Property's flags. 
            var propertyFlags = 0;

            // A flag that marks a ValueType
            var cplusTypeFlag = 0;

            // A constant string value returned by this member.
            var defaultValue = new IntPtr(0);

            var defaultValueLength = 0;

            // method token AddOn
            var methodTokenGetter = 0;

            // method token RemoveOn
            var methodTokenSetter = 0;

            var otherMethodTokens = new int[1024];

            var otherMethodTokensCount = 0;

            // Get the Property's properties. 
            var hresult = this.import.GetPropertyProps(
                token, 
                ref typeDefToken, 
                propertyName, 
                propertyName.Length, 
                ref nameLength, 
                ref propertyFlags, 
                ref sigBlob, 
                ref sigBlobLength, 
                ref cplusTypeFlag, 
                ref defaultValue, 
                ref defaultValueLength, 
                ref methodTokenSetter, 
                ref methodTokenGetter, 
                otherMethodTokens, 
                1024, 
                ref otherMethodTokensCount);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the Property's name. 
            var fullTypeName = new string(propertyName, 0, nameLength - 1);

            var sigBlobBytes = new byte[sigBlobLength];
            for (var byteIndex = 0; byteIndex < sigBlobLength; byteIndex++)
            {
                sigBlobBytes[byteIndex] = Marshal.ReadByte(sigBlob, byteIndex);
            }

            var defaultValueBytes = new byte[defaultValueLength];
            for (var byteIndex = 0; byteIndex < defaultValueLength; byteIndex++)
            {
                defaultValueBytes[byteIndex] = Marshal.ReadByte(defaultValue, byteIndex);
            }

            var otherMethods = new Method[otherMethodTokensCount];
            for (var otherMethodTokenIndex = 0; otherMethodTokenIndex < otherMethodTokensCount; otherMethodTokenIndex++)
            {
                otherMethods[otherMethodTokenIndex] = this.GetMethodProperties(otherMethodTokens[otherMethodTokenIndex]);
            }

            var propertyProperties = new Property
                {
                    Token = token, 
                    FullName = fullTypeName, 
                    Flags = (CorPropertyAttr)propertyFlags, 
                    CPlusTypeFlag = (CorElementType)cplusTypeFlag, 
                    DefaultValue = defaultValueBytes, 
                    SignatureBlob = sigBlobBytes, 
                    Getter = this.GetMethodProperties(methodTokenGetter), 
                    Setter = methodTokenSetter.IsNotEmpty(CorTokenType.MethodDef) ? this.GetMethodProperties(methodTokenSetter) : null, 
                    Other = otherMethods
                };

            propertyProperties.ReadSignature(this);

            return propertyProperties;
        }

        public TypeDefinition GetTypeDefByTypeRef(TypeReference typeRef)
        {
            // todo: if you can't find TypeDef by typeRef it means you need to iterate all modules (EnumModules) and look there.
            var mappedCppType = string.Empty;
            if (!CsTypesToCppTypes.Map.TryGetValue(typeRef.FullName, out mappedCppType))
            {
                mappedCppType = typeRef.FullName;
            }

            if (!mappedCppType.StartsWith("System."))
            {
                var typeDefTokenForTypeRef = 0;
                this.import.FindTypeDefByName(mappedCppType, 0, ref typeDefTokenForTypeRef);
                if (typeDefTokenForTypeRef != 0)
                {
                    return this.GetTypeDefinitionProperties(typeDefTokenForTypeRef);
                }
            }

            return null;
        }

        public TypeDefinition GetTypeDefinitionProperties(int token)
        {
            object metadataObject = null;
            if (this.metadataObjectsCache.TryGetValue(token, out metadataObject))
            {
                return (TypeDefinition)metadataObject;
            }

            // The TypeDef's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var typeName = new char[1024];

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // TypeDef's flags. 
            var typeDefFlags = 0;

            // If the TypeDef is a derived type then the base type's token. 
            var baseTypeToken = 0;

            // Get the TypeDef's properties. 
            var hresult = this.import.GetTypeDefProps(token, typeName, typeName.Length, ref nameLength, ref typeDefFlags, ref baseTypeToken);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the TypeDef's name. 
            var fullTypeName = new string(typeName, 0, nameLength - 1);
            var corTypeAttr = (CorTypeAttr)typeDefFlags;

            var typeDefProp = new TypeDefinition() { Token = token, FullName = fullTypeName, Type = corTypeAttr };

            this.metadataObjectsCache[token] = typeDefProp;

            if (baseTypeToken > 0)
            {
                if (baseTypeToken.Is(CorTokenType.TypeDef))
                {
                    typeDefProp.Base = this.GetTypeDefinitionProperties(baseTypeToken);
                }
                else if (baseTypeToken.Is(CorTokenType.TypeRef))
                {
                    var typeRef = this.GetTypeReferenceProperties(baseTypeToken);
                    typeDefProp.BaseReference = typeRef;
                    typeDefProp.Base = this.GetTypeDefByTypeRef(typeRef);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            typeDefProp.Interfaces = this.EnumerateInterfaceImplementations(token).ToArray();

            return typeDefProp;
        }

        public TypeReference GetTypeReferenceProperties(int token)
        {
            object metadataObject = null;
            if (this.metadataObjectsCache.TryGetValue(token, out metadataObject))
            {
                return (TypeReference)metadataObject;
            }

            // The TypeRef's name will be stored in this array. The 1024 is a "magical number", seems like a type's name can be maximum this long. The corhlpr.h also defines a suspicious constant like this: #define MAX_CLASSNAME_LENGTH 1024 
            var typeName = new char[1024];

            // Number of how many characters were filled in the typeName array. 
            var nameLength = 0;

            // TypeRef's flags. 
            var resolutionScope = 0;

            // Get A pointer to the scope in which the reference is made. This value is an AssemblyRef or ModuleRef token.
            var hresult = this.import.GetTypeRefProps(token, ref resolutionScope, typeName, typeName.Length, ref nameLength);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // supress names "" & "\0";
            if (nameLength <= 1)
            {
                // return null for this, we do not need to know about empty base
                return null;
            }

            // Get the TypeRef's name. 
            var fullTypeName = new string(typeName, 0, nameLength - 1);

            var typeRefProp = new TypeReference() { Token = token, FullName = fullTypeName };

            this.metadataObjectsCache[token] = typeRefProp;

            // seems it expects only AssemblyRef, not ModuleRef
            // typeRefProp.Module = this.GetModuleReferenceProperties(resolutionScope);
            return typeRefProp;
        }

        #endregion

        #region Methods

        private TypeSpecification GetTypeSpecificationProperties(int typeSpec)
        {
            var sigBlob = new IntPtr(0);

            var sigBlobLength = 0;

            // Get the Property's properties. 
            var hresult = this.import.GetTypeSpecFromToken(typeSpec, ref sigBlob, ref sigBlobLength);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            var sigBlobBytes = new byte[sigBlobLength];
            for (var byteIndex = 0; byteIndex < sigBlobLength; byteIndex++)
            {
                sigBlobBytes[byteIndex] = Marshal.ReadByte(sigBlob, byteIndex);
            }

            var typeSpecProperties = new TypeSpecification { Token = typeSpec, SignatureBlob = sigBlobBytes, };

            typeSpecProperties.ReadSignature(this);

            return typeSpecProperties;
        }

        private void InitializeMetadataInterface(string winmdFilePath)
        {
            var dispenser = new MetaDataDispenserEx();

            // GUID of the IMetaDataImport interface. 
            var metaDataImportGuid = new Guid(Guids.IMetaDataImport);

            // Open the assembly. 
            object rawScope = null;
            var hresult = dispenser.OpenScope(winmdFilePath, 0, ref metaDataImportGuid, ref rawScope);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            // The rawScope contains an IMetaDataImport interface. 
            this.import = (IMetaDataImport)rawScope;
        }

        #endregion
    }
}