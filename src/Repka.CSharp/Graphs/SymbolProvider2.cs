using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace Repka.Graphs
{
    public class SymbolProvider2 : GraphProvider
    {
        public HashSet<string> Skip { get; init; } = new();

        public int Threads { get; init; } = 1;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                Progress.Start($"Sources");
                var sources = directory.EnumerateFiles("*.cs", SearchOption.AllDirectories)
                    .Where(file => !Skip.Any(skip => file.FullName.Contains(skip)))
                    .Select(file => (File: file, Syntax: file.ToSyntax()))
                    .ToList();

                Progress.Start($"Sources: {sources.Count}");
                CSharpCompilation compilation = CSharpCompilation.Create(directory.FullName.GetHashCode().ToString());
                compilation = compilation.AddSyntaxTrees(sources.Select(source => source.Syntax.Tree));

                int i = 0;
                sources.AsParallel().WithDegreeOfParallelism(Threads).ForAll(source =>
                {
                    Interlocked.Increment(ref i);
                    Progress.Report($"Sources: {i} of {sources.Count}");
                    foreach (var typeToken in GetTypeTokens(source.Syntax.Unit, compilation, source.File.FullName))
                    {
                        graph.Add(typeToken);
                    }
                });
                Progress.Start($"Sources: {i}");
            }
        }

        private IEnumerable<GraphToken> GetTypeTokens(CompilationUnitSyntax unitSyntax, CSharpCompilation compilation, GraphKey refererKey)
        {
            foreach (var syntax in unitSyntax.DescendantNodes())
            {
                if (syntax is TypeDeclarationSyntax typeDeclarationSyntax)
                {
                    INamedTypeSymbol? symbol = compilation.GetDeclaredSymbol(typeDeclarationSyntax) as INamedTypeSymbol;
                    foreach (var token in GetTypeDeclarationTokens(typeDeclarationSyntax, compilation, refererKey))
                        yield return token;
                }
                else if (syntax is TypeSyntax typeUsageSyntax)
                {
                    foreach (var token in GetTypeUsageTokens(typeUsageSyntax, compilation, refererKey))
                        yield return token;
                }
                else if (syntax is BlockSyntax blockSyntax)
                {

                }
            }
        }

        private IEnumerable<GraphToken> GetTypeDeclarationTokens(TypeDeclarationSyntax typeDeclarationSyntax, CSharpCompilation compilation, 
            GraphKey refererKey)
        {
            ISymbol? typeDeclarationSymbol = compilation.GetDeclaredSymbol(typeDeclarationSyntax);
            if (typeDeclarationSymbol is not null)
            {
                GraphKey typeKey = typeDeclarationSymbol.ToDisplayString();
                yield return new GraphNodeToken(typeKey, CSharpLabels.IsSymbol);
                yield return new GraphLinkToken(refererKey, typeKey, CSharpLabels.DefinesSymbol);
            }
            
        }

        private IEnumerable<GraphToken> GetTypeUsageTokens(TypeSyntax typeSyntax, CSharpCompilation compilation, GraphKey refererKey)
        {
            ISymbol? typeSymbol = compilation.GetSymbol(typeSyntax);
            if (typeSymbol is not null)
            {
                INamedTypeSymbol? namedTypeSymbol = default;
                if (typeSymbol is INamedTypeSymbol)
                    namedTypeSymbol = typeSymbol as INamedTypeSymbol;
                else if (typeSymbol is IMethodSymbol methodSymbol && methodSymbol.IsExtensionMethod)
                    namedTypeSymbol = methodSymbol.ContainingType;

                if (namedTypeSymbol is not null)
                {
                    GraphKey typeKey = namedTypeSymbol.ToDisplayString();
                    yield return new GraphLinkToken(refererKey, typeKey, CSharpLabels.UsesSymbol);
                }
            }
        }

        private IEnumerable<GraphToken> GetMethodTokens(MethodDeclarationSyntax methodSyntax, CSharpCompilation compilation, GraphKey refererKey)
        {
            ISymbol? methodSymbol = compilation.GetDeclaredSymbol(methodSyntax);
            if (methodSymbol is not null)
            {
                GraphKey methodKey = methodSymbol.ToDisplayString();
                yield return new GraphNodeToken(methodKey, CSharpLabels.IsSymbol);
                yield return new GraphLinkToken(refererKey, methodKey, CSharpLabels.DefinesSymbol);
            }
        }

        private void Test()
        {
            
        }


        //public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        //{
        //    DirectoryInfo directory = new(key);
        //    if (directory.Exists)
        //    {
        //        Progress.Start($"Sources");
        //        var sources = directory.EnumerateFiles("*.cs", SearchOption.AllDirectories)
        //            .Select(file => (File: file, Syntax: file.ToSyntax()))
        //            .ToList();

        //        Progress.Start($"Sources: {sources.Count}");
        //        CSharpCompilation compilation = CSharpCompilation.Create(directory.FullName.GetHashCode().ToString());
        //        compilation = compilation.AddSyntaxTrees(sources.Select(source => source.Syntax.Tree));

        //        int i = 0;
        //        foreach (var source in sources)
        //        {
        //            Progress.Report($"Sources: {++i} of {sources.Count}");
        //            foreach (var typeToken in GetTypeTokens(source.Syntax.Unit, compilation, source.File.FullName))
        //            {
        //                yield return typeToken;
        //            }
        //        }
        //        Progress.Start($"Sources: {i}");
        //    }
        //}

    }
}
