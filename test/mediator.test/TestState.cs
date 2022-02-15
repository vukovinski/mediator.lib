using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace mediator.test;

public class TestState<T>
{
    public T? Value { get; set; }

    public List<string> CallerNames = new();
    public async Task SetAsync(T? value) => Value = value;
    public void AddCallerName([CallerMemberName] string callerName = "unknownCaller") => CallerNames.Add(callerName);

    public override string ToString() => Value?.ToString() ?? "Null";
}
