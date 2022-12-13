namespace Repka.Caching
{
    public abstract class ObjectStorage
    {
        public abstract Stream Read(string key);

        public abstract Stream Write(string key);
    }
}
