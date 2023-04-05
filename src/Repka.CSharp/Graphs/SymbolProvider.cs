using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repka.Caching;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Workspaces;
using System.Collections.Immutable;
using static Repka.Graphs.ProjectDsl;
using static Repka.Graphs.SymbolDsl;

namespace Repka.Graphs
{
    public partial class SymbolProvider : GraphProvider
    {
        public ReportProvider? ReportProvider { get; init; }

        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            WorkspaceBuilder workspaceBuilder = new();
            workspaceBuilder.AddSolution(key);

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Creating workspace", projectNodes.Count);
            projectNodes.AsParallel().Peek(projectProgress.Increment).ForAll(projectNode => workspaceBuilder.AddProject(projectNode));
            projectProgress.Complete();

            AdhocWorkspace workspace = workspaceBuilder.Workspace;

            if (ReportProvider is not null)
            {
                projectProgress = Progress.Percent("Collecting diagnostics", projectNodes.Count);
                ReportProvider.Report(workspace).ForAll(_ => projectProgress.Increment());
                projectProgress.Complete();
            }

            List<Document> documents = workspace.CurrentSolution.Projects.SelectMany(project => project.Documents).ToList();

            HashSet<GraphKey> keys = new();
            ProgressPercentage symbolProgress = Progress.Percent("Collecting symbol declarations", documents.Count);
            foreach (var token in documents.AsParallel().Peek(symbolProgress.Increment).SelectMany(document => GetDeclarationTokens(document)))
            {
                yield return token;
                if (token is GraphNodeToken nodeToken)
                    keys.Add(nodeToken.Key);
            }
            symbolProgress.Complete();

            symbolProgress = Progress.Percent("Collecting symbol references", documents.Count);
            foreach (var token in documents.AsParallel().Peek(symbolProgress.Increment).SelectMany(document => GetUsageTokens(document, keys)))
                yield return token;
            symbolProgress.Complete();
        }

        private IEnumerable<GraphToken> GetDeclarationTokens(Document document)
        {
            FileInfo file = document.GetFile();
            SyntaxNode syntax = document.GetSyntax();
            SemanticModel semantic = document.GetSemantic();

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

        private IEnumerable<GraphToken> GetTypeDeclarationTokens(BaseTypeDeclarationSyntax typeSyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? typeSymbol = semantic.GetDeclaredSymbol(typeSyntax);
            return CreateSymbolTokens(typeSymbol, SymbolLabels.IsType, file);
        }

        private IEnumerable<GraphToken> GetFieldDeclarationTokens(FieldDeclarationSyntax fieldSyntax, SemanticModel semantic, FileInfo file)
        {
            foreach (var variable in fieldSyntax.Declaration.Variables)
            {
                ISymbol? fieldSymbol = semantic.GetDeclaredSymbol(variable);
                foreach (var token in CreateSymbolTokens(fieldSymbol, SymbolLabels.IsField, file))
                    yield return token;
            }
        }

        private IEnumerable<GraphToken> GetPropertyDeclarationTokens(PropertyDeclarationSyntax propertySyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? propertySymbol = semantic.GetDeclaredSymbol(propertySyntax);
            return CreateSymbolTokens(propertySymbol, SymbolLabels.IsProperty, file);
        }

        private IEnumerable<GraphToken> GetMethodDeclarationTokens(MethodDeclarationSyntax methodSyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? methodSymbol = semantic.GetDeclaredSymbol(methodSyntax);
            return CreateSymbolTokens(methodSymbol, SymbolLabels.IsMethod, file);
        }

        private IEnumerable<GraphToken> CreateSymbolTokens(ISymbol? symbol, GraphLabel label, FileInfo file)
        {
            if (symbol is not null)
            {
                GraphKey symbolKey = symbol.ToDisplayString(SymbolFormat.Default);
                yield return new GraphNodeToken(symbolKey, SymbolLabels.IsSymbol, label);

                GraphKey fileKey = file.FullName;
                yield return new GraphLinkToken(fileKey, symbolKey, SymbolLabels.DefinesSymbol);
            }
        }


        private IEnumerable<GraphToken> GetUsageTokens(Document document, ISet<GraphKey> scope)
        {
            FileInfo file = document.GetFile();
            SyntaxNode syntax = document.GetSyntax();
            SemanticModel semantic = document.GetSemantic();

            foreach (var linkToken in GetUsageTokens(syntax, semantic, file))
            {
                if (scope.Contains(linkToken.SourceKey) || scope.Contains(linkToken.TargetKey))
                    yield return linkToken;
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
