using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Repka.Graphs
{
    public class SymbolProvider : GraphProvider
    {
        public HashSet<string> Skip { get; init; } = new();

        public int Threads { get; init; } = 1;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                Progress.Start($"Sources:");

                int fileCount = 0;
                FileInfo[] files = directory.GetFiles("*.cs", SearchOption.AllDirectories);
                var documents = files.AsParallel().WithDegreeOfParallelism(Threads)
                    .Select(file =>
                    {
                        Interlocked.Increment(ref fileCount);
                        Progress.Report($"Sources: {fileCount} of {files.Length}");
                        return file;
                    })
                    .Where(file => !Skip.Any(skip => file.FullName.Contains(skip)))
                    .Select(file => (File: file, Syntax: file.ToSyntax()))
                    .ToList();
                Progress.Finish($"Sources: {fileCount}");

                CSharpCompilation compilation = CSharpCompilation.Create(directory.FullName.GetHashCode().ToString());
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)); // mscorlib
                compilation = compilation.AddSyntaxTrees(documents.Select(document => document.Syntax.Tree));

                Progress.Start($"Documents:");
                int documentCount = 0;
                documents.AsParallel().WithDegreeOfParallelism(Threads).ForAll(document =>
                {
                    Interlocked.Increment(ref documentCount);
                    Progress.Report($"Documents: {documentCount} of {documents.Count}");

                    SemanticModel semantic = compilation.GetSemanticModel(document.Syntax.Tree, ignoreAccessibility: true);
                    foreach (var token in GetTokens(document.Syntax.Unit, semantic, document.File))
                    {
                        graph.Add(token);
                    }
                });
                Progress.Finish($"Documents: {documentCount}");
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
