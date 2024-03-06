using Microsoft.CodeAnalysis;
using Repka.Paths;
using System.Collections;

namespace Repka.Solutions
{
    public class SolutionProjectCollection : IEnumerable<SolutionProject>
    {
        private readonly Dictionary<AbsolutePath, SolutionProject> _projectsByLocation = new();

        public void AddRange(IEnumerable<AbsolutePath> projectLocations, IEnumerable<SolutionPlatform> platforms)
        {
            foreach (var projectLocation in projectLocations)
                Add(projectLocation, platforms);
        }

        public void Add(AbsolutePath projectLocation, IEnumerable<SolutionPlatform> platforms)
        {
            _projectsByLocation[projectLocation] = SolutionProject.Csproj(projectLocation, platforms);
           
            foreach (var folderPath in projectLocation.Parents())
            {
                ;
                if (!_projectsByLocation.ContainsKey(folderPath))
                    _projectsByLocation[folderPath] = SolutionProject.Folder(folderPath);
            }
        }

        public IEnumerable<(SolutionProject Project, SolutionProject Folder)> AsRelations() => 
            _projectsByLocation.Keys
                .SelectMany(path =>
                {
                    AbsolutePath projectPath = path;
                    AbsolutePath? folderPath = path.Parent();

                    List<(SolutionProject Project, SolutionProject Folder)> relations = new();
                    if (!string.IsNullOrWhiteSpace(projectPath?.ToString()) &&
                        !string.IsNullOrWhiteSpace(folderPath?.ToString()) &&
                        _projectsByLocation.TryGetValue(projectPath, out SolutionProject? project) &&
                        _projectsByLocation.TryGetValue(folderPath, out SolutionProject? folder))
                        relations.Add((project, folder));
                    return relations;
                });

        public IEnumerator<SolutionProject> GetEnumerator() => 
            _projectsByLocation.Values
                .Where(project => 
                    !string.IsNullOrWhiteSpace(project.Name) &&
                    !string.IsNullOrWhiteSpace(project.Decription))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            GetEnumerator();
    }
}
