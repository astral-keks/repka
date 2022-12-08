using Microsoft.DotNet.Interactive.Formatting;

namespace Repka.Graphs
{
    public class GraphFormatters
    {
        public static GraphFormatters Reset() => new(true);
        public static GraphFormatters NoReset() => new(false);
        private GraphFormatters(bool reset)
        {
            if (reset)
                Formatter.ResetToDefault();
        }

        public Registration<T> Register<T>(Func<T, object> formatter)
        {
            return new Registration<T>(formatter, this);
        }

        public class Registration<T>
        {
            private readonly Func<T, object> _formatter;
            private readonly GraphFormatters _formatters;

            public Registration(Func<T, object> formatter, GraphFormatters formatters)
            {
                _formatter = formatter;
                _formatters = formatters;
            }

            public GraphFormatters As(params string[] mediaTypes)
            {
                AsSelf(mediaTypes);
                AsEnumerable(mediaTypes);
                return _formatters;
            }

            public GraphFormatters AsSelf(params string[] mediaTypes)
            {
                foreach (var mimeType in mediaTypes)
                {
                    Formatter.Register<T>(item => _formatter(item).ToDisplayString(mimeType), mimeType);
                }

                return _formatters;
            }

            public GraphFormatters AsEnumerable(params string[] mediaTypes)
            {
                foreach (var mimeType in mediaTypes)
                {
                    Formatter.Register<IEnumerable<T>>(items => items.Select(_formatter).ToDisplayString(mimeType), mimeType);
                }

                return _formatters;
            }
        }
    }
}
