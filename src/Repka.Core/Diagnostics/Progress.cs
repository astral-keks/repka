namespace Repka.Diagnostics
{
    public class Progress
    {
        public virtual void Start(string message)
        {
        }

        public virtual void Notify(string progress)
        {
        }

        public virtual void Finish(string message)
        {
        }
    }
}
