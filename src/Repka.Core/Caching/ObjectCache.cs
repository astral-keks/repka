namespace Repka.Caching
{
    public class ObjectCache
    {
        private readonly ObjectStorage _storage;
        private readonly ObjectFormat[] _formats;

        public ObjectCache(ObjectStorage storage, params ObjectFormat[] formats)
        {
            _storage = storage;
            _formats = formats;
        }

        public TValue GetOrAdd<TValue>(string key, Func<TValue> factory)
        {
            TValue? value = Get<TValue>(key);

            if (value is null)
            {
                value = factory();
                Add(key, value);
            }

            return value;
        }

        private TValue? Get<TValue>(string key)
        {
            TValue? value = default;

            using Stream stream = _storage.Read(key);
            if (stream != Stream.Null)
            {
                ObjectFormat<TValue> format = _formats
                    .OfType<ObjectFormat<TValue>>()
                    .Single();
                format.ReadValue(stream, out value);
            }

            return value;
        }

        private void Add<TValue>(string key, TValue value)
        {
            using Stream stream = _storage.Write(key);

            ObjectFormat<TValue> format = _formats
                .OfType<ObjectFormat<TValue>>()
                .Single();
            format.WriteValue(stream, value);
        }

    }
}
