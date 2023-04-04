namespace Repka.Projects
{
    public class DocumentReference
    {
        public DocumentReference(string relativePath, string absolutePath)
        {
            RelativePath = relativePath;
            AbsolutePath = absolutePath;
        }

        public string RelativePath { get; }

        public string AbsolutePath { get; }
    }
}
