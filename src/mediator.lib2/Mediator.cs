using System;
using System.Linq;
using System.Reflection;

using ImTools;

namespace mediator.lib2;

public class Mediator
{
    public Mediator() { }
    public Mediator(Mediator previous) : this(previous._handlers.Enumerate().Select(e => e.Value)) { }
    public Mediator(IEnumerable<Delegate> delegates) => delegates.ToList().ForEach(d => RegisterHandler(d));

    public static Mediator Singleton => _instance.Value;
    private static readonly Lazy<Mediator> _instance = new(() => new Mediator());
    private ImHashMap<MethodDescription, Delegate> _handlers = ImHashMap<MethodDescription, Delegate>.Empty;

    public void RegisterHandler(Delegate d)
    {
        var key = new MethodDescription(d);
        var hash = RegistrationHasher.Calculate(key);
        _handlers = _handlers.AddOrUpdate(hash, key, d);
    }

    public void UnregisterHandlerByRef(Delegate d)
    {
        var key = new MethodDescription(d);
        var hash = RegistrationHasher.Calculate(key);
        _handlers = _handlers.Remove(hash, key);
    }

    public void ReplaceHandler(Delegate old, Delegate @new)
    {
        UnregisterHandlerByRef(old);
        RegisterHandler(@new);
    }

    public void Clear() => _handlers = ImHashMap<MethodDescription, Delegate>.Empty;

    #region handle
    public void Handle<A>(A input)
    {
        ResolveActionHandler<A>().Invoke(input);
    }

    public void Handle<A, B>(A input, B input2)
    {
        ResolveActionHandler<A, B>().Invoke(input, input2);
    }

    public void Handle<A, B, C>(A input, B input2, C input3)
    {
        ResolveActionHandler<A, B, C>().Invoke(input, input2, input3);
    }

    public void Handle<A, B, C, D>(A input, B input2, C input3, D input4)
    {
        ResolveActionHandler<A, B, C, D>().Invoke(input, input2, input3, input4);
    }

    public void Handle<A, B, C, D, E>(A input, B input2, C input3, D input4, E input5)
    {
        ResolveActionHandler<A, B, C, D, E>().Invoke(input, input2, input3, input4, input5);
    }

