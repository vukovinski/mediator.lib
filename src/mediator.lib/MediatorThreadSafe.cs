using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("mediator.test")]

namespace mediator.lib;


// use case: have an API controller call some unknown! method with POSTed data, and get the results back, with more indirection than usual

// example call site:

//class ExampleController
//{
//    private readonly Mediator _mediator;

//    public ExampleController(Mediator mediator) => _mediator = mediator;

//    public string? PostData(PostDataMessage data)
//    {
//        if (data != null)
//        {
//            var result = _mediator.Handle<PostDataMessage,PostDataResponse>(data); // call indirection;
//            return new string(result.ToString());
//        }
//        return null;
//    }
//}

//record PostDataMessage(object data);
//record PostDataResponse(int statusCode);

/// <summary>
/// A thread safe communication primitive. Holds a manifest of registered handler methods, which can
/// be called using one of the handle methods. Depending on the shape of the input, a handler will be chosen.
/// </summary>
public sealed class MediatorThreadSafe
{
    private readonly object _lock = new();
    private HandlerRegistration? this[HandlerArity arity, params Type[] argTypes]
    {
        get
        {
            try
            {
                Monitor.Enter(_lock);
                var rCount = argTypes.Length;
                var requestedArgTypes = argTypes;
                var requestedReturn = typeof(void);

                var candidates = _handlers
                    .Where(r => r.Arity == arity)
                    .Where(r => r.ArgTypes.SequenceEqual(requestedArgTypes))
                    .Where(r => r.Handler.Method.ReturnType == requestedReturn);

                return candidates.SingleOrDefault();
            }
            finally
            {
                if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
            }
        }
        set
        {
            try
            {
                Monitor.Enter(_lock);
                var rCount = argTypes.Length;
                var requestedArgTypes = argTypes;
                var requestedReturn = typeof(void);

                var candidates = _handlers
                    .Where(r => r.Arity == arity)
                    .Where(r => r.ArgTypes.SequenceEqual(requestedArgTypes))
                    .Where(r => r.Handler.Method.ReturnType == requestedReturn);

                var candidate = candidates.Single();
                var candidateIndex = _handlers.IndexOf(candidate);
                _handlers[candidateIndex] = value
                    ?? throw new ArgumentNullException(nameof(value), "New handler cannot be null.");
            }
            finally
            {
                if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
            }
        }
    }

    private bool ExistsHandler(HandlerArity arity, params Type[] argTypes)
    {
        return this[arity, argTypes] != null;
    }

