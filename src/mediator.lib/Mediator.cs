using System;
using System.Linq;
using System.Reflection;

using ImTools;

namespace mediator.lib;

public class Mediator
{
    public Mediator() { }
    public Mediator(Mediator previous) : this(previous._handlers.Enumerate().Select(e => e.Value)) { }
    public Mediator(IEnumerable<Delegate> delegates) => delegates.ToList().ForEach(d => RegisterHandler(d));

    /// <summary>
    /// The singleton instance of the mediator for use in simple apps.
    /// </summary>
    public static Mediator Singleton => _instance.Value;
    private static readonly Lazy<Mediator> _instance = new(() => new Mediator());
    private ImHashMap<MethodDescription, Delegate> _handlers = ImHashMap<MethodDescription, Delegate>.Empty;

    /// <summary>
    /// Register a method to the handler manifest.
    /// </summary>
    /// <param name="d">The method to register.</param>
    public void RegisterHandler(Delegate d)
    {
        var key = new MethodDescription(d);
        var hash = RegistrationHasher.Calculate(key);
        _handlers = _handlers.AddOrUpdate(hash, key, d);
    }

    /// <summary>
    /// Unregister a method from the handler manifest.
    /// </summary>
    /// <param name="d">The method to unregister.</param>
    public void UnregisterHandlerByRef(Delegate d)
    {
        var key = new MethodDescription(d);
        var hash = RegistrationHasher.Calculate(key);
        _handlers = _handlers.Remove(hash, key);
    }

    /// <summary>
    /// Replace a registered method with another method.
    /// </summary>
    /// <param name="old">Reference of the old method.</param>
    /// <param name="new">Reference of the new method.</param>
    public void ReplaceHandler(Delegate old, Delegate @new)
    {
        UnregisterHandlerByRef(old);
        RegisterHandler(@new);
    }

    /// <summary>
    /// Clear the handler manifest.
    /// </summary>
    public void Clear() => _handlers = ImHashMap<MethodDescription, Delegate>.Empty;

    #region handle
    /// <summary>
    /// Resolves an action delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException">Throws NRE when no handler for given input is found.</exception>
    public void Handle<A>(A input)
    {
        ResolveActionHandler<A>().Invoke(input);
    }

    /// <summary>
    /// Resolves an action delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException">Throws NRE when no handler for given input is found.</exception>
    public void Handle<A, B>(A input, B input2)
    {
        ResolveActionHandler<A, B>().Invoke(input, input2);
    }

    /// <summary>
    /// Resolves an action delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException">Throws NRE when no handler for given input is found.</exception>
    public void Handle<A, B, C>(A input, B input2, C input3)
    {
        ResolveActionHandler<A, B, C>().Invoke(input, input2, input3);
    }

    /// <summary>
    /// Resolves an action delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException">Throws NRE when no handler for given input is found.</exception>
    public void Handle<A, B, C, D>(A input, B input2, C input3, D input4)
    {
        ResolveActionHandler<A, B, C, D>().Invoke(input, input2, input3, input4);
    }

    /// <summary>
    /// Resolves an action delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException">Throws NRE when no handler for given input is found.</exception>
    public void Handle<A, B, C, D, E>(A input, B input2, C input3, D input4, E input5)
    {
        ResolveActionHandler<A, B, C, D, E>().Invoke(input, input2, input3, input4, input5);
    }

