namespace mediator.lib;

public record class HandlerRegistration(HandlerArity Arity, Type[] ArgTypes, Delegate Handler);