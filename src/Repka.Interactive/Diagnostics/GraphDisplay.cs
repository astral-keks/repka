using Microsoft.DotNet.Interactive;

namespace Repka.Diagnostics
{
    public class GraphDisplay : Progress
    {
        private DisplayedValue? _value;
        private DateTime _lastReported;

        public override void Start(string message)
        {
            Display(message);
        }

        public override void Notify(string progress)
        {
            if (DateTime.UtcNow - _lastReported > TimeSpan.FromSeconds(0.5))
            {
                Display(progress);
            }
        }

        public override void Finish(string message)
        {
            Display(message);
        }

        private void Display(string? progress = null)
        {
            if (progress is not null)
            {
                _value ??= Kernel.display(progress);
                _value.Update(progress);
            }
            _lastReported = DateTime.UtcNow;
        }

    }
}
