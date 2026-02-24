using System.Collections.Immutable;
using LLVMSharp.Interop;

namespace MyCompiler;

public sealed class Context
{
    private readonly ImmutableDictionary<string, (LLVMValueRef Value, MyType Type, int? length)> _source;

    public static Context Empty => new Context();

    private Context()
        => _source = ImmutableDictionary<string, (LLVMValueRef, MyType, int?)>.Empty;

    private Context(ImmutableDictionary<string, (LLVMValueRef, MyType, int?)> source)
        => _source = source;

    public Context Add(string key, LLVMValueRef value, MyType type, int? length = default)
        => new Context(_source.SetItem(key, (value, type, length)));

    public (LLVMValueRef Value, MyType Type, int? length)? Get(string key)
    {
        if (_source.TryGetValue(key, out var value))
            return value;

        return null;
    }
}
