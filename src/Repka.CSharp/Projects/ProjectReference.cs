namespace Repka.Projects
{
    internal class ProjectReference
    {
        public ProjectReference(string relativePath, string absolutePath)
        {
            RelativePath = relativePath;
            AbsolutePath = absolutePath;
        }

        public string RelativePath { get; }

        public string AbsolutePath { get; }
    }
}
