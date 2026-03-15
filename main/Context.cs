#nullable enable
using System.Collections.Immutable;
using LLVMSharp.Interop;

namespace MyCompiler
{
    public record ContextEntry(LLVMValueRef Value, object value, MyType Type, MyType? ElementType = null); // really wished we would also save the actual value for easy use

    public sealed class Context
    {
        private readonly ImmutableDictionary<string, ContextEntry> _source;

        public static Context Empty => new Context();

        private Context()
            => _source = ImmutableDictionary<string, ContextEntry>.Empty;

        private Context(ImmutableDictionary<string, ContextEntry> source)
            => _source = source;

        public Context Add(string name, LLVMValueRef value, object _value, MyType type = MyType.Int, MyType? elementType = null)
        {
            var newEntry = new ContextEntry(value, _value, type, elementType);
            return new Context(_source.SetItem(name, newEntry));
        }

        public ContextEntry? Get(string key)
        {
            return _source.TryGetValue(key, out var entry) ? entry : null;
        }
        public ImmutableDictionary<string, ContextEntry> GetAll()
        {
            return _source;
        }
    }
}