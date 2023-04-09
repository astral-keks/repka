using Microsoft.CodeAnalysis;
using static Repka.Graphs.DocumentDsl;

namespace Repka.Graphs
{
    public static class WorkspaceDsl
    {
        public static SemanticModel? Semantic(this DocumentNode document) => document.Attribute(WorkspaceAttributes.Semantic)
            .GetValue<SemanticModel>();

        public static SyntaxNode? Syntax(this DocumentNode document) => document.Attribute(WorkspaceAttributes.Syntax)
            .GetValue<SyntaxNode>();

        public static class WorkspaceAttributes
        {
            public const string Syntax = nameof(Syntax);
            public const string Semantic = nameof(Semantic);
        }
    }
}