    public void Handle<A, B, C, D, E, F>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        ResolveActionHandler<A, B, C, D, E, F>().Invoke(input, input2, input3, input4, input5, input6);
    }

    public R Handle<A, R>(A input)
    {
        return ResolveFunctionHandler<A, R>().Invoke(input);
    }

    public R Handle<A, B, R>(A input, B input2)
    {
        return ResolveFunctionHandler<A, B, R>().Invoke(input, input2);
    }

    public R Handle<A, B, C, R>(A input, B input2, C input3)
    {
        return ResolveFunctionHandler<A, B, C, R>().Invoke(input, input2, input3);
    }

    public R Handle<A, B, C, D, R>(A input, B input2, C input3, D input4)
    {
        return ResolveFunctionHandler<A, B, C, D, R>().Invoke(input, input2, input3, input4);
    }

    public R Handle<A, B, C, D, E, R>(A input, B input2, C input3, D input4, E input5)
    {
        return ResolveFunctionHandler<A, B, C, D, E, R>().Invoke(input, input2, input3, input4, input5);
    }

    public R Handle<A, B, C, D, E, F, R>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return ResolveFunctionHandler<A, B, C, D, E, F, R>().Invoke(input, input2, input3, input4, input5, input6);
    }
    #endregion

    #region start handle
    public Task StartHandler<A>(A input)
    {
        return Task.Run(() => ResolveActionHandler<A>().Invoke(input));
    }

    public Task StartHandler<A, B>(A input, B input2)
    {
        return Task.Run(() => ResolveActionHandler<A, B>().Invoke(input, input2));
    }

    public Task StartHandler<A, B, C>(A input, B input2, C input3)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C>().Invoke(input, input2, input3));
    }

    public Task StartHandler<A, B, C, D>(A input, B input2, C input3, D input4)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C, D>().Invoke(input, input2, input3, input4));
    }

    public Task StartHandlerr<A, B, C, D, E>(A input, B input2, C input3, D input4, E input5)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C, D, E>().Invoke(input, input2, input3, input4, input5));
    }

    public Task StartHandler<A, B, C, D, E, F>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C, D, E, F>().Invoke(input, input2, input3, input4, input5, input6));
    }

    public Task<R> StartHandler<A, R>(A input)
    {
        return Task.Run(() => ResolveFunctionHandler<A, R>().Invoke(input));
    }

    public Task<R> StartHandler<A, B, R>(A input, B input2)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, R>().Invoke(input, input2));
    }

    public Task<R> StartHandler<A, B, C, R>(A input, B input2, C input3)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, R>().Invoke(input, input2, input3));
    }

    public Task<R> StartHandler<A, B, C, D, R>(A input, B input2, C input3, D input4)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, D, R>().Invoke(input, input2, input3, input4));
    }

    public Task<R> StartHandler<A, B, C, D, E, R>(A input, B input2, C input3, D input4, E input5)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, D, E, R>().Invoke(input, input2, input3, input4, input5));
    }

    public Task<R> StartHandler<A, B, C, D, E, F, R>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, D, E, F, R>().Invoke(input, input2, input3, input4, input5, input6));
    }
    #endregion

    #region handle async
    public async Task HandleAsync<A>(A input)
    {
        await Task.Run(async () => ResolveActionHandler<A>().Invoke(input));
    }

    public async Task HandleAsync<A, B>(A input, B input2)
    {
        await Task.Run(async () => ResolveActionHandler<A, B>().Invoke(input, input2));
    }

    public async Task HandleAsync<A, B, C>(A input, B input2, C input3)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C>().Invoke(input, input2, input3));
    }

    public async Task HandleAsync<A, B, C, D>(A input, B input2, C input3, D input4)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C, D>().Invoke(input, input2, input3, input4));
    }

    public async Task HandleAsync<A, B, C, D, E>(A input, B input2, C input3, D input4, E input5)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C, D, E>().Invoke(input, input2, input3, input4, input5));
    }

    public async Task HandleAsync<A, B, C, D, E, F>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C, D, E, F>().Invoke(input, input2, input3, input4, input5, input6));
    }

    public async Task<R> HandleAsync<A, R>(A input)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, R>().Invoke(input));
    }

    public async Task<R> HandleAsync<A, B, R>(A input, B input2)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, R>().Invoke(input, input2));
    }

    public async Task<R> HandleAsync<A, B, C, R>(A input, B input2, C input3)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, R>().Invoke(input, input2, input3));
    }

    public async Task<R> HandleAsync<A, B, C, D, R>(A input, B input2, C input3, D input4)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, D, R>().Invoke(input, input2, input3, input4));
    }

    public async Task<R> HandleAsync<A, B, C, D, E, R>(A input, B input2, C input3, D input4, E input5)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, D, E, R>().Invoke(input, input2, input3, input4, input5));
    }

    public async Task<R> HandleAsync<A, B, C, D, E, F, R>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, D, E, F, R>().Invoke(input, input2, input3, input4, input5, input6));
    }
    #endregion

    #region resolve action handler
    public Action<A>? ResolveActionHandler<A>()
    {
        var key = MethodDescription.ActOfSig<A>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A>)entry.Value;
    }

    public Action<A, B>? ResolveActionHandler<A, B>()
    {
        var key = MethodDescription.ActOfSig<A, B>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B>)entry.Value;
    }

    public Action<A, B, C>? ResolveActionHandler<A, B, C>()
    {
        var key = MethodDescription.ActOfSig<A, B, C>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B, C>)entry.Value;
    }

    public Action<A, B, C, D>? ResolveActionHandler<A, B, C, D>()
    {
        var key = MethodDescription.ActOfSig<A, B, C, D>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B, C, D>)entry.Value;
    }

    public Action<A, B, C, D, E>? ResolveActionHandler<A, B, C, D, E>()
    {
        var key = MethodDescription.ActOfSig<A, B, C, D, E>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action < A, B, C, D, E>)entry.Value;
    }

    public Action<A, B, C, D, E, F>? ResolveActionHandler<A, B, C, D, E, F>()
    {
        var key = MethodDescription.ActOfSig<A, B, C, D, E, F>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B, C, D, E, F>)entry.Value;
    }
    #endregion

    #region resolve function handler
    public Func<A, R>? ResolveFunctionHandler<A,R>()
    {
        var key = MethodDescription.FuncOfSig<A, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, R>)entry.Value;
    }

    public Func<A, B, R>? ResolveFunctionHandler<A, B, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, R>)entry.Value;
    }

    public Func<A, B, C, R>? ResolveFunctionHandler<A, B, C, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, C, R>)entry.Value;
    }

    public Func<A, B, C, D, R>? ResolveFunctionHandler<A, B, C, D, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, D, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, C, D, R>)entry.Value;
    }

    public Func<A, B, C, D, E, R>? ResolveFunctionHandler<A, B, C, D, E, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, D, E, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, C, D, E, R>)entry.Value;
    }

    public Func<A, B, C, D, E, F, R>? ResolveFunctionHandler<A, B, C, D, E, F, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, D, E, F, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func < A, B, C, D, E, F, R>)entry.Value;
    }
    #endregion
}
