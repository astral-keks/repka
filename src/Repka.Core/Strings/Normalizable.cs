namespace Repka.Strings
{
    public abstract class Normalizable
    {
        public Normalizable(string value)
        {
            Normalized = value.ToLowerInvariant();
        }

        protected string Normalized { get; }
    }
}
