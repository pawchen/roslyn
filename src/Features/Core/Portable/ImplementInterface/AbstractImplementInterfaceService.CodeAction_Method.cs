﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.ImplementInterface
{
    internal abstract partial class AbstractImplementInterfaceService
    {
        internal partial class ImplementInterfaceCodeAction
        {
            private ISymbol GenerateMethod(
                Compilation compilation,
                IMethodSymbol method,
                Accessibility accessibility,
                DeclarationModifiers modifiers,
                bool generateAbstractly,
                bool useExplicitInterfaceSymbol,
                string memberName)
            {
                var syntaxFacts = Document.GetRequiredLanguageService<ISyntaxFactsService>();

                var updatedMethod = method.EnsureNonConflictingNames(State.ClassOrStructType, syntaxFacts);

                updatedMethod = updatedMethod.RemoveInaccessibleAttributesAndAttributesOfTypes(
                    State.ClassOrStructType,
                    AttributesToRemove(compilation));

                return CodeGenerationSymbolFactory.CreateMethodSymbol(
                    updatedMethod,
                    accessibility: accessibility,
                    modifiers: modifiers,
                    explicitInterfaceImplementations: useExplicitInterfaceSymbol ? [updatedMethod] : default,
                    name: memberName,
                    statements: generateAbstractly
                        ? default
                        : [CreateStatement(compilation, updatedMethod)]);
            }

            private SyntaxNode CreateStatement(Compilation compilation, IMethodSymbol method)
            {
                var factory = Document.GetRequiredLanguageService<SyntaxGenerator>();
                return ThroughMember == null
                    ? factory.CreateThrowNotImplementedStatement(compilation)
                    : factory.GenerateDelegateThroughMemberStatement(method, ThroughMember);
            }
        }
    }
}
