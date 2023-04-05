namespace Repka.FileSystems
{
    public static class FileSystemPaths
    {
        public const string Repka = ".repka";

        public static string Aux(string? root, string name) 
            => Path.Combine(Aux(root), name);

        public static string Aux(string? root)
        {
            root ??= ".";

            if (root.EndsWith(Repka))
                return root;
            int index = root.IndexOf(Repka);
            if (index >= 0)
                return root.Substring(0, index + Repka.Length);
            if (File.Exists(root))
                root = Path.GetDirectoryName(root);
            return Path.Combine(root ?? ".", ".repka");
        }
    }
}
