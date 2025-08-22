// Lower numbers initialize first
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleOrderAttribute : Attribute
{
    [Header("Lower numbers initialize first")]
    [SerializeField] public int Order { get; }
    public ModuleOrderAttribute(int order) => Order = order;
}
