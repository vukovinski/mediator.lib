namespace mediator.lib;

internal static class Helpers
{
    public static bool DerivesFrom(this Type? child, Type parent)
    {
        if (child == null) return false;
        if (child == parent) return false;
        if (child.BaseType == parent) return true;
        if (DerivesFrom(child.BaseType, parent)) return true;
        return false;
    }

    public static Type[] GetArgTypes(this Delegate handler)
    {
        return handler.Method.GetParameters().Select(p => p.ParameterType).ToArray();
    }
}
