namespace Repka.Solutions
{
    public class SolutionPlatform
    {
        public static SolutionPlatform DebugAnyCPU { get; } = new SolutionPlatform("Debug|Any CPU");
        public static SolutionPlatform ReleaseAnyCPU { get; } = new SolutionPlatform("Release|Any CPU");

        private readonly string _name;

        public SolutionPlatform(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
