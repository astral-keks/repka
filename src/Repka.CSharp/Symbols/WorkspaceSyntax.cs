using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Security.Cryptography;

namespace Repka.Symbols
{
    public class WorkspaceSyntax
    {
        private readonly string _root;
        private readonly FileInfo _file;

        public WorkspaceSyntax(string root, FileInfo file)
        {
            _root = root;
            _file = file;
            _content = new(GetContent, true);
            _checkSum = new(GetCheckSum, true);
            _syntaxTree = new(GetSyntaxTree, true);
            _syntaxRoot = new(GetSyntaxRoot, true);
        }

        public FileInfo File => _file;

        public string RelativePath => _file.FullName.Replace(_root, string.Empty);

        public SyntaxNode Root => _syntaxRoot.Value;
        private readonly Lazy<SyntaxNode> _syntaxRoot;
        private SyntaxNode GetSyntaxRoot()
        {
            return Tree.GetCompilationUnitRoot();
        }

        public SyntaxTree Tree => _syntaxTree.Value;
        private readonly Lazy<SyntaxTree> _syntaxTree;
        private SyntaxTree GetSyntaxTree()
        {
            using MemoryStream stream = new(Content);
            SourceText text = SourceText.From(stream);
            return CSharpSyntaxTree.ParseText(text);
        }

        public DateTime LastWriteTimeUtc => File.LastWriteTimeUtc;

        public string CheckSum => _checkSum.Value;
        private readonly Lazy<string> _checkSum;
        private string GetCheckSum()
        {
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Content);
            return Convert.ToHexString(hash);
        }

        public byte[] Content => _content.Value;
        private readonly Lazy<byte[]> _content;
        private byte[] GetContent()
        {
            return System.IO.File.ReadAllBytes(File.FullName);
        }
    }
}
