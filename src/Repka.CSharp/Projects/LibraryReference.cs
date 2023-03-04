namespace Repka.Projects
{
    internal class LibraryReference
    {
        public LibraryReference(string relativePath, string absolutePath)
        {
            RelativePath = relativePath;
            AbsolutePath = absolutePath;
        }

        public string RelativePath { get; }

        public string AbsolutePath { get; }
    }
}