    private bool TryGetHandler<THandler>(HandlerArity arity, out THandler? handler) where THandler : Delegate
    {
        handler = null;
        try
        {
            Monitor.Enter(_lock);
            var rCount = typeof(THandler).GetGenericArguments().Length;
            var requestedFunc = typeof(THandler).Name.StartsWith("Func");
            var requestedArgTypes = typeof(THandler).GetGenericArguments().SkipLast(requestedFunc ? 1 : 0);
            var requestedReturn = typeof(THandler).GetGenericArguments().Skip(requestedFunc ? rCount - 1 : rCount).SingleOrDefault();

            handler = _handlers
                .Where(r => r.Arity == arity)
                .Where(r => r.ArgTypes.SequenceEqual(requestedArgTypes))
                .Where(r => (!requestedFunc && r.Handler.Method.ReturnType == typeof(void)) ||
                            (requestedFunc && r.Handler.Method.ReturnType == requestedReturn))
                .Select(r => r.Handler)
                .SingleOrDefault() as THandler;

            return handler != null;
        }
        finally
        {
            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// The static instance of the mediator for use in simple apps.
    /// </summary>
    public static MediatorThreadSafe Singleton => _instance.Value;
    private readonly List<HandlerRegistration> _handlers = new();
    private static readonly Lazy<MediatorThreadSafe> _instance = new(() => new MediatorThreadSafe());

    /// <summary>
    /// A thread safe communication primitive. Holds a manifest of registered handler methods, which can
    /// be called using one of the handle methods. Depending on the shape of the input, a handler will be chosen.
    /// </summary>
    public MediatorThreadSafe() { }

    /// <summary>
    /// A thread safe communication primitive. Holds a manifest of registered handler methods, which can
    /// be called using one of the handle methods. Depending on the shape of the input, a handler will be chosen.
    /// </summary>
    public MediatorThreadSafe(MediatorThreadSafe previous) : this(previous._handlers) { }

    /// <summary>
    /// A thread safe communication primitive. Holds a manifest of registered handler methods, which can
    /// be called using one of the handle methods. Depending on the shape of the input, a handler will be chosen.
    /// </summary>
    public MediatorThreadSafe(IEnumerable<HandlerRegistration> registrations) => _handlers = new List<HandlerRegistration>(registrations);

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public TResult Handle<TInput, TResult>(TInput input)
    {
        if (TryGetHandler(HandlerArity.Of1, out Func<TInput, TResult>? handler) && handler != null)
        {
            try
            {
                return handler(input);
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and start a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Task<TResult> StartHandler<TInput, TResult>(TInput input)
    {
        if (TryGetHandler(HandlerArity.Of1, out Func<TInput, TResult>? handler) && handler != null)
        {
            try
            {
                return Task.Run(() => handler(input));
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="input"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void Handle<TInput>(TInput input)
    {
        if (TryGetHandler(HandlerArity.Of1, out Action<TInput>? handler) && handler != null)
        {
            try
            {
                handler(input);
                return;
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and start a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Task StartHandler<TInput>(TInput input)
    {
        if (TryGetHandler(HandlerArity.Of1, out Action<TInput>? handler) && handler != null)
        {
            try
            {
                return Task.Run(() => handler(input));
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task<TResult> HandleAsync<TInput, TResult>(TInput input)
    {
        if (TryGetHandler(HandlerArity.Of1, out Func<TInput, Task<TResult>>? handler) && handler != null)
        {
            try
            {
                return await handler(input);
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public TResult Handle<TInput, TInput2, TResult>(TInput input, TInput2 input2)
    {
        if (TryGetHandler(HandlerArity.Of2, out Func<TInput, TInput2, TResult>? handler) && handler != null)
        {
            try
            {
                return handler(input, input2);
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void Handle<TInput, TInput2>(TInput input, TInput2 input2)
    {
        if (TryGetHandler(HandlerArity.Of2, out Action<TInput, TInput2>? handler) && handler != null)
        {
            try
            {
                handler(input, input2);
                return;
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and start a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Task<TResult> StartHandler<TInput, TInput2, TResult>(TInput input, TInput2 input2)
    {
        if (TryGetHandler(HandlerArity.Of2, out Func<TInput, TInput2, TResult>? handler) && handler != null)
        {
            try
            {
                return Task.Run(() => handler(input, input2));
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and start a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Task StartHandler<TInput, TInput2>(TInput input, TInput2 input2)
    {
        if (TryGetHandler(HandlerArity.Of2, out Action<TInput, TInput2>? handler) && handler != null)
        {
            try
            {
                return Task.Run(() => handler(input, input2));
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task<TResult> HandleAsync<TInput, TInput2, TResult>(TInput input, TInput2 input2)
    {
        if (TryGetHandler(HandlerArity.Of2, out Func<TInput, TInput2, Task<TResult>>? handler) && handler != null)
        {
            try
            {
                return await handler(input, input2);
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TInput3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <param name="input3"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public TResult Handle<TInput, TInput2, TInput3, TResult>(TInput input, TInput2 input2, TInput3 input3)
    {
        if (TryGetHandler(HandlerArity.Of3, out Func<TInput, TInput2, TInput3, TResult>? handler) && handler != null)
        {
            try
            {
                return handler(input, input2, input3);
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TInput3"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <param name="input3"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void Handle<TInput, TInput2, TInput3>(TInput input, TInput2 input2, TInput3 input3)
    {
        if (TryGetHandler(HandlerArity.Of3, out Action<TInput, TInput2, TInput3>? handler) && handler != null)
        {
            try
            {
                handler(input, input2, input3);
                return;
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and start a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TInput3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <param name="input3"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Task<TResult> StartHandler<TInput, TInput2, TInput3, TResult>(TInput input, TInput2 input2, TInput3 input3)
    {
        if (TryGetHandler(HandlerArity.Of3, out Func<TInput, TInput2, TInput3, TResult>? handler) && handler != null)
        {
            try
            {
                return Task.Run(() => handler(input, input2, input3));
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and start a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TInput3"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <param name="input3"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Task StartHandler<TInput, TInput2, TInput3>(TInput input, TInput2 input2, TInput3 input3)
    {
        if (TryGetHandler(HandlerArity.Of3, out Action<TInput, TInput2, TInput3>? handler) && handler != null)
        {
            try
            {
                return Task.Run(() => handler(input, input2, input3));
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Find and invoke a matching handler registered with the mediator.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInput2"></typeparam>
    /// <typeparam name="TInput3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="input"></param>
    /// <param name="input2"></param>
    /// <param name="input3"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task<TResult> HandleAsync<TInput, TInput2, TInput3, TResult>(TInput input, TInput2 input2, TInput3 input3)
    {
        if (TryGetHandler(HandlerArity.Of3, out Func<TInput, TInput2, TInput3, Task<TResult>>? handler) && handler != null)
        {
            try
            {
                return await handler(input, input2, input3);
            }
            catch
            {
                throw;
            }
        }
        throw new NullReferenceException("No handler found for given input!");
    }

    /// <summary>
    /// Register a new handler by infering the supported message types from the passed method parameters.
    /// Delegates with up to 3 arguments are supported.
    /// </summary>
    /// <param name="messageHandler"></param>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(Delegate messageHandler)
    {
        var argTypes = messageHandler.GetArgTypes();
        var arity = (HandlerArity)argTypes.Length;
        if (ExistsHandler(arity, argTypes))
            throw new InvalidOperationException("Handler for specified message type(s) already registered.");

        try
        {
            Monitor.Enter(_lock);
            _handlers.Add(new HandlerRegistration(arity, argTypes, messageHandler));
        }
        finally
        {
            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// Register a new handler by using a type-based key.
    /// All calls to handle methods with arguments of the type matching this key will be routed to the configured delegate.
    /// Delegates with up to 3 arguments are supported.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageHandler"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(Type messageType, Delegate messageHandler)
    {
        var argTypes = new[] { messageType };
        if (ExistsHandler(HandlerArity.Of1, argTypes))
            throw new InvalidOperationException("Handler for specified message type(s) already registered.");

        try
        {
            Monitor.Enter(_lock);
            _handlers.Add(new HandlerRegistration(HandlerArity.Of1, argTypes, messageHandler));
        }
        finally
        {
            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// Register a new handler by using a type-based key.
    /// All calls to handle methods with arguments of the type matching this key will be routed to the configured delegate.
    /// Delegates with up to 3 arguments are supported.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageHandler"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(Type messageType, Type messageType2, Delegate messageHandler)
    {
        var argTypes = new[] { messageType, messageType2 };
        if (ExistsHandler(HandlerArity.Of2, argTypes))
            throw new InvalidOperationException("Handler for specified message types already registered.");

        try
        {
            Monitor.Enter(_lock);
            _handlers.Add(new HandlerRegistration(HandlerArity.Of2, argTypes, messageHandler));
        }
        finally
        {
            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// Register a new handler by using a type-based key.
    /// All calls to handle methods with arguments of the type matching this key will be routed to the configured delegate.
    /// Delegates with up to 3 arguments are supported.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageHandler"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(Type messageType, Type messageType2, Type messageType3, Delegate messageHandler)
    {
        var argTypes = new[] { messageType, messageType2, messageType3 };
        if (ExistsHandler(HandlerArity.Of3, argTypes))
            throw new InvalidOperationException("Handler for specified message types already registered.");

        try
        {
            Monitor.Enter(_lock);
            _handlers.Add(new HandlerRegistration(HandlerArity.Of3, argTypes, messageHandler));
        }
        finally
        {
            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
        }
    }

    private void ReplaceHandler(HandlerArity arity, Delegate newHandler, params Type[] newHandlerArgs)
    {
        if (ExistsHandler(arity, newHandlerArgs))
        {
            this[arity, newHandlerArgs] = new HandlerRegistration(arity, newHandlerArgs, newHandler);
        }
        else throw new InvalidOperationException("No handler for specified message type(s) was registered to be replaced.");
    }

    /// <summary>
    /// Replace the delegate under an existing registration.
    /// </summary>
    /// <param name="currentHandler"></param>
    /// <param name="messageHandler"></param>
    public void ReplaceHandler(Delegate currentHandler, Delegate messageHandler)
    {
        var newArgTypes = currentHandler.GetArgTypes();
        ReplaceHandler((HandlerArity)newArgTypes.Length, messageHandler, newArgTypes);
    }

    /// <summary>
    /// Replace the delegate under an existing registration.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageHandler"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ReplaceHandler(Type messageType, Delegate messageHandler)
    {
        ReplaceHandler(HandlerArity.Of1, messageHandler, messageType);
    }

    /// <summary>
    /// Replace the delegate under an existing registration.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageType2"></param>
    /// <param name="messageHandler"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ReplaceHandler(Type messageType, Type messageType2, Delegate messageHandler)
    {
        ReplaceHandler(HandlerArity.Of2, messageHandler, messageType, messageType2);
    }

    /// <summary>
    /// Replace the delegate under an existing registration.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageType2"></param>
    /// <param name="messageType3"></param>
    /// <param name="messageHandler"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ReplaceHandler(Type messageType, Type messageType2, Type messageType3, Delegate messageHandler)
    {
        ReplaceHandler(HandlerArity.Of3, messageHandler, messageType, messageType2, messageType3);
    }

    /// <summary>
    /// Remove a handler delegate from the manifest.
    /// </summary>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public bool UnregisterHandler(Type messageType)
    {
        if (Monitor.TryEnter(_lock, 200))
        {
            try
            {
                var candidate = _handlers
                    .Where(r => r.ArgTypes.Length == 1
                                && r.ArgTypes[0] == messageType)
                    .SingleOrDefault();

                if (candidate != null) return _handlers.Remove(candidate);
            }
            finally
            {
                if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
            }
        }
        return false;
    }

    /// <summary>
    /// Remove a handler delegate from the manifest.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageType2"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool UnregisterHandler(Type messageType, Type messageType2)
    {
        if (Monitor.TryEnter(_lock, 200))
        {
            try
            {
                var candidate = _handlers
                    .Where(r => r.ArgTypes.Length == 2
                                && r.ArgTypes[0] == messageType
                                && r.ArgTypes[1] == messageType2)
                    .SingleOrDefault();

                if (candidate != null) return _handlers.Remove(candidate);
            }
            finally
            {
                if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
            }
        }
        return false;
    }

    /// <summary>
    /// Remove a handler delegate from the manifest.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="messageType2"></param>
    /// <param name="messageType3"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool UnregisterHandler(Type messageType, Type messageType2, Type messageType3)
    {
        if (Monitor.TryEnter(_lock, 200))
        {
            try
            {
                var candidate = _handlers
                    .Where(r => r.ArgTypes.Length == 3
                                && r.ArgTypes[0] == messageType
                                && r.ArgTypes[1] == messageType2
                                && r.ArgTypes[2] == messageType3)
                    .SingleOrDefault();

                if (candidate != null) return _handlers.Remove(candidate);
            }
            finally
            {
                if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
            }
        }
        return false;
    }

    /// <summary>
    /// Remove a handler delegate from the manifest.
    /// </summary>
    /// <param name="handlerRef"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public bool UnregisterHandlerByRef(Delegate handlerRef)
    {
        if (Monitor.TryEnter(_lock, 200))
        {
            try
            {
                var candidate = _handlers.FirstOrDefault(r => ReferenceEquals(r.Handler, handlerRef));
                if (candidate == null)
                    throw new KeyNotFoundException("Such handler is unknown to this mediator instance.");
                return _handlers.Remove(candidate);
            }
            finally
            {
                if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
            }
        }
        return false;
    }

    /// <summary>
    /// Remove a handler registration and all other registrations whose key's types derive from the passed key.
    /// </summary>
    /// <param name="messageBaseType"></param>
    //public void UnregisterHandlerAndChildren(Type messageBaseType)
    //{

    //    if (Monitor.TryEnter(_lock, 200))
    //    {
    //        try
    //        {
    //            //_handlers.Keys
    //            //    .Where(k => k == messageBaseType || k.DerivesFrom(messageBaseType))
    //            //    .ToList().ForEach(k => _handlers.Remove(k));
    //        }
    //        finally
    //        {
    //            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
    //        }
    //    }
    //}

    /// <summary>
    /// Removes all registrations.
    /// </summary>
    public void Clear()
    {
        try
        {
            Monitor.Enter(_lock);
            _handlers.Clear();
        }
        finally
        {
            if (Monitor.IsEntered(_lock)) Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// Formats a list of registered handlers line by line. Crossplatform.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var med = "";
        var messageTypes = "";
        var copy = _handlers.ToArray();
        for (int v = 0; v < copy.Length; v++)
        {
            messageTypes = "(" + copy[v].ArgTypes.Aggregate("", (a, c) => a + ", " + c).TrimStart(',', ' ') + ")";
            med = $"{med}{Environment.NewLine}{messageTypes} => {copy[v].Handler.Method.Name}";
        }
        return med.Trim();
    }
}