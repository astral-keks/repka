using Microsoft.CodeAnalysis;
using Repka.Workspaces;
using static Repka.Graphs.DocumentDsl;

namespace Repka.Graphs
{
    public static class WorkspaceDsl
    {
        public static SemanticModel? Semantic(this DocumentNode document) =>
            document.Attribute<SemanticAttribute>()?.Value;

        public static SyntaxNode? Syntax(this DocumentNode document) =>
            document.Attribute<SyntaxAttribute>()?.Value;

        public class SemanticAttribute : GraphAttribute<SemanticModel>
        {
            public SemanticAttribute(Document document) 
                : base(document.FilePath ?? throw new ArgumentNullException("document.FilePath"), () => document.GetSemantic())
            {
            }
        }

        public class SyntaxAttribute : GraphAttribute<SyntaxNode>
        {
            public SyntaxAttribute(Document document)
                : base(document.FilePath ?? throw new ArgumentNullException("document.FilePath"), () => document.GetSyntax())
            {
            }
        }
    }
}
