#nullable enable
using System.Collections.Immutable;
using LLVMSharp.Interop;

namespace MyCompiler
{
    public record ContextEntry(LLVMValueRef Value, Type? Type, Type? ElementType = null);

    public sealed class Context
    {
        private readonly ImmutableDictionary<string, ContextEntry> _source;

        public static Context Empty => new Context();

        private Context()
            => _source = ImmutableDictionary<string, ContextEntry>.Empty;

        private Context(ImmutableDictionary<string, ContextEntry> source)
            => _source = source;

        public Context Add(string name, LLVMValueRef value, Type? type = null, Type? elementType = null)
        {
            var newEntry = new ContextEntry(value, type, elementType);
            return new Context(_source.SetItem(name, newEntry));
        }

        public ContextEntry? Get(string key)
        {
            return _source.TryGetValue(key, out var entry) ? entry : null;
        }
    }
}