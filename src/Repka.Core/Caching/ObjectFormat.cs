using System.Diagnostics.CodeAnalysis;

namespace Repka.Caching
{
    public abstract class ObjectFormat
    {
        public abstract Type ValueType { get; }

        public abstract void ReadValue(Stream stream, out object? value);

        public abstract void WriteValue(Stream stream, object value);
    }

    public abstract class ObjectFormat<TValue> : ObjectFormat
    {
        public override Type ValueType => typeof(TValue);

        public override void ReadValue(Stream stream, out object? value)
        {
            ReadValue(stream, out TValue? tvalue);
            value = tvalue;
        }

        public override void WriteValue(Stream stream, object value)
        {
            TValue tvalue = (TValue)value;
            WriteValue(stream, tvalue);
        }

        public abstract void ReadValue(Stream stream, out TValue? value);

        public abstract void WriteValue(Stream stream, TValue value);
    }
}
