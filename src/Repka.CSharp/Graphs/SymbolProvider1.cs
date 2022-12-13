using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace Repka.Graphs
{
    public class SymbolProvider1 : GraphProvider
    {
        public HashSet<string> Skip { get; init; } = new();

        public int Threads { get; init; } = 1;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                Progress.Start($"Sources:");

                AdhocWorkspace workspace = new();
                Project project = workspace.AddProject("GraphProject", "C#");

                int fileCount = 0;
                FileInfo[] files = directory.GetFiles("*.cs", SearchOption.AllDirectories);
                var documents = files.AsParallel().WithDegreeOfParallelism(Threads)
                    .Where(file => !Skip.Any(skip => file.FullName.Contains(skip)))
                    .Select(file =>
                    {
                        Interlocked.Increment(ref fileCount);
                        Progress.Report($"Sources: {fileCount} of {files.Length}");
                        
                        using Stream stream = file.OpenRead();
                        SourceText text = SourceText.From(stream);
                        Document document = workspace.AddDocument(project.Id, file.Name, text);
                        return document;
                    })
                    .ToList();
                Progress.Finish($"Sources: {fileCount}");

                Progress.Start($"Documents:");
                int documentCount = 0;
                documents.AsParallel().WithDegreeOfParallelism(Threads).ForAll(document =>
                {
                    Interlocked.Increment(ref documentCount);
                    Progress.Report($"Documents: {documentCount} of {documents.Count}");
                    foreach (var token in GetTokens(document, workspace.CurrentSolution))
                    {
                        graph.Add(token);
                    }
                });
                Progress.Finish($"Documents: {documentCount}");
            }
        }

        private IEnumerable<GraphToken> GetTokens(Document document, Solution solution)
        {
            SyntaxNode? syntax = document.GetSyntaxRoot();
            SemanticModel? semantic = document.GetSemanticModel();
            if (syntax is not null && semantic is not null)
            {
                foreach (var typeSyntax in syntax.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    INamedTypeSymbol? typeSymbol = semantic.GetDeclaredSymbol(typeSyntax);
                    if (typeSymbol is not null)
                    {
                        GraphKey typeKey = typeSymbol.ToDisplayString();
                        yield return new GraphNodeToken(typeKey, CSharpLabels.IsSymbol);
                        
                        IEnumerable<INamedTypeSymbol> references = typeSymbol.GetRefererTypes(solution);
                        foreach (var refererSymbol in typeSymbol.GetRefererTypes(solution))
                        {
                            GraphKey refererKey = refererSymbol.ToDisplayString();
                            yield return new GraphLinkToken(refererKey, typeKey, CSharpLabels.UsesSymbol);
                        }

                        if (typeSymbol.IsStatic)
                        {
                            IEnumerable<IMethodSymbol> extensionMethodSymbols = typeSymbol.GetMembers()
                                .OfType<IMethodSymbol>()
                                .Where(methodSymbol => methodSymbol.IsExtensionMethod);
                            foreach (var extensionMethodSymbol in extensionMethodSymbols)
                            {
                                foreach (var refererSymbol in extensionMethodSymbol.GetRefererTypes(solution))
                                {
                                    GraphKey refererKey = refererSymbol.ToDisplayString();
                                    yield return new GraphLinkToken(refererKey, typeKey, CSharpLabels.UsesSymbol);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
