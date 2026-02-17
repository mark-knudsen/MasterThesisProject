using System.Collections.Immutable;
using LLVMSharp.Interop;

namespace MyCompiler;

public sealed class Context
{
    private readonly ImmutableDictionary<string, (LLVMValueRef Value, MyType Type)> _source;

    public static Context Empty => new Context();

    private Context()
        => _source = ImmutableDictionary<string, (LLVMValueRef, MyType)>.Empty;

    private Context(ImmutableDictionary<string, (LLVMValueRef, MyType)> source)
        => _source = source;

    public Context Add(string key, LLVMValueRef value, MyType type)
        => new Context(_source.SetItem(key, (value, type)));

    public (LLVMValueRef Value, MyType Type)? Get(string key)
    {
        if (_source.TryGetValue(key, out var value))
            return value;

        return null;
    }
}
