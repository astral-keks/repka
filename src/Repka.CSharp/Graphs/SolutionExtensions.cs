using Microsoft.Build.Construction;

namespace Repka.Graphs
{
    internal static class SolutionExtensions
    {
        public static SolutionFile? ToSolution(this FileInfo file)
        {
            try
            {
                return SolutionFile.Parse(file.FullName);
            }
            catch
            {
                return null;
            }
        }
    }
}
