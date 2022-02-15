namespace mediator.lib;

internal static class RegistrationHasher
{
    public static int Calculate(MethodDescription md)
    {
        return md.ParameterTypes
            .Aggregate(0, (a, curr) => a ^ curr.GetHashCode())
            ^ md.ReturnType.GetHashCode();
    }
}