using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Symbols;
using System.Collections.Immutable;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.SymbolDsl;

namespace Repka.Graphs
{
    public partial class SymbolProvider : GraphProvider
    {
        public override void AddTokens(GraphKey key, Graph graph)
        {
            HashSet<GraphKey> keys = new();
            List<DocumentNode> documents = graph.Documents().ToList();
            ProgressPercentage symbolProgress = Progress.Percent("Collecting symbol declarations", documents.Count);
            foreach (var token in documents.Peek(symbolProgress.Increment).SelectMany(document => GetDeclarationTokens(document)))
            {
                graph.Add(token);
                if (token is GraphNodeToken nodeToken)
                    keys.Add(nodeToken.Key);
            }
            symbolProgress.Complete();

            symbolProgress = Progress.Percent("Collecting symbol references", documents.Count);
            foreach (var token in documents.Peek(symbolProgress.Increment).SelectMany(document => GetUsageTokens(document, keys)))
                graph.Add(token);
            symbolProgress.Complete();
        }

        private IEnumerable<GraphToken> GetDeclarationTokens(DocumentNode documentNode)
        {
            FileInfo file = documentNode.File;
            SyntaxNode? syntax = documentNode.Syntax();
            SemanticModel? semantic = documentNode.Semantic();

            if (syntax is not null && semantic is not null)
            {
                foreach (var descendant in syntax.DescendantNodes())
                {
                    if (descendant is BaseTypeDeclarationSyntax typeDeclarationSyntax)
                    {
                        foreach (var token in GetTypeDeclarationTokens(typeDeclarationSyntax, semantic, file))
                            yield return token;
                    }
                    else if (descendant is FieldDeclarationSyntax fieldDeclarationSyntax)
                    {
                        foreach (var token in GetFieldDeclarationTokens(fieldDeclarationSyntax, semantic, file))
                            yield return token;
                    }
                    else if (descendant is PropertyDeclarationSyntax propertyDeclarationSyntax)
                    {
                        foreach (var token in GetPropertyDeclarationTokens(propertyDeclarationSyntax, semantic, file))
                            yield return token;
                    }
                    else if (descendant is MethodDeclarationSyntax methodDeclarationSyntax)
                    {
                        foreach (var token in GetMethodDeclarationTokens(methodDeclarationSyntax, semantic, file))
                            yield return token;
                    }
                }
            }
        }

        private IEnumerable<GraphToken> GetTypeDeclarationTokens(BaseTypeDeclarationSyntax typeSyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? typeSymbol = semantic.GetDeclaredSymbol(typeSyntax);
            return CreateSymbolTokens(typeSymbol, typeSyntax, file, SymbolLabels.IsType);
        }

        private IEnumerable<GraphToken> GetFieldDeclarationTokens(FieldDeclarationSyntax fieldSyntax, SemanticModel semantic, FileInfo file)
        {
            foreach (var variableSyntax in fieldSyntax.Declaration.Variables)
            {
                ISymbol? fieldSymbol = semantic.GetDeclaredSymbol(variableSyntax);
                foreach (var token in CreateSymbolTokens(fieldSymbol, variableSyntax, file, SymbolLabels.IsField))
                    yield return token;
            }
        }

        private IEnumerable<GraphToken> GetPropertyDeclarationTokens(PropertyDeclarationSyntax propertySyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? propertySymbol = semantic.GetDeclaredSymbol(propertySyntax);
            return CreateSymbolTokens(propertySymbol, propertySyntax, file, SymbolLabels.IsProperty);
        }

        private IEnumerable<GraphToken> GetMethodDeclarationTokens(MethodDeclarationSyntax methodSyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? methodSymbol = semantic.GetDeclaredSymbol(methodSyntax);
            return CreateSymbolTokens(methodSymbol, methodSyntax, file, SymbolLabels.IsMethod);
        }

        private IEnumerable<GraphToken> CreateSymbolTokens(ISymbol? symbol, SyntaxNode syntax, FileInfo file, GraphLabel label)
        {
            if (symbol is not null)
            {
                GraphKey symbolKey = symbol.ToDisplayString(SymbolFormat.Default);
                int symbolSize = syntax.ToString().Count(ch => ch == '\n') + 1;
                GraphLabel symbolSizeLabel = new(SymbolLabels.SymbolSize, symbolSize.ToString());
                yield return new GraphNodeToken(symbolKey, SymbolLabels.IsSymbol, symbolSizeLabel, label);

                GraphKey fileKey = file.FullName;
                yield return new GraphLinkToken(fileKey, symbolKey, SymbolLabels.DefinesSymbol);
            }
        }


        private IEnumerable<GraphToken> GetUsageTokens(DocumentNode documentNode, ISet<GraphKey> scope)
        {
            FileInfo file = documentNode.File;
            SyntaxNode? syntax = documentNode.Syntax();
            SemanticModel? semantic = documentNode.Semantic();

            if (syntax is not null && semantic is not null)
            {
                foreach (var linkToken in GetUsageTokens(syntax, semantic, file))
                {
                    if (scope.Contains(linkToken.SourceKey) || scope.Contains(linkToken.TargetKey))
                        yield return linkToken;
                }
            }
        }

        private IEnumerable<GraphLinkToken> GetUsageTokens(SyntaxNode syntax, SemanticModel semantic, FileInfo file)
        {
            foreach (var descendant in syntax.DescendantNodes().OfType<TypeSyntax>())
            {
                SymbolInfo symbolInfo = semantic.GetSymbolInfo(descendant);
                ImmutableArray<ISymbol> symbols = symbolInfo.CandidateSymbols;
                if (symbolInfo.Symbol is not null)
                    symbols = symbols.Add(symbolInfo.Symbol);
                foreach (var symbol in symbols)
                {
                    if (symbol is INamedTypeSymbol typeSymbol)
                    {
                        foreach (var token in GetTypeUsageTokens(typeSymbol, file))
                            yield return token;
                    }
                    else if (symbol is IFieldSymbol fieldSymbol)
                    {
                        foreach (var token in GetMemberUsageTokens(fieldSymbol, file))
                            yield return token;
                    }
                    else if (symbol is IPropertySymbol propertySymbol)
                    {
                        foreach (var token in GetMemberUsageTokens(propertySymbol, file))
                            yield return token;
                    }
                    else if (symbol is IMethodSymbol methodSymbol)
                    {
                        foreach (var token in GetMemberUsageTokens(methodSymbol, file))
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
                yield return new GraphLinkToken(fileKey, typeKey, SymbolLabels.UsesSymbol);
            }
        }

        private IEnumerable<GraphLinkToken> GetMemberUsageTokens(ISymbol memberSymbol, FileInfo file)
        {
            if (memberSymbol is IMethodSymbol methodSymbol && methodSymbol.IsGenericMethod)
                memberSymbol = methodSymbol.OriginalDefinition;

            GraphKey fileKey = file.FullName;
            GraphKey memberKey = memberSymbol.ToDisplayString(SymbolFormat.Default);
            yield return new GraphLinkToken(fileKey, memberKey, SymbolLabels.UsesSymbol);

            foreach (var token in GetTypeUsageTokens(memberSymbol.ContainingType, file))
                yield return token;
        }
    }
}
