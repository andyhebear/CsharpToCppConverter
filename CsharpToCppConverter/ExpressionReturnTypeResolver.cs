// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionReturnTypeResolver.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the ExpressionReturnTypeResolver type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Converters.Adapters;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Dtos;
    using Converters.Extentions;
    using Converters.Metadata;

    using StyleCop;
    using StyleCop.CSharp;

    using Field = StyleCop.CSharp.Field;
    using Method = StyleCop.CSharp.Method;
    using Property = StyleCop.CSharp.Property;

    public class ExpressionReturnTypeResolver
    {
        // todo: do not use @class, use it as context of INamesResolver
        #region Constructors and Destructors

        public ExpressionReturnTypeResolver(INamesResolver namesModel)
        {
            this.NamesModel = namesModel;
        }

        #endregion

        #region Public Properties

        public INamesResolver NamesModel { get; private set; }

        public bool IsMemberFound { get; private set; }

        #endregion

        #region Public Methods and Operators

        public IEnumerable<ResolvedContextItem> Resolve(Expression expression, IEnumerable<ResolvedContextItem> resolvedContext = null)
        {
            var methodInvocationExpression = expression as MethodInvocationExpression;
            if (methodInvocationExpression != null)
            {
                return this.Resolve(methodInvocationExpression.Name, resolvedContext);
            }

            var memberAccessExpression = expression as MemberAccessExpression;
            if (memberAccessExpression != null)
            {
                return this.Resolve(memberAccessExpression, resolvedContext);
            }

            var literalExpression = expression as LiteralExpression;
            if (literalExpression != null)
            {
                return this.Resolve(literalExpression, resolvedContext);
            }

            var asExpression = expression as AsExpression;
            if (asExpression != null)
            {
                return this.Pairs(this.TryResolveNameAsGlobalOrInUsingNamespaces(asExpression.Type).Get(), asExpression.Type, (ICodeElement)null);
            }

            var castExpression = expression as CastExpression;
            if (castExpression != null)
            {
                return this.Pairs(
                    this.TryResolveNameAsGlobalOrInUsingNamespaces(castExpression.Type).Get(),
                    FindGenericTypesParameters(castExpression.Type as GenericType));
            }

            var parenthesizedExpression = expression as ParenthesizedExpression;
            if (parenthesizedExpression != null)
            {
                return this.Resolve(parenthesizedExpression.InnerExpression, resolvedContext);
            }

            var arrayAccessExpression = expression as ArrayAccessExpression;
            if (arrayAccessExpression != null)
            {
                return this.Resolve(arrayAccessExpression.Array, resolvedContext);
            }

            var newExpression = expression as NewExpression;
            if (newExpression != null)
            {
                return this.Resolve(newExpression.TypeCreationExpression, resolvedContext);
            }

            throw new NotImplementedException();
        }

        public IEnumerable<ResolvedContextItem> Resolve(MethodInvocationExpression methodInvocationExpression, IEnumerable<ResolvedContextItem> resolvedContext = null)
        {
            return this.Resolve(methodInvocationExpression.Name, resolvedContext);
        }

        // todo: remove all ToList() after debugging
        public IEnumerable<ResolvedContextItem> Resolve(MemberAccessExpression memberAccessExpression, IEnumerable<ResolvedContextItem> resolvedContext = null)
        {
            if (memberAccessExpression.OperatorType == MemberAccessExpression.Operator.Dot)
            {
                var leftHandResolved = this.Resolve(memberAccessExpression.LeftHandSide, resolvedContext);
                return this.Resolve(memberAccessExpression.RightHandSide, leftHandResolved);
            }

            throw new NotImplementedException();
        }

        public IEnumerable<ResolvedContextItem> Resolve(LiteralExpression literalExpression, IEnumerable<ResolvedContextItem> resolvedContext = null)
        {
            var isThis = string.CompareOrdinal(literalExpression.Text, "this") == 0;
            if (isThis || string.CompareOrdinal(literalExpression.Text, "base") == 0)
            {
                return this.Pairs(this.ResolveThisOrBaseLiteralExpression(literalExpression, isThis), (TypeToken)null, (ICodeElement)null);
            }

            if (resolvedContext == null)
            {
                // you should not resolve types here, because name points to variable not to its type
                // but because it is variable you can return type to avoid resolving it later
                var foundTypesOfVarsOrParamsFromLocalScope =
                    this.SelectLocalVarsAndMethodParameters(literalExpression)
                        .Where(v => this.SelectVarByName(v, literalExpression.Text))
                        .SelectMany(this.SelectType).ToList();

                var @class = literalExpression.FindParent<Class>();

                //you can't resolve returning type here, because name points to method
                //but you can try to resolve method by params here
                var foundFoundReturnTypesOfMembersFromClassScope =
                    this.SelectClassHierarchy(literalExpression)
                        .SelectMany(classType => classType.ChildCodeElements)
                        .Where(member => member.MemberNameEqualsTo(literalExpression.Text) && this.MethodArgumentsTypesEqualToOrCurrent(member))
                        .SelectMany(m => this.FindReturnTypeOrCurrent(new ResolvedContextItem { Member = m }, @class));

                // here you can return type because name points to type
                var foundTypesOfReferencesFromGlobalNamespacesScope =
                    this.Pairs(this.TryResolveNameAsGlobalOrInUsingNamespaces(literalExpression).Get(), (TypeToken)null, (ICodeElement)null);

                return
                    foundTypesOfVarsOrParamsFromLocalScope.Union(
                        foundFoundReturnTypesOfMembersFromClassScope.Union(foundTypesOfReferencesFromGlobalNamespacesScope)).ToList();
            }

            return this.ResolvedByContext(literalExpression.Text, literalExpression.FindParent<Class>(), resolvedContext);
        }

        #endregion

        #region Methods

        private IEnumerable<GenericTypeParameter> FindGenericTypesParameters(ICodeElement codeElement)
        {
            return this.FindGenericTypesParameters(this.FindTokenType(codeElement) as GenericType);
        }

        private IEnumerable<GenericTypeParameter> FindGenericTypesParameters(GenericType genericType)
        {
            if (genericType != null)
            {
                return genericType.GenericTypesParameters;
            }

            return null;
        }

        private IList<TypeDescriptor> FindGenericTypeDescriptors(ICodeElement codeElement)
        {
            var typeDefAdapterBase = codeElement as TypeDefinitionMetadataICodeElementAdapterBase;
            if (typeDefAdapterBase != null)
            {
                return typeDefAdapterBase.GenericTypes;
            }

            return null;
        }

        private FullyQualifiedNamesCache.NamespaceNode FindNamespaceNodeByFullNamespace(
            string firstFullNamespacePart, string secondNamespacePart = null)
        {
            var namespaceNode = this.NamesModel.FullyQualifiedNames[firstFullNamespacePart];
            if (namespaceNode == null)
            {
                return null;
            }

            return secondNamespacePart != null ? namespaceNode[secondNamespacePart] : namespaceNode;
        }

        // todo: finish resolving generic types for metadata types
        private IEnumerable<ResolvedContextItem> FindReturnTypeOrCurrent(ResolvedContextItem resolvedContextItem, Class @class)
        {
            var codeElement = resolvedContextItem.Member ?? resolvedContextItem.Type;

            if (codeElement is Class)
            {
                return this.Pairs(codeElement, resolvedContextItem);
            }

            if (codeElement is TypeDefinitionMetadataICodeElementAdapter)
            {
                return this.Pairs(codeElement, resolvedContextItem);
            }

            var method = codeElement as Method;
            if (method != null)
            {
                var returnType = method.ReturnType;
                return this.Pairs(
                    this.TryResolveNameAsGlobalOrInUsingNamespaces(method.FindParent<Class>(), returnType.Text).Get(), returnType, (ICodeElement)method);
            }

            var methodMetadataICodeElementAdapter = codeElement as MethodMetadataICodeElementAdapter;
            if (methodMetadataICodeElementAdapter != null)
            {
                var returnTypeDescriptor = methodMetadataICodeElementAdapter.Method.ReturnTypeDescriptor;
                return this.FindTypeForIncludingGenetics(returnTypeDescriptor, methodMetadataICodeElementAdapter.GenericTypes, resolvedContextItem, @class);
            }

            var property = codeElement as Property;
            if (property != null)
            {
                this.IsMemberFound = true;
                var returnType = property.ReturnType;
                return this.FindTypeFor(returnType, @class, codeElement);
            }

            var field = codeElement as Field;
            if (field != null)
            {
                this.IsMemberFound = true;
                var fieldType = field.FieldType;
                return this.FindTypeFor(fieldType, @class, codeElement);
            }

            var propertyMetadata = codeElement as PropertyMetadataICodeElementAdapter;
            if (propertyMetadata != null)
            {
                var typeDescriptor = propertyMetadata.Property.TypeDescriptor;
                return this.FindTypeForIncludingGenetics(typeDescriptor, propertyMetadata.GenericTypes, resolvedContextItem, @class);
            }

            // return default empty result
            return this.Pairs(((FullyQualifiedNamesCache.NamespaceNode)null).Get(), (TypeToken)null, (ICodeElement)null);
        }

        private IEnumerable<ResolvedContextItem> FindTypeForIncludingGenetics(TypeDescriptor typeDescriptor, IEnumerable<TypeDescriptor> genericTypes, ResolvedContextItem resolvedContextItem, Class @class)
        {
            if (typeDescriptor.ElementType == CorElementType.ELEMENT_TYPE_VAR || typeDescriptor.ElementType == CorElementType.ELEMENT_TYPE_MVAR)
            {
                return this.ResolveGenericType(typeDescriptor, genericTypes, resolvedContextItem, @class);
            }

            return this.FindTypeFor(typeDescriptor, resolvedContextItem);
        }

        private IEnumerable<ResolvedContextItem> ResolveGenericType(TypeDescriptor typeDescriptor, IEnumerable<TypeDescriptor> genericTypes, ResolvedContextItem resolvedContextItem, Class @class)
        {
            // todo: finish reading GenericTypeNumber after ELEMENT_TYPE_VAR in signature
            // todo: finish it: if  genericTypesParameters is null, you have not finished something
            var genericParamNumber = typeDescriptor.GenericParamNumber;

            // processs metadata before (because GenericTypesParameters could contains TypeToken for nested generic types)
            if (resolvedContextItem.GenericTypeDescriptorParameters != null)
            {
                var genericTypeDescriptor = resolvedContextItem.GenericTypeDescriptorParameters.Skip((int)genericParamNumber).First();
                // switch to nested generics
                resolvedContextItem.GenericTypeDescriptorParameters = genericTypeDescriptor.GenericTypes;

                // if genericTypeDescriptor.ElementType is ELEMENT_TYPE_VAR then use GenericTypesParameters to resolve it, it should have declared types in code
                if (genericTypeDescriptor.ElementType != CorElementType.ELEMENT_TYPE_VAR && genericTypeDescriptor.ElementType != CorElementType.ELEMENT_TYPE_MVAR)
                {
                    return this.FindTypeFor(genericTypeDescriptor, resolvedContextItem);
                }
            }

            if (resolvedContextItem.GenericTypesParameters != null)
            {
                var currentGenericType = resolvedContextItem.GenericTypesParameters.Skip((int)genericParamNumber).First().Type;
                return this.FindTypeFor(currentGenericType, @class, (ICodeElement)null);
            }

            if (genericTypes != null)
            {
                var genericTypeDescriptor = genericTypes.Skip((int)genericParamNumber).First();
                return this.FindTypeFor(genericTypeDescriptor, resolvedContextItem);
            }

            // we didn't implement all possible ways to resolve generic params
            throw new NotImplementedException();
        }

        private IEnumerable<ResolvedContextItem> FindTypeFor(TypeToken typeToken, Class @class, ICodeElement codeElement)
        {
            return this.Pairs(this.TryResolveNameAsGlobalOrInUsingNamespaces(@class, typeToken.Text).Get(), typeToken, codeElement);
        }

        private IEnumerable<ResolvedContextItem> FindTypeFor(TypeDescriptor typeDescriptor, ResolvedContextItem resolvedContextItem)
        {
            var fullNameInterface = typeDescriptor.TypeDefinition as IFullName;
            if (fullNameInterface != null)
            {
                var fullyQualifiedName = fullNameInterface.FullName.GetRootedName();

                if (typeDescriptor.GenericTypes != null
                    && typeDescriptor.GenericTypes.Any(t => t.ElementType == CorElementType.ELEMENT_TYPE_VAR || t.ElementType == CorElementType.ELEMENT_TYPE_MVAR))
                {
                    // we need to join generics
                    return this.Pairs(this.NamesModel.FullyQualifiedNames[fullyQualifiedName].Get(), resolvedContextItem);
                }

                return this.Pairs(this.NamesModel.FullyQualifiedNames[fullyQualifiedName].Get(), typeDescriptor.GenericTypes);
            }

            // null value
            return this.Pairs(((FullyQualifiedNamesCache.NamespaceNode)null).Get(), (TypeToken)null, (ICodeElement)null);
        }

        private IEnumerable<ResolvedContextItem> Pairs(ICodeElement codeElement)
        {
            return new[] { new ResolvedContextItem { Type = codeElement, GenericTypesParameters = this.FindGenericTypesParameters(codeElement) } };
        }

        private IEnumerable<ResolvedContextItem> Pairs(ICodeElement codeElement, IEnumerable<GenericTypeParameter> genericTypesParameters)
        {
            return new[] { new ResolvedContextItem { Type = codeElement, GenericTypesParameters = genericTypesParameters } };
        }

        private IEnumerable<ResolvedContextItem> Pairs(IEnumerable<ICodeElement> codeElement, TypeToken typeToken, ICodeElement member)
        {
            var genericType = this.FindGenericTypesParameters(typeToken as GenericType);
            return codeElement.Select(ce => new ResolvedContextItem { Type = ce, Member = member, GenericTypesParameters = genericType }).ToList();
        }

        private IEnumerable<ResolvedContextItem> Pairs(IEnumerable<ICodeElement> codeElement, TypeToken typeToken, ICodePart code)
        {
            var genericType = this.FindGenericTypesParameters(typeToken as GenericType);
            return codeElement.Select(ce => new ResolvedContextItem { Type = ce, Code = code, GenericTypesParameters = genericType }).ToList();
        }

        private IEnumerable<ResolvedContextItem> Pairs(IEnumerable<ICodeElement> codeElement, IEnumerable<GenericTypeParameter> genericTypesParameters)
        {
            return codeElement.Select(ce => new ResolvedContextItem { Type = ce, GenericTypesParameters = genericTypesParameters }).ToList();
        }

        private IEnumerable<ResolvedContextItem> Pairs(IEnumerable<ICodeElement> codeElement, IList<TypeDescriptor> genericTypesParameters)
        {
            return codeElement.Select(ce => new ResolvedContextItem { Type = ce, GenericTypeDescriptorParameters = genericTypesParameters }).ToList();
        }

        private IEnumerable<ResolvedContextItem> Pairs(ICodeElement codeElement, ResolvedContextItem resolvedContextItem)
        {
            return new[]
                       {
                           new ResolvedContextItem
                               {
                                   Type = codeElement,
                                   GenericTypesParameters = resolvedContextItem.GenericTypesParameters,
                                   GenericTypeDescriptorParameters = resolvedContextItem.GenericTypeDescriptorParameters
                               }
                       };
        }

        private IEnumerable<ResolvedContextItem> Pairs(IEnumerable<ICodeElement> codeElement, ResolvedContextItem resolvedContextItem)
        {
            return
                codeElement.Select(
                    ce =>
                    new ResolvedContextItem
                        {
                            Type = ce,
                            GenericTypesParameters = resolvedContextItem.GenericTypesParameters,
                            GenericTypeDescriptorParameters = FindGenericTypeDescriptors(resolvedContextItem.Type) ?? resolvedContextItem.GenericTypeDescriptorParameters
                        })
                           .ToList();
        }

        // todo: finish it for Metadata types
        private TypeToken FindTokenType(ICodeElement codeElement)
        {
            var method = codeElement as Method;
            if (method != null)
            {
                return method.ReturnType;
            }

            var property = codeElement as Property;
            if (property != null)
            {
                return property.ReturnType;
            }

            var field = codeElement as Field;
            if (field != null)
            {
                return field.FieldType;
            }

            // return default empty result
            return null;
        }

        private IEnumerable<ICodeElement> GetMainAndPartialClassesOrDefault(Class @class)
        {
            var globalType = this.NamesModel.FullyQualifiedNames[@class.FullyQualifiedName];
            if (globalType != null)
            {
                return globalType.IterateCodeElements();
            }

            return new[] { @class };
        }

        private IEnumerable<ICodeElement> ResolveThisOrBaseLiteralExpression(ICodePart literalExpression, bool isThis)
        {
            var @class = literalExpression.FindParent<Class>();
            if (isThis)
            {
                return this.FindNamespaceNodeByFullNamespace(@class.FullNamespaceName).Get();
            }

            return this.TryResolveNameAsGlobalOrInUsingNamespaces(@class, @class.BaseClass).Get();
        }

        // todo: it seems this should use ResolveContext in and out
        private IEnumerable<ResolvedContextItem> ResolveTypeForVarKeyword(VariableDeclaratorExpression variableDeclaratorExpression)
        {
            var parentVariable = variableDeclaratorExpression.ParentVariable;

            if ("var".Equals(parentVariable.Type.Text))
            {
                if (variableDeclaratorExpression.Initializer != null)
                {
                    return this.Resolve(variableDeclaratorExpression.Initializer);
                }

                // resolve if var is in For-Each
                var forEachStatement = parentVariable.Parent as ForeachStatement;
                if (forEachStatement != null)
                {
                    var resolvedForEachCollection = this.Resolve(forEachStatement.Item);

                    // resolve GetAt() to resolve type
                    // todo: this is temporary hack
                    var parent = forEachStatement.FindParent<Class>();
                    var firstResolvedContext = this.ResolvedByContext("First", parent, resolvedForEachCollection);
                    return this.ResolvedByContext("Current", parent, firstResolvedContext);
                }
            }

            return null;
        }

        // todo: synchronize code to use FindReturnTypeOrCurrent only once
        private IEnumerable<ResolvedContextItem> ResolvedByContext(
            string literal, Class @class, IEnumerable<ResolvedContextItem> resolvedContext)
        {
            var foundMemberReturnTypesByName =
                resolvedContext.SelectMany(m => this.FindReturnTypeOrCurrent(m, @class))
                               .SelectMany(m => this.Pairs(this.SelectClassHierarchy(m.Type), m))
                               .SelectMany(classType => this.Pairs(classType.Type.ChildCodeElements, classType))
                               .Where(member => member.Type.MemberNameEqualsTo(literal)
                                                && this.MethodArgumentsTypesEqualToOrCurrent(member.Type))
                               .SelectMany(m => this.FindReturnTypeOrCurrent(m, @class));

            // resolve it from determined context
            return foundMemberReturnTypesByName.ToList();
        }

        private IEnumerable<ICodePart> SelectAllAncestorsRecurse<T>(ICodeUnit statement)
        {
            if (statement == null)
            {
                yield break;
            }

            var parent = statement.Parent as ICodeUnit;
            if (parent == null)
            {
                yield break;
            }

            foreach (var childStatement in parent.ChildStatements)
            {
                if (ReferenceEquals(parent, childStatement))
                {
                    break;
                }

                foreach (var codePart in childStatement.FindVarDeclarations())
                {
                    yield return codePart;
                }
            }

            if (parent is T)
            {
                yield break;
            }

            foreach (var varDecl in this.SelectAllAncestorsRecurse<T>(parent.Parent as ICodeUnit))
            {
                yield return varDecl;
            }
        }

        private IEnumerable<ICodeElement> SelectAllUsingDerectives(ICodeElement currentElement)
        {
            if (currentElement == null || currentElement.ChildCodeElements == null)
            {
                yield break;
            }

            foreach (var childElement in currentElement.ChildCodeElements)
            {
                if (childElement is UsingDirective)
                {
                    yield return childElement;
                }
                else if (childElement is Namespace)
                {
                    foreach (var usingDerective in this.SelectAllUsingDerectives(childElement))
                    {
                        yield return usingDerective;
                    }
                }
            }
        }

        private IEnumerable<ICodeElement> SelectBaseClassHierarchy(Class @class)
        {
            var current = @class;
            while (!string.IsNullOrWhiteSpace(current.BaseClass))
            {
                var codeElements = this.TryResolveNameAsGlobalOrInUsingNamespaces(current, current.BaseClass).Get().ToList();
                Debug.Assert(codeElements.Any(), string.Format("Could not resolve type: {0}", current.BaseClass));
                var codeElement = codeElements.First();

                var baseClass = codeElement as Class;
                if (baseClass != null)
                {
                    current = baseClass;
                    Debug.Assert(current != null);
                    yield return current;
                }
                else
                {
                    var typeDefinition = codeElement as TypeDefinitionMetadataICodeElementAdapter;
                    if (typeDefinition != null)
                    {
                        foreach (var typeDef in this.SelectClassHierarchy(typeDefinition))
                        {
                            yield return typeDef;
                        }
                    }

                    break;
                }
            }

            // return System.Object
            var platformObjectTypeDefinition =
                this.TryResolveNameAsGlobalOrInUsingNamespaces(current, "Platform.Object").Get().First() as
                TypeDefinitionMetadataICodeElementAdapter;

            yield return platformObjectTypeDefinition;
        }

        private IEnumerable<ICodeElement> SelectClassHierarchy(Expression expression)
        {
            var @class = expression.FindParent<Class>();
            if (@class == null)
            {
                yield break;
            }

            foreach (var classHierarchy in this.SelectClassHierarchy(@class))
            {
                yield return classHierarchy;
            }
        }

        private IEnumerable<ICodeElement> SelectClassHierarchy(ICodeElement codeElement)
        {
            var @class = codeElement as Class;
            if (@class != null)
            {
                foreach (var classHierarchy in this.SelectClassHierarchy(@class))
                {
                    yield return classHierarchy;
                }

                yield break;
            }

            var typeDefinition = codeElement as TypeDefinitionMetadataICodeElementAdapter;
            if (typeDefinition != null)
            {
                foreach (var classHierarchy in this.SelectClassHierarchy(typeDefinition))
                {
                    yield return classHierarchy;
                }

                yield break;
            }

            // or just return current element
            yield return codeElement;
        }

        private IEnumerable<ICodeElement> SelectClassHierarchy(TypeDefinitionMetadataICodeElementAdapter typeDef)
        {
            yield return typeDef;

            var metadataICodeElementAdapter = typeDef as MetadataICodeElementAdapter;
            if (metadataICodeElementAdapter != null && !metadataICodeElementAdapter.IsClassName)
            {
                yield break;
            }

            // return all current interfaces
            foreach (var @interface in typeDef.TypeDefinition.Interfaces)
            {
                var interfaceTypeDef = @interface as TypeDefinition;
                if (interfaceTypeDef != null)
                {
                    yield return new TypeDefinitionMetadataICodeElementAdapter(interfaceTypeDef, null, typeDef.Reader);
                }
                else
                {
                    var interfaceTypeSpec = @interface as TypeSpecification;
                    if (interfaceTypeSpec != null)
                    {
                        yield return new TypeDefinitionMetadataICodeElementAdapter(interfaceTypeSpec.TypeDescriptor, typeDef.Reader);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            var current = typeDef.TypeDefinition.Base;
            while (current != null)
            {
                yield return new TypeDefinitionMetadataICodeElementAdapter(current, null, typeDef.Reader);

                // return all base interfaces
                foreach (var @interface in current.Interfaces)
                {
                    var interfaceTypeDef = @interface as TypeDefinition;
                    if (interfaceTypeDef != null)
                    {
                        yield return new TypeDefinitionMetadataICodeElementAdapter(interfaceTypeDef, null, typeDef.Reader);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                current = current.Base;
            }
        }

        private IEnumerable<ICodeElement> SelectClassHierarchy(Class @class)
        {
            foreach (var mainOrPartialClass in
                from classElement in this.GetMainAndPartialClassesOrDefault(@class).OfType<Class>()
                where classElement != null
                select classElement)
            {
                yield return mainOrPartialClass;
                foreach (var baseClass in this.SelectBaseClassHierarchy(mainOrPartialClass))
                {
                    yield return baseClass;
                }
            }
        }

        private IEnumerable<ICodePart> SelectLocalVarsAndMethodParameters(Expression expression)
        {
            var allAncestorsRecurse = this.SelectAllAncestorsRecurse<IParameterContainer>(expression.FindParentStatement());
            var currentScope = expression.FindAllParentOfTypeOrDefault<IParameterContainer>();
            return currentScope.SelectMany(c => c.Parameters.Cast<ICodePart>()).Union(allAncestorsRecurse);
        }

        // do not remove it, it is reserved function
        private bool MethodArgumentsTypesEqualToOrCurrent(ICodeElement codeElement)
        {
            // todo: add checking parameters types, not parameter names
            var method = codeElement as Method;
            if (method != null)
            {
                // todo: finish code here to return false if method parameters not equal types of input params
                return true;
            }

            var methodMetadataICodeElementAdapter = codeElement as MethodMetadataICodeElementAdapter;
            if (methodMetadataICodeElementAdapter != null)
            {
                // todo: finish code here to return false if method parameters not equal types of input params
                return true;
            }

            // return true to avoid filtering types intead of methods
            return true;
        }

        private IEnumerable<ResolvedContextItem> SelectType(ICodePart codePart)
        {
            var parameter = codePart as Parameter;
            if (parameter != null)
            {
                var @class = parameter.FindParent<Class>();
                var typeToken = parameter.Type;
                return this.Pairs(this.TryResolveNameAsGlobalOrInUsingNamespaces(@class, typeToken.Text).Get(), typeToken, codePart);
            }

            var variableDeclaratorExpression = codePart as VariableDeclaratorExpression;
            if (variableDeclaratorExpression != null)
            {
                var selectType = this.ResolveTypeForVarKeyword(variableDeclaratorExpression);
                if (selectType != null)
                {
                    return selectType;
                }

                var @class = variableDeclaratorExpression.FindParent<Class>();
                var typeToken = variableDeclaratorExpression.ParentVariable.Type;
                return this.Pairs(
                    this.TryResolveNameAsGlobalOrInUsingNamespaces(@class, typeToken.Text).Get(),
                    typeToken,
                    variableDeclaratorExpression.ParentVariable);
            }

            throw new NotImplementedException();
        }

        private bool SelectVarByName(ICodePart codePart, string name)
        {
            return string.CompareOrdinal(name, codePart.GetNameFromCodePart()) == 0;
        }

        private FullyQualifiedNamesCache.NamespaceNode TryResolveNameAsGlobalOrInUsingNamespaces(Expression expression)
        {
            var typeResolver = new TypeResolver(expression.Text, this.NamesModel);
            if (typeResolver.NamespaceNode != null)
            {
                return typeResolver.NamespaceNode;
            }

            var globalType = this.NamesModel.FullyQualifiedNames[expression.Text];
            if (globalType != null)
            {
                return globalType;
            }

            // find all using derectives;
            return this.TryResolveNameInUsingNamespaces(expression);
        }

        private FullyQualifiedNamesCache.NamespaceNode TryResolveNameAsGlobalOrInUsingNamespaces(TypeToken typeToken)
        {
            var typeResolver = new TypeResolver(typeToken, this.NamesModel);
            if (typeResolver.NamespaceNode != null)
            {
                return typeResolver.NamespaceNode;
            }

            var globalType = this.NamesModel.FullyQualifiedNames[typeToken.Text];
            if (globalType != null)
            {
                return globalType;
            }

            // find all using derectives;
            return this.TryResolveNameInUsingNamespaces(typeToken);
        }

        private FullyQualifiedNamesCache.NamespaceNode TryResolveNameAsGlobalOrInUsingNamespaces(Class @class, string namespaceExpression)
        {
            var typeResolver = new TypeResolver(namespaceExpression, this.NamesModel);
            if (typeResolver.NamespaceNode != null)
            {
                return typeResolver.NamespaceNode;
            }

            var globalType = this.NamesModel.FullyQualifiedNames[namespaceExpression];
            if (globalType != null)
            {
                return globalType;
            }

            return this.TryResolveNameInUsingNamespaces(@class, namespaceExpression);
        }

        private FullyQualifiedNamesCache.NamespaceNode TryResolveNameInUsingNamespaces(Expression expression)
        {
            var @class = expression.FindParent<Class>();
            return this.TryResolveNameInUsingNamespaces(@class, expression.Text);
        }

        private FullyQualifiedNamesCache.NamespaceNode TryResolveNameInUsingNamespaces(TypeToken typeToken)
        {
            var @class = typeToken.FindParent<Class>();
            return this.TryResolveNameInUsingNamespaces(@class, typeToken.Text);
        }

        private FullyQualifiedNamesCache.NamespaceNode TryResolveNameInUsingNamespaces(Class @class, string namespaceExpression)
        {
            var currentNamespace = @class.Parent as Namespace;
            if (currentNamespace != null)
            {
                var currentNamespaceNode = this.FindNamespaceNodeByFullNamespace(currentNamespace.FullNamespaceName, namespaceExpression);
                if (currentNamespaceNode != null)
                {
                    return currentNamespaceNode;
                }
            }

            // find all using derectives;
            return (from codeElement in this.SelectAllUsingDerectives(@class.Document.DocumentContents)
                    let usingDerective = codeElement as UsingDirective
                    where usingDerective != null
                    let subNode = this.FindNamespaceNodeByFullNamespace(usingDerective.NamespaceType, namespaceExpression)
                    where subNode != null
                    select subNode).FirstOrDefault();
        }

        #endregion

        // todo: it should have ElementType and Member not just property ElementType which can hold members on same cases
        // todo: white adapters to treat code and metadata as one thing
        public class ResolvedContextItem
        {
            #region Public Properties

            public ICodeElement Member { get; set; }

            public ICodePart Code { get; set; }

            /// <summary>
            /// return type of method or references type by variable of by global name
            /// </summary>
            public ICodeElement Type { get; set; }

            // from code
            public IEnumerable<GenericTypeParameter> GenericTypesParameters { get; set; }

            // from metadata
            public IList<TypeDescriptor> GenericTypeDescriptorParameters { get; set; }

            #endregion
        }
    }
}