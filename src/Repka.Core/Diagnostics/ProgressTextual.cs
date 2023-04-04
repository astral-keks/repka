namespace Repka.Diagnostics
{
    public class ProgressTextual : Progress
    {
        private readonly TextWriter _writer;

        public static ProgressTextual Console { get; } = new(System.Console.Out);

        public ProgressTextual(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Start(string message)
        {
            _writer.WriteLine(message);
        }

        public override void Notify(string progress)
        {
            _writer.WriteLine(progress);
        }

        public override void Finish(string message)
        {
            _writer.WriteLine(message);
        }
    }
}
