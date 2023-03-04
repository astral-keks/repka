using Microsoft.CodeAnalysis;

namespace Repka.Symbols
{
    public class WorkspaceSemantic
    {
        private readonly Compilation _compilation;

        public WorkspaceSemantic(WorkspaceSyntax source, Compilation compilation)
        {
            Syntax = source;
            _compilation = compilation;
            _semantic = new(GetSemantic, true);
            _diagnostics = new(GetDiagnostics, true);
        }

        public WorkspaceSyntax Syntax { get; }

        public SemanticModel Model => _semantic.Value;
        private readonly Lazy<SemanticModel> _semantic;
        private SemanticModel GetSemantic()
        {
            return _compilation.GetSemanticModel(Syntax.Tree, ignoreAccessibility: true);
        }

        public IEnumerable<Diagnostic> Errors => Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        public ICollection<Diagnostic> Diagnostics => _diagnostics.Value;
        private readonly Lazy<ICollection<Diagnostic>> _diagnostics;
        private ICollection<Diagnostic> GetDiagnostics()
        {
            return Model.GetDiagnostics(Syntax.Root.Span);
        }
    }
}
