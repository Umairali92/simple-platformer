using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequireModuleAttribute : Attribute
{
    public Type ModuleType { get; }
    public RequireModuleAttribute(Type moduleType) => ModuleType = moduleType;
}