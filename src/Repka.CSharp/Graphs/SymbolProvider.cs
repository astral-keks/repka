﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repka.Caching;
using Repka.Reports;
using System.Collections.Immutable;
using Repka.Symbols;
using Repka.Diagnostics;

using Workspace = Repka.Symbols.Workspace;

namespace Repka.Graphs
{
    public partial class SymbolProvider : GraphProvider
    {
        public List<WorkspaceReference> References { get; init; } = new();

        public CacheProvider CacheProvider { get; init; } = new();

        public WorkspaceProvider WorkspaceProvider { get; init; } = new();

        public ReportProvider? ReportProvider { get; init; }

        public int Threads { get; init; } = 1;

        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            IEnumerable<WorkspaceReference> references = WorkspaceProvider.GetWorkspaceReferences(key).Concat(References);
            Workspace workspace = WorkspaceProvider.CreateWorkspace(key, references);
            if (!workspace.IsEmpty)
            {
                if (ReportProvider is not null)
                {
                    using (ReportWriter referencesWriter = ReportProvider.GetWriter(key, "resolved-references"))
                        referencesWriter.Write(references.GetResolvedReferencesReport());

                    using (ReportWriter referencesWriter = ReportProvider.GetWriter(key, "unresolved-references"))
                        referencesWriter.Write(references.GetUnresolvedReferencesReport());

                    using (ReportWriter diagrnosticsWriter = ReportProvider.GetWriter(key, "diagnostics"))
                    {
                        workspace.Semantics.WithProgress(Progress, "Diagnostics")
                            .AsParallel().WithDegreeOfParallelism(Threads)
                            .Select(semantic => semantic.GetDiagnositcReport())
                            .ForAll(report => diagrnosticsWriter.Write(report));
                    }
                }

                HashSet<GraphKey> declarationKeys = new();
                using SymbolCache declarationCache = new(CacheProvider.GetCache(key, "declarations"));
                IEnumerable<GraphToken> declarationTokens = workspace.Semantics.WithProgress(Progress, "Declarations sources")
                    .AsParallel().WithDegreeOfParallelism(Threads)
                    .SelectMany(semantic => declarationCache.GetOrAdd(semantic.Syntax, () => GetDeclarationTokens(semantic).ToList()));
                foreach (var declarationToken in declarationTokens)
                {
                    if (declarationToken is GraphNodeToken nodeToken)
                        declarationKeys.Add(nodeToken.Key);
                    yield return declarationToken;
                }

                using SymbolCache usageCache = new(CacheProvider.GetCache(key, "usages"));
                IEnumerable<GraphToken> usageTokens = workspace.Semantics.WithProgress(Progress, "Usages sources")
                    .AsParallel().WithDegreeOfParallelism(Threads)
                    .SelectMany(semantic => usageCache.GetOrAdd(semantic.Syntax, () => GetUsageTokens(semantic, declarationKeys).ToList()));
                foreach (var usageToken in usageTokens)
                    yield return usageToken;
            }
        }

        private IEnumerable<GraphToken> GetDeclarationTokens(WorkspaceSemantic semantic)
        {
            foreach (var descendant in semantic.Syntax.Root.DescendantNodes())
            {
                if (descendant is BaseTypeDeclarationSyntax typeDeclarationSyntax)
                {
                    foreach (var token in GetTypeDeclarationTokens(typeDeclarationSyntax, semantic.Model, semantic.Syntax.File))
                        yield return token;
                }
                else if (descendant is FieldDeclarationSyntax fieldDeclarationSyntax)
                {
                    foreach (var token in GetFieldDeclarationTokens(fieldDeclarationSyntax, semantic.Model, semantic.Syntax.File))
                        yield return token;
                }
                else if (descendant is PropertyDeclarationSyntax propertyDeclarationSyntax)
                {
                    foreach (var token in GetPropertyDeclarationTokens(propertyDeclarationSyntax, semantic.Model, semantic.Syntax.File))
                        yield return token;
                }
                else if (descendant is MethodDeclarationSyntax methodDeclarationSyntax)
                {
                    foreach (var token in GetMethodDeclarationTokens(methodDeclarationSyntax, semantic.Model, semantic.Syntax.File))
                        yield return token;
                }
            }
        }

        private IEnumerable<GraphToken> GetTypeDeclarationTokens(BaseTypeDeclarationSyntax typeSyntax, SemanticModel semantic, FileInfo file)
        {
            INamedTypeSymbol? typeSymbol = semantic.GetDeclaredSymbol(typeSyntax);
            return CreateSymbolTokens(typeSymbol, CSharpLabels.IsType, file);
        }

