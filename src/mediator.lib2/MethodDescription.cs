using ImTools;

namespace mediator.lib2;

internal class MethodDescription : IEquatable<MethodDescription>
{
    public Type ReturnType { get; init; } = typeof(void);
    public Type[] ParameterTypes { get; init; } = Array.Empty<Type>();

    private MethodDescription() { }

    public MethodDescription(Delegate d)
    {
        ReturnType = d.Method.ReturnType ?? typeof(void);
        ParameterTypes = d.Method.GetParameters().Select(p => p.ParameterType).ToArray();
    }

    public static MethodDescription ActOfSig<A>()
    {
        return new MethodDescription() { ReturnType = typeof(void), ParameterTypes = new Type[] { typeof(A) } };
    }

    public static MethodDescription ActOfSig<A,B>()
    {
        return new MethodDescription() { ReturnType = typeof(void), ParameterTypes = new Type[] { typeof(A), typeof(B) } };
    }

    public static MethodDescription ActOfSig<A, B, C>()
    {
        return new MethodDescription() { ReturnType = typeof(void), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C) } };
    }

    public static MethodDescription ActOfSig<A, B, C, D>()
    {
        return new MethodDescription() { ReturnType = typeof(void), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C), typeof(D) } };
    }

    public static MethodDescription ActOfSig<A, B, C, D, E>()
    {
        return new MethodDescription() { ReturnType = typeof(void), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E) } };
    }

    public static MethodDescription ActOfSig<A, B, C, D, E, F>()
    {
        return new MethodDescription() { ReturnType = typeof(void), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F) } };
    }

    public static MethodDescription FuncOfSig<A,R>()
    {
        return new MethodDescription() { ReturnType = typeof(R), ParameterTypes = new Type[] { typeof(A) } };
    }
    public static MethodDescription FuncOfSig<A, B, R>()
    {
        return new MethodDescription() { ReturnType = typeof(R), ParameterTypes = new Type[] { typeof(A), typeof(B) } };
    }

    public static MethodDescription FuncOfSig<A, B, C, R>()
    {
        return new MethodDescription() { ReturnType = typeof(R), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C) } };
    }

    public static MethodDescription FuncOfSig<A, B, C, D, R>()
    {
        return new MethodDescription() { ReturnType = typeof(R), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C), typeof(D) } };
    }

    public static MethodDescription FuncOfSig<A, B, C, D, E, R>()
    {
        return new MethodDescription() { ReturnType = typeof(R), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E) } };
    }

    public static MethodDescription FuncOfSig<A, B, C, D, E, F, R>()
    {
        return new MethodDescription() { ReturnType = typeof(R), ParameterTypes = new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F) } };
    }

    #region equality
    public override bool Equals(object? obj)
    {
        if (obj is MethodDescription other)
            return Equals(other);
        else return false;
    }

    public override int GetHashCode()
    {
        return RegistrationHasher.Calculate(this);
    }

    public bool Equals(MethodDescription? other)
    {
        if (other == null) return false;
        else if (ReferenceEquals(this, other)) return true;
        else if (other.ReturnType != ReturnType) return false;
        else if (other.ParameterTypes.Length != ParameterTypes.Length) return false;
        else if (!other.ParameterTypes.SequenceEqual(ParameterTypes)) return false;

        return true;
    }
    #endregion
}
