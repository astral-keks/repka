using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Repka.Workspaces
{
    public static class WorkspaceExtensions
    {
        private static readonly SyntaxNode EmptySyntaxNode = SyntaxFactory.EmptyStatement();
        private static readonly SyntaxTree EmptySyntaxTree = SyntaxFactory.SyntaxTree(EmptySyntaxNode);
        private static readonly Compilation EmptyCompilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), new[] { EmptySyntaxTree });
        private static readonly SemanticModel EmptySemanticModel = EmptyCompilation.GetSemanticModel(EmptySyntaxTree);

        public static FileInfo GetFile(this Document document)
        {
            return new(document.FilePath ?? throw new ArgumentException("Document has no file path"));
        }

        public static SyntaxNode GetSyntax(this Document document)
        {
            return document.GetSyntaxRootAsync(CancellationToken.None).GetAwaiter().GetResult() ?? EmptySyntaxNode;
        }

        public static SemanticModel GetSemantic(this Document document)
        {
            return document.GetSemanticModelAsync(CancellationToken.None).GetAwaiter().GetResult() ?? EmptySemanticModel;
        }
    }
}