        private IEnumerable<GraphToken> GetFieldDeclarationTokens(FieldDeclarationSyntax fieldSyntax, SemanticModel semantic, FileInfo file)
        {
            foreach (var variable in fieldSyntax.Declaration.Variables)
            {
                ISymbol? fieldSymbol = semantic.GetDeclaredSymbol(variable);
                foreach (var token in CreateSymbolTokens(fieldSymbol, CSharpLabels.IsField, file))
                    yield return token;
            }
        }

        private IEnumerable<GraphToken> GetPropertyDeclarationTokens(PropertyDeclarationSyntax propertySyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? propertySymbol = semantic.GetDeclaredSymbol(propertySyntax);
            return CreateSymbolTokens(propertySymbol, CSharpLabels.IsProperty, file);
        }

        private IEnumerable<GraphToken> GetMethodDeclarationTokens(MethodDeclarationSyntax methodSyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? methodSymbol = semantic.GetDeclaredSymbol(methodSyntax);
            return CreateSymbolTokens(methodSymbol, CSharpLabels.IsMethod, file);
        }

        private IEnumerable<GraphToken> CreateSymbolTokens(ISymbol? symbol, GraphLabel label, FileInfo file)
        {
            if (symbol is not null)
            {
                GraphKey symbolKey = symbol.ToDisplayString(SymbolFormat.Default);
                yield return new GraphNodeToken(symbolKey, CSharpLabels.IsSymbol, label);

                GraphKey fileKey = file.FullName;
                yield return new GraphLinkToken(fileKey, symbolKey, CSharpLabels.DefinesSymbol);
            }
        }


        private IEnumerable<GraphToken> GetUsageTokens(WorkspaceSemantic semantic, ISet<GraphKey> scope)
        {
            foreach (var linkToken in GetUsageTokens(semantic))
            {
                if (scope.Contains(linkToken.SourceKey) || scope.Contains(linkToken.TargetKey))
                    yield return linkToken;
            }
        }

        private IEnumerable<GraphLinkToken> GetUsageTokens(WorkspaceSemantic semantic)
        {
            foreach (var descendant in semantic.Syntax.Root.DescendantNodes().OfType<TypeSyntax>())
            {
                SymbolInfo symbolInfo = semantic.Model.GetSymbolInfo(descendant);
                ImmutableArray<ISymbol> symbols = symbolInfo.CandidateSymbols;
                if (symbolInfo.Symbol is not null)
                    symbols = symbols.Add(symbolInfo.Symbol);
                foreach (var symbol in symbols)
                {
                    if (symbol is INamedTypeSymbol typeSymbol)
                    {
                        foreach (var token in GetTypeUsageTokens(typeSymbol, semantic.Syntax.File))
                            yield return token;
                    }
                    else if (symbol is IFieldSymbol fieldSymbol)
                    {
                        foreach (var token in GetMemberUsageTokens(fieldSymbol, semantic.Syntax.File))
                            yield return token;
                    }
                    else if (symbol is IPropertySymbol propertySymbol)
                    {
                        foreach (var token in GetMemberUsageTokens(propertySymbol, semantic.Syntax.File))
                            yield return token;
                    }
                    else if (symbol is IMethodSymbol methodSymbol)
                    {
                        foreach (var token in GetMemberUsageTokens(methodSymbol, semantic.Syntax.File))
                            yield return token;
                    }
                }
            }
        }

        private IEnumerable<GraphLinkToken> GetTypeUsageTokens(INamedTypeSymbol typeSymbol, FileInfo file)
        {
            if (typeSymbol.IsGenericType)
                typeSymbol = typeSymbol.OriginalDefinition;

            if (typeSymbol is not null)
            {
                GraphKey fileKey = file.FullName;
                GraphKey typeKey = typeSymbol.ToDisplayString(SymbolFormat.Default);
                yield return new GraphLinkToken(fileKey, typeKey, CSharpLabels.UsesSymbol);
            }
        }

        private IEnumerable<GraphLinkToken> GetMemberUsageTokens(ISymbol memberSymbol, FileInfo file)
        {
            if (memberSymbol is IMethodSymbol methodSymbol && methodSymbol.IsGenericMethod)
                memberSymbol = methodSymbol.OriginalDefinition;

            GraphKey fileKey = file.FullName;
            GraphKey memberKey = memberSymbol.ToDisplayString(SymbolFormat.Default);
            yield return new GraphLinkToken(fileKey, memberKey, CSharpLabels.UsesSymbol);

            foreach (var token in GetTypeUsageTokens(memberSymbol.ContainingType, file))
                yield return token;
        }
    }
}
