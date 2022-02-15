using Xunit;
using System;
using mediator.lib;
using System.Threading.Tasks;

namespace mediator.test;

public class MediatorShould : TestState<int>
{
    public Mediator TestInstance
    {
        get
        {
            return new Mediator(new Delegate[] {
                // actions
                (int a) => { AddCallerName(); Value = a; },
                (int a, int b) => { AddCallerName(); Value = b; },
                (int a, int b, int c) => { AddCallerName(); Value = c; },
                // functions
                (int a) => { AddCallerName(); Value = a; return Value; },
                (int a, int b) => { AddCallerName(); Value = b; return Value; },
                (int a, int b, int c) => { AddCallerName(); Value = c; return Value; },
                // async functions
                (int a) => { AddCallerName(); Value = a; return Task.FromResult(Value); },
                (int a, int b) => { AddCallerName(); Value = b; return Task.FromResult(Value); },
                (int a, int b, int c) => { AddCallerName(); Value = c; return Task.FromResult(Value); }
            });
        }
    }

    public MediatorThreadSafe TestInstanceTS
    {
        get
        {
            return new MediatorThreadSafe(new Delegate[] {
                // actions
                (int a) => { AddCallerName(); Value = a; },
                (int a, int b) => { AddCallerName(); Value = b; },
                (int a, int b, int c) => { AddCallerName(); Value = c; },
                // functions
                (int a) => { AddCallerName(); Value = a; return Value; },
                (int a, int b) => { AddCallerName(); Value = b; return Value; },
                (int a, int b, int c) => { AddCallerName(); Value = c; return Value; },
                // async functions
                (int a) => { AddCallerName(); Value = a; return Task.FromResult(Value); },
                (int a, int b) => { AddCallerName(); Value = b; return Task.FromResult(Value); },
                (int a, int b, int c) => { AddCallerName(); Value = c; return Task.FromResult(Value); }
            });
        }
    }

    [Fact]
    public async Task DisambiguateMethodsByParameters_ReturnSuccessfullyForValidCalls()
    {
        bool looped = false;
        dynamic mediator = TestInstance; test_start:
        mediator.Handle(47); Assert.Equal(47, Value);
        mediator.Handle(47, 49); Assert.Equal(49, Value);
        mediator.Handle(47, 49, 77); Assert.Equal(77, Value);

        Assert.Equal(47, mediator.Handle<int, int>(47));
        Assert.Equal(49, mediator.Handle<int, int, int>(47, 49));
        Assert.Equal(77, mediator.Handle<int, int, int, int>(47, 49, 77));

        await mediator.StartHandler(47); Assert.Equal(47, Value);
        await mediator.StartHandler(47, 49); Assert.Equal(49, Value);
        await mediator.StartHandler(47, 49, 77); Assert.Equal(77, Value);

        Assert.Equal(47, await mediator.StartHandler<int, int>(47));
        Assert.Equal(49, await mediator.StartHandler<int, int, int>(47, 49));
        Assert.Equal(77, await mediator.StartHandler<int, int, int, int>(47, 49, 77));

        Assert.Equal(47, await mediator.HandleAsync<int, int>(47));
        Assert.Equal(49, await mediator.HandleAsync<int, int, int>(47, 49));
        Assert.Equal(77, await mediator.HandleAsync<int, int, int, int>(47, 49, 77));

        if (!looped)
        {
            looped = true;
            mediator = TestInstanceTS;
            goto test_start;
        }
    }
}