    /// <summary>
    /// Resolves an action delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException">Throws NRE when no handler for given input is found.</exception>
    public void Handle<A, B, C, D, E, F>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        ResolveActionHandler<A, B, C, D, E, F>().Invoke(input, input2, input3, input4, input5, input6);
    }

    /// <summary>
    /// Resolves a function delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The result of the found handler.</returns>
    /// <exception cref="NullReferenceException">Throws NRe when no handler matching the input and return types is found.</exception>
    public R Handle<A, R>(A input)
    {
        return ResolveFunctionHandler<A, R>().Invoke(input);
    }

    /// <summary>
    /// Resolves a function delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The result of the found handler.</returns>
    /// <exception cref="NullReferenceException">Throws NRe when no handler matching the input and return types is found.</exception>
    public R Handle<A, B, R>(A input, B input2)
    {
        return ResolveFunctionHandler<A, B, R>().Invoke(input, input2);
    }

    /// <summary>
    /// Resolves a function delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The result of the found handler.</returns>
    /// <exception cref="NullReferenceException">Throws NRe when no handler matching the input and return types is found.</exception>
    public R Handle<A, B, C, R>(A input, B input2, C input3)
    {
        return ResolveFunctionHandler<A, B, C, R>().Invoke(input, input2, input3);
    }

    /// <summary>
    /// Resolves a function delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The result of the found handler.</returns>
    /// <exception cref="NullReferenceException">Throws NRe when no handler matching the input and return types is found.</exception>
    public R Handle<A, B, C, D, R>(A input, B input2, C input3, D input4)
    {
        return ResolveFunctionHandler<A, B, C, D, R>().Invoke(input, input2, input3, input4);
    }

    /// <summary>
    /// Resolves a function delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The result of the found handler.</returns>
    /// <exception cref="NullReferenceException">Throws NRe when no handler matching the input and return types is found.</exception>
    public R Handle<A, B, C, D, E, R>(A input, B input2, C input3, D input4, E input5)
    {
        return ResolveFunctionHandler<A, B, C, D, E, R>().Invoke(input, input2, input3, input4, input5);
    }

    /// <summary>
    /// Resolves a function delegate and invokes it with the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The result of the found handler.</returns>
    /// <exception cref="NullReferenceException">Throws NRE when no handler matching the input and return types is found.</exception>
    public R Handle<A, B, C, D, E, F, R>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return ResolveFunctionHandler<A, B, C, D, E, F, R>().Invoke(input, input2, input3, input4, input5, input6);
    }
    #endregion

    #region start handle
    /// <summary>
    /// Finds an action delegate and invokes it on the configured task pool.
    /// </summary>
    /// <returns>A references to the Task representing the work.</returns>
    public Task StartHandler<A>(A input)
    {
        return Task.Run(() => ResolveActionHandler<A>().Invoke(input));
    }

    /// <summary>
    /// Finds an action delegate and invokes it on the configured task pool.
    /// </summary>
    /// <returns>A references to the Task representing the work.</returns>
    public Task StartHandler<A, B>(A input, B input2)
    {
        return Task.Run(() => ResolveActionHandler<A, B>().Invoke(input, input2));
    }

    /// <summary>
    /// Finds an action delegate and invokes it on the configured task pool.
    /// </summary>
    /// <returns>A references to the Task representing the work.</returns>
    public Task StartHandler<A, B, C>(A input, B input2, C input3)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C>().Invoke(input, input2, input3));
    }

    /// <summary>
    /// Finds an action delegate and invokes it on the configured task pool.
    /// </summary>
    /// <returns>A references to the Task representing the work.</returns>
    public Task StartHandler<A, B, C, D>(A input, B input2, C input3, D input4)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C, D>().Invoke(input, input2, input3, input4));
    }

    /// <summary>
    /// Finds an action delegate and invokes it on the configured task pool.
    /// </summary>
    /// <returns>A references to the Task representing the work.</returns>
    public Task StartHandlerr<A, B, C, D, E>(A input, B input2, C input3, D input4, E input5)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C, D, E>().Invoke(input, input2, input3, input4, input5));
    }

    /// <summary>
    /// Finds an action delegate and invokes it on the configured task pool.
    /// </summary>
    /// <returns>A references to the Task representing the work.</returns>
    public Task StartHandler<A, B, C, D, E, F>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return Task.Run(() => ResolveActionHandler<A, B, C, D, E, F>().Invoke(input, input2, input3, input4, input5, input6));
    }

    /// <summary>
    /// Finds a function delegate and invokes it on a configured task pool.
    /// </summary>
    /// <returns>A reference to the Task[<typeparamref name="R"/>] representing the work.</returns>
    public Task<R> StartHandler<A, R>(A input)
    {
        return Task.Run(() => ResolveFunctionHandler<A, R>().Invoke(input));
    }

    /// <summary>
    /// Finds a function delegate and invokes it on a configured task pool.
    /// </summary>
    /// <returns>A reference to the Task[<typeparamref name="R"/>] representing the work.</returns>
    public Task<R> StartHandler<A, B, R>(A input, B input2)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, R>().Invoke(input, input2));
    }

    /// <summary>
    /// Finds a function delegate and invokes it on a configured task pool.
    /// </summary>
    /// <returns>A reference to the Task[<typeparamref name="R"/>] representing the work.</returns>
    public Task<R> StartHandler<A, B, C, R>(A input, B input2, C input3)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, R>().Invoke(input, input2, input3));
    }

    /// <summary>
    /// Finds a function delegate and invokes it on a configured task pool.
    /// </summary>
    /// <returns>A reference to the Task[<typeparamref name="R"/>] representing the work.</returns>
    public Task<R> StartHandler<A, B, C, D, R>(A input, B input2, C input3, D input4)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, D, R>().Invoke(input, input2, input3, input4));
    }

    /// <summary>
    /// Finds a function delegate and invokes it on a configured task pool.
    /// </summary>
    /// <returns>A reference to the Task[<typeparamref name="R"/>] representing the work.</returns>
    public Task<R> StartHandler<A, B, C, D, E, R>(A input, B input2, C input3, D input4, E input5)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, D, E, R>().Invoke(input, input2, input3, input4, input5));
    }

    /// <summary>
    /// Finds a function delegate and invokes it on a configured task pool.
    /// </summary>
    /// <returns>A reference to the Task[<typeparamref name="R"/>] representing the work.</returns>
    public Task<R> StartHandler<A, B, C, D, E, F, R>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return Task.Run(() => ResolveFunctionHandler<A, B, C, D, E, F, R>().Invoke(input, input2, input3, input4, input5, input6));
    }
    #endregion

    #region handle async
    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task HandleAsync<A>(A input)
    {
        await Task.Run(async () => ResolveActionHandler<A>().Invoke(input));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task HandleAsync<A, B>(A input, B input2)
    {
        await Task.Run(async () => ResolveActionHandler<A, B>().Invoke(input, input2));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task HandleAsync<A, B, C>(A input, B input2, C input3)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C>().Invoke(input, input2, input3));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task HandleAsync<A, B, C, D>(A input, B input2, C input3, D input4)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C, D>().Invoke(input, input2, input3, input4));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task HandleAsync<A, B, C, D, E>(A input, B input2, C input3, D input4, E input5)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C, D, E>().Invoke(input, input2, input3, input4, input5));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task HandleAsync<A, B, C, D, E, F>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        await Task.Run(async () => ResolveActionHandler<A, B, C, D, E, F>().Invoke(input, input2, input3, input4, input5, input6));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task<R> HandleAsync<A, R>(A input)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, R>().Invoke(input));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task<R> HandleAsync<A, B, R>(A input, B input2)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, R>().Invoke(input, input2));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task<R> HandleAsync<A, B, C, R>(A input, B input2, C input3)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, R>().Invoke(input, input2, input3));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task<R> HandleAsync<A, B, C, D, R>(A input, B input2, C input3, D input4)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, D, R>().Invoke(input, input2, input3, input4));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task<R> HandleAsync<A, B, C, D, E, R>(A input, B input2, C input3, D input4, E input5)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, D, E, R>().Invoke(input, input2, input3, input4, input5));
    }

    /// <summary>
    /// Finds an action delegate and invokes it asynchronously.
    /// </summary>
    public async Task<R> HandleAsync<A, B, C, D, E, F, R>(A input, B input2, C input3, D input4, E input5, F input6)
    {
        return await Task.Run(async () => ResolveFunctionHandler<A, B, C, D, E, F, R>().Invoke(input, input2, input3, input4, input5, input6));
    }
    #endregion

    #region resolve action handler (untyped)
    /// <summary>
    /// Finds a registered action delegate whose arguments match the passed param types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveActionHandler(Type paramType)
    {
        var key = MethodDescription.ActOfParams(paramType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose arguments match the passed param types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveActionHandler(Type paramType, Type paramType2)
    {
        var key = MethodDescription.ActOfParams(paramType, paramType2);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose arguments match the passed param types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveActionHandler(Type paramType, Type paramType2, Type paramType3)
    {
        var key = MethodDescription.ActOfParams(paramType, paramType2, paramType3);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose arguments match the passed param types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveActionHandler(Type paramType, Type paramType2, Type paramType3, Type paramType4)
    {
        var key = MethodDescription.ActOfParams(paramType, paramType2, paramType3, paramType4);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose arguments match the passed param types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveActionHandler(Type paramType, Type paramType2, Type paramType3, Type paramType4, Type paramType5)
    {
        var key = MethodDescription.ActOfParams(paramType, paramType2, paramType3, paramType4, paramType5);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose arguments match the passed param types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveActionHandler(Type paramType, Type paramType2, Type paramType3, Type paramType4, Type paramType5, Type paramType6)
    {
        var key = MethodDescription.ActOfParams(paramType, paramType2, paramType3, paramType4, paramType5, paramType6);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }
    #endregion

    #region resolve action handler
    /// <summary>
    /// Finds a registered action delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Action<A>? ResolveActionHandler<A>()
    {
        var key = MethodDescription.ActOfSig<A>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A>)entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Action<A, B>? ResolveActionHandler<A, B>()
    {
        var key = MethodDescription.ActOfSig<A, B>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B>)entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Action<A, B, C>? ResolveActionHandler<A, B, C>()
    {
        var key = MethodDescription.ActOfSig<A, B, C>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B, C>)entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Action<A, B, C, D>? ResolveActionHandler<A, B, C, D>()
    {
        var key = MethodDescription.ActOfSig<A, B, C, D>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B, C, D>)entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Action<A, B, C, D, E>? ResolveActionHandler<A, B, C, D, E>()
    {
        var key = MethodDescription.ActOfSig<A, B, C, D, E>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action < A, B, C, D, E>)entry.Value;
    }

    /// <summary>
    /// Finds a registered action delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Action<A, B, C, D, E, F>? ResolveActionHandler<A, B, C, D, E, F>()
    {
        var key = MethodDescription.ActOfSig<A, B, C, D, E, F>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Action<A, B, C, D, E, F>)entry.Value;
    }
    #endregion

    #region resolve function handler (untyped)
    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type returnType)
    {
        var key = MethodDescription.FuncOfParams(returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type paramType1, Type returnType)
    {
        var key = MethodDescription.FuncOfParams(paramType1, returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type paramType1, Type paramType2, Type returnType)
    {
        var key = MethodDescription.FuncOfParams(paramType1, paramType2, returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type paramType1, Type paramType2, Type paramType3, Type returnType)
    {
        var key = MethodDescription.FuncOfParams(paramType1, paramType2, paramType3, returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type paramType1, Type paramType2, Type paramType3, Type paramType4, Type returnType)
    {
        var key = MethodDescription.FuncOfParams(paramType1, paramType2, paramType3, paramType4, returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type paramType1, Type paramType2, Type paramType3, Type paramType4, Type paramType5, Type returnType)
    {
        var key = MethodDescription.FuncOfParams(paramType1, paramType2, paramType3, paramType4, paramType5, returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose arguments match the passed param and return types.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Delegate? ResolveFunctionHandler(Type paramType1, Type paramType2, Type paramType3, Type paramType4, Type paramType5, Type paramType6, Type returnType)
    {
        var key = MethodDescription.FuncOfParams(paramType1, paramType2, paramType3, paramType4, paramType5, paramType6, returnType);
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : entry.Value;
    }
    #endregion

    #region resolve function handler
    /// <summary>
    /// Finds a registered function delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Func<A, R>? ResolveFunctionHandler<A,R>()
    {
        var key = MethodDescription.FuncOfSig<A, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, R>)entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Func<A, B, R>? ResolveFunctionHandler<A, B, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, R>)entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Func<A, B, C, R>? ResolveFunctionHandler<A, B, C, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, C, R>)entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Func<A, B, C, D, R>? ResolveFunctionHandler<A, B, C, D, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, D, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, C, D, R>)entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Func<A, B, C, D, E, R>? ResolveFunctionHandler<A, B, C, D, E, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, D, E, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func<A, B, C, D, E, R>)entry.Value;
    }

    /// <summary>
    /// Finds a registered function delegate whose signature matches the passed typeparams.
    /// </summary>
    /// <returns>A reference to the registered handler or null.</returns>
    public Func<A, B, C, D, E, F, R>? ResolveFunctionHandler<A, B, C, D, E, F, R>()
    {
        var key = MethodDescription.FuncOfSig<A, B, C, D, E, F, R>();
        var hash = RegistrationHasher.Calculate(key);
        var entry = _handlers.GetEntryOrDefault(hash, key);

        return entry == null ? null : (Func < A, B, C, D, E, F, R>)entry.Value;
    }
    #endregion
}
