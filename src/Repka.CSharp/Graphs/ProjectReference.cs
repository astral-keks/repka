namespace Repka.Graphs
{
    public class ProjectReference
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
