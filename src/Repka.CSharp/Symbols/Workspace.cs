using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Repka.Reports;

namespace Repka.Symbols
{
    public class Workspace
    {
        private readonly List<WorkspaceSyntax> _syntaxes;
        private readonly List<WorkspaceReference> _references;

        public Workspace(IEnumerable<WorkspaceSyntax> sources, IEnumerable<WorkspaceReference> references)
        {
            _syntaxes = sources.ToList();
            _references = references.ToList();

            _semantics = new(GetSemantics, true);
            _compilation = new(GetCompilation, true);
        }

        public bool IsEmpty => !_syntaxes.Any();

        public ICollection<WorkspaceSemantic> Semantics => _semantics.Value;
        private readonly Lazy<ICollection<WorkspaceSemantic>> _semantics;
        public ICollection<WorkspaceSemantic> GetSemantics()
        {
            return _syntaxes.Select(syntax => new WorkspaceSemantic(syntax, Compilation)).ToList();
        }

        public Compilation Compilation => _compilation.Value;
        private readonly Lazy<Compilation> _compilation;
        public Compilation GetCompilation()
        {
            CSharpCompilation compilation = CSharpCompilation.Create(Guid.NewGuid().ToString());
            IEnumerable<PortableExecutableReference> references = _references
                .Where(reference => reference.Exists)
                .Select(reference => reference.Target)
                .OfType<string>()
                .Select(path => MetadataReference.CreateFromFile(path));
            compilation = compilation.AddReferences(references);
            compilation = compilation.AddSyntaxTrees(_syntaxes.Select(source => source.Tree));
            return compilation;
        }
    }
}
