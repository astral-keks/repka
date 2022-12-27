using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repka.Caching;

namespace Repka.Graphs
{
    public class SymbolProvider : GraphProvider
    {
        public CacheProvider Caching { get; init; } = new();

        public HashSet<string> Skip { get; init; } = new();

        public int Threads { get; init; } = 1;

        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            ICollection<SymbolSource> sources = GetSources(key);
            if (sources.Any())
            {
                using SymbolCache cache = new(Caching.GetCache(key));

                Lazy<Compilation> compilation = new(() => GetCompilation(sources), isThreadSafe: true);
                foreach (var symbol in GetTokens(sources, compilation, cache))
                {
                    yield return symbol;
                }
            }
        }

        private ICollection<SymbolSource> GetSources(GraphKey key)
        {
            List<SymbolSource> sources = new();

            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                int fileCount = 0;
                Progress.Start($"Files *.cs:");
                FileInfo[] files = directory.GetFiles("*.cs", SearchOption.AllDirectories);
                sources.AddRange(files.AsParallel().WithDegreeOfParallelism(Threads)
                    .Select(file =>
                    {
                        Interlocked.Increment(ref fileCount);
                        Progress.Report($"Files *.cs: {fileCount} of {files.Length}");
                        return file;
                    })
                    .Where(file => !Skip.Any(skip => file.FullName.Contains(skip)))
                    .Select(file => new SymbolSource(key, file)));
                Progress.Finish($"Files *.cs: {fileCount}");
            }

            return sources;
        }

        private Compilation GetCompilation(ICollection<SymbolSource> sources)
        {
            CSharpCompilation compilation = CSharpCompilation.Create(Guid.NewGuid().ToString());
            compilation = compilation.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)); // mscorlib
            compilation = compilation.AddSyntaxTrees(sources.Select(source => source.SyntaxTree));
            return compilation;
        }

        private IEnumerable<GraphToken> GetTokens(ICollection<SymbolSource> sources, Lazy<Compilation> compilation, SymbolCache cache)
        {
            int sourceCount = 0;
            Progress.Start($"Sources:");
            ParallelQuery<GraphToken> symbols = sources.AsParallel()
                .WithDegreeOfParallelism(Threads)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .SelectMany(source =>
                {
                    Interlocked.Increment(ref sourceCount);
                    Progress.Report($"Sources: {sourceCount} of {sources.Count}");

                    return cache.GetOrAdd(source, () => GetTokens(source, compilation.Value).ToList());
                });
            foreach (var symbol in symbols)
            {
                yield return symbol;
            }

            Progress.Finish($"Sources: {sourceCount}");
        }

        private IEnumerable<GraphToken> GetTokens(SymbolSource source, Compilation compilation)
        {
            SemanticModel semantic = compilation.GetSemanticModel(source.SyntaxTree, ignoreAccessibility: true);
            foreach (var token in GetTokens(source.SyntaxRoot, semantic, source.File))
            {
                yield return token;
            }
        }

        private IEnumerable<GraphToken> GetTokens(SyntaxNode syntax, SemanticModel semantic, FileInfo file)
        {
            foreach (var descendant in syntax.DescendantNodes())
            {
                if (descendant is BaseTypeDeclarationSyntax typeDeclarationSyntax)
                {
                    foreach (var token in GetTypeDeclarationTokens(typeDeclarationSyntax, semantic, file))
                        yield return token;
                }
                else if (descendant is TypeSyntax typeSyntax)
                {
                    foreach (var token in GetTypeUsageTokens(typeSyntax, semantic, file))
                        yield return token;
                }
            }
        }

        private IEnumerable<GraphToken> GetTypeDeclarationTokens(BaseTypeDeclarationSyntax typeSyntax, SemanticModel semantic, FileInfo file)
        {
            INamedTypeSymbol? typeSymbol = semantic.GetDeclaredSymbol(typeSyntax);
            if (typeSymbol is not null)
            {
                GraphKey typeKey = typeSymbol.ToDisplayString();
                yield return new GraphNodeToken(typeKey, CSharpLabels.IsSymbol);

                GraphKey fileKey = file.FullName;
                yield return new GraphLinkToken(fileKey, typeKey, CSharpLabels.DefinesSymbol);
            }
        }

        private IEnumerable<GraphToken> GetTypeUsageTokens(TypeSyntax typeSyntax, SemanticModel semantic, FileInfo file)
        {
            ISymbol? typeSymbol = semantic.GetSymbolInfo(typeSyntax).Symbol;

            INamedTypeSymbol? namedTypeSymbol = default;
            if (typeSymbol is INamedTypeSymbol)
            {
                namedTypeSymbol = (INamedTypeSymbol)typeSymbol;
                if (namedTypeSymbol.IsGenericType)
                    namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
            }
            else if (typeSymbol is IMethodSymbol methodSymbol && methodSymbol.IsExtensionMethod)
                namedTypeSymbol = methodSymbol.ContainingType;
            
            if (namedTypeSymbol is not null)
            {
                GraphKey fileKey = file.FullName;
                GraphKey typeKey = namedTypeSymbol.ToDisplayString();
                yield return new GraphLinkToken(fileKey, typeKey, CSharpLabels.UsesSymbol);
            }
        }

    }
}
