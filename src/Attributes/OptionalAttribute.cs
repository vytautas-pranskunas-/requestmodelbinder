using System;

namespace RequestModelBinder.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OptionalAttribute : Attribute
    {
    }
}