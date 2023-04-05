﻿using Repka.FileSystems;

namespace Repka.Diagnostics
{
    public class FileSystemReportProvider : ReportProvider
    {
        public override ReportWriter GetWriter(string store, string name)
        {
            string directory = FileSystemPaths.Aux(store);
            Directory.CreateDirectory(directory);

            string location = Path.Combine(directory, $"{name}.txt");
            StreamWriter writer = new(location);
            return new FileSystemReportWriter(writer);
        }
    }
}
