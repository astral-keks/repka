using Repka.Paths;
using System.Text.RegularExpressions;

namespace Repka.Solutions
{
    internal class SolutionFile
    {
        private static readonly Regex _csprojPattern = new(@"([^""]+\.csproj)""");

        public static SolutionFile Parse(AbsolutePath path)
        {
            List<AbsolutePath> projects = File.ReadAllLines(path)
                .Select(line => _csprojPattern.Match(line))
                .Where(match => match.Groups[1].Success)
                .Select(match => match.Groups[1].Value)
                .Select(relativePath => path.Parent()?.Combine(relativePath))
                .OfType<AbsolutePath>()
                .ToList();
            return new SolutionFile { Projects = projects };
        }

        public List<AbsolutePath> Projects { get; init; } = new(0);
    }
}